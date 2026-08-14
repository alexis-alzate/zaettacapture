(function () {
  "use strict";

  var counterApiUrl = "https://ocnoiraaqosfmbluccba.supabase.co/functions/v1/download-counter";
  var githubApiUrl = "https://api.github.com/repos/alexis-alzate/zaettacapture/releases?per_page=100";
  var counter = document.querySelector("[data-download-count]");
  var status = document.querySelector("[data-download-status]");
  var refreshButton = document.querySelector("[data-download-refresh]");
  var counterCard = document.querySelector(".download-counter");
  var miniSignal = document.querySelector("[data-download-mini]");
  var miniStatus = document.querySelector("[data-mini-status]");
  var miniCounters = document.querySelectorAll("[data-download-count-mini]");
  var downloadLinks = document.querySelectorAll('a[href*="/ZaettaCaptureSetup.exe"]');

  if (!counter || !status || !refreshButton || !counterCard) {
    return;
  }

  counterCard.classList.add("counter-enhanced");

  var fallbackValue = Number(counter.textContent.replace(/\D/g, "")) || 0;
  var prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  var numberFormatter = new Intl.NumberFormat("es-CO");
  var animationFrame = null;
  var lastKnownValue = fallbackValue;
  var refreshSerial = 0;
  var activeController = null;

  function setStatus(message, state) {
    status.textContent = message;
    counterCard.dataset.state = state;

    if (miniSignal) {
      miniSignal.dataset.state = state;
    }

    if (miniStatus) {
      if (state === "loading") {
        miniStatus.textContent = "Sincronizando";
      } else if (state === "live") {
        miniStatus.textContent = "En vivo";
      } else {
        miniStatus.textContent = "Último dato";
      }
    }
  }

  function renderCount(value) {
    var formattedValue = numberFormatter.format(value);
    counter.textContent = formattedValue;

    miniCounters.forEach(function (miniCounter) {
      miniCounter.textContent = formattedValue;
    });

    if (miniSignal) {
      miniSignal.setAttribute("aria-label", formattedValue + " descargas y contando. Actualizar contador.");
    }
  }

  function animateCount(target) {
    if (animationFrame) {
      window.cancelAnimationFrame(animationFrame);
    }

    var displayedValue = Number(counter.textContent.replace(/\D/g, "")) || 0;
    var safeTarget = Math.max(Math.round(Number(target) || 0), displayedValue, lastKnownValue);
    lastKnownValue = safeTarget;

    if (prefersReducedMotion || safeTarget === displayedValue) {
      renderCount(safeTarget);
      return;
    }

    var startValue = displayedValue;
    var startedAt = null;
    var duration = 700;

    function update(timestamp) {
      if (!startedAt) {
        startedAt = timestamp;
      }

      var progress = Math.min((timestamp - startedAt) / duration, 1);
      var eased = 1 - Math.pow(1 - progress, 4);
      var current = Math.round(startValue + (safeTarget - startValue) * eased);
      renderCount(current);

      if (progress < 1) {
        animationFrame = window.requestAnimationFrame(update);
      } else {
        animationFrame = null;
      }
    }

    animationFrame = window.requestAnimationFrame(update);
  }

  function parseCounterResponse(data) {
    var value = Number(data && data.count);

    if (!Number.isSafeInteger(value) || value < 0) {
      throw new Error("Invalid counter response");
    }

    return value;
  }

  async function fetchLiveCount(signal) {
    var response = await fetch(counterApiUrl, {
      method: "GET",
      headers: { Accept: "application/json" },
      cache: "no-store",
      signal: signal
    });

    if (!response.ok) {
      throw new Error("Counter API responded with " + response.status);
    }

    return parseCounterResponse(await response.json());
  }

  function getGitHubDownloadTotal(releases) {
    return releases.reduce(function (releaseTotal, release) {
      if (release.draft || release.prerelease || !Array.isArray(release.assets)) {
        return releaseTotal;
      }

      return releaseTotal + release.assets.reduce(function (assetTotal, asset) {
        var isInstaller = asset.name === "ZaettaCaptureSetup.exe";
        return assetTotal + (isInstaller ? Number(asset.download_count) || 0 : 0);
      }, 0);
    }, 0);
  }

  async function fetchGitHubFallback(signal) {
    var response = await fetch(githubApiUrl, {
      headers: { Accept: "application/vnd.github+json" },
      cache: "no-store",
      signal: signal
    });

    if (!response.ok) {
      throw new Error("GitHub API responded with " + response.status);
    }

    var total = getGitHubDownloadTotal(await response.json());

    if (!total) {
      throw new Error("No installer downloads found");
    }

    return total;
  }

  async function refreshCount(options) {
    var settings = options || {};
    var requestId = ++refreshSerial;

    if (activeController) {
      activeController.abort();
    }

    activeController = "AbortController" in window ? new AbortController() : null;

    if (!settings.silent) {
      refreshButton.disabled = true;
      if (miniSignal) {
        miniSignal.disabled = true;
      }
      setStatus("Sincronizando contador en tiempo real...", "loading");
    }

    try {
      var total = await fetchLiveCount(activeController ? activeController.signal : undefined);

      if (requestId !== refreshSerial) {
        return;
      }

      animateCount(total);
      setStatus("En vivo · se actualiza con cada descarga", "live");
    } catch (error) {
      if (requestId !== refreshSerial || error.name === "AbortError") {
        return;
      }

      try {
        var fallbackTotal = await fetchGitHubFallback(activeController ? activeController.signal : undefined);

        if (requestId !== refreshSerial) {
          return;
        }

        animateCount(fallbackTotal);
        setStatus("Respaldo activo · última cifra verificada", "fallback");
      } catch (fallbackError) {
        if (requestId !== refreshSerial || fallbackError.name === "AbortError") {
          return;
        }

        renderCount(lastKnownValue);
        setStatus("Mostrando la última cifra verificada", "fallback");
      }
    } finally {
      if (requestId === refreshSerial) {
        activeController = null;
        refreshButton.disabled = false;
        if (miniSignal) {
          miniSignal.disabled = false;
        }
      }
    }
  }

  async function incrementDownloadCounter() {
    var response = await fetch(counterApiUrl, {
      method: "POST",
      headers: { Accept: "application/json" },
      cache: "no-store",
      keepalive: true
    });

    if (!response.ok) {
      throw new Error("Counter API responded with " + response.status);
    }

    var total = parseCounterResponse(await response.json());
    animateCount(total);
    setStatus("Descarga registrada · contador actualizado", "live");
    return total;
  }

  function startTrackedDownload(downloadUrl) {
    var downloadStarted = false;

    function startDownload() {
      if (downloadStarted) {
        return;
      }

      downloadStarted = true;
      window.location.assign(downloadUrl);
    }

    setStatus("Registrando descarga...", "loading");
    var safetyTimer = window.setTimeout(startDownload, 900);

    incrementDownloadCounter().catch(function () {
      setStatus("La descarga continúa · reconectando contador", "fallback");
    }).finally(function () {
      window.clearTimeout(safetyTimer);
      startDownload();
    });
  }

  downloadLinks.forEach(function (downloadLink) {
    downloadLink.addEventListener("click", function (event) {
      if (event.defaultPrevented || event.button !== 0) {
        return;
      }

      if (event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
        incrementDownloadCounter().catch(function () {});
        return;
      }

      event.preventDefault();
      startTrackedDownload(downloadLink.href);
    });
  });

  counterCard.addEventListener("pointermove", function (event) {
    var bounds = counterCard.getBoundingClientRect();
    counterCard.style.setProperty("--pointer-x", event.clientX - bounds.left + "px");
    counterCard.style.setProperty("--pointer-y", event.clientY - bounds.top + "px");
  });

  counterCard.addEventListener("pointerleave", function () {
    counterCard.style.removeProperty("--pointer-x");
    counterCard.style.removeProperty("--pointer-y");
  });

  refreshButton.addEventListener("click", function () {
    refreshCount();
  });

  if (miniSignal) {
    miniSignal.addEventListener("click", function () {
      refreshCount();
    });

    miniSignal.addEventListener("pointermove", function (event) {
      var bounds = miniSignal.getBoundingClientRect();
      miniSignal.style.setProperty("--signal-x", event.clientX - bounds.left + "px");
      miniSignal.style.setProperty("--signal-y", event.clientY - bounds.top + "px");
    });

    miniSignal.addEventListener("pointerleave", function () {
      miniSignal.style.removeProperty("--signal-x");
      miniSignal.style.removeProperty("--signal-y");
    });
  }

  if ("IntersectionObserver" in window) {
    var observer = new IntersectionObserver(function (entries) {
      if (entries[0].isIntersecting) {
        counterCard.classList.add("is-visible");
        observer.disconnect();
      }
    }, { threshold: 0.35 });

    observer.observe(counterCard);
  } else {
    counterCard.classList.add("is-visible");
  }

  document.addEventListener("visibilitychange", function () {
    if (!document.hidden) {
      refreshCount({ silent: true });
    }
  });

  window.setInterval(function () {
    if (!document.hidden) {
      refreshCount({ silent: true });
    }
  }, 15000);

  refreshCount();
}());

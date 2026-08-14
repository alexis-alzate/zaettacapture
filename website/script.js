(function () {
  "use strict";

  var apiUrl = "https://api.github.com/repos/alexis-alzate/zaettacapture/releases?per_page=100";
  var counter = document.querySelector("[data-download-count]");
  var status = document.querySelector("[data-download-status]");
  var refreshButton = document.querySelector("[data-download-refresh]");
  var counterCard = document.querySelector(".download-counter");
  var miniSignal = document.querySelector("[data-download-mini]");
  var miniStatus = document.querySelector("[data-mini-status]");
  var miniCounters = document.querySelectorAll("[data-download-count-mini]");

  if (!counter || !status || !refreshButton || !counterCard) {
    return;
  }

  counterCard.classList.add("counter-enhanced");

  var fallbackValue = Number(counter.textContent.replace(/\D/g, "")) || 0;
  var prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  var numberFormatter = new Intl.NumberFormat("es-CO");
  var animationFrame = null;

  function setStatus(message, state) {
    status.textContent = message;
    counterCard.dataset.state = state;

    if (miniSignal) {
      miniSignal.dataset.state = state;
    }

    if (miniStatus) {
      miniStatus.textContent = state === "loading" ? "Actualizando" : "Comunidad activa";
    }
  }

  function renderCount(value) {
    var formattedValue = numberFormatter.format(value);
    counter.textContent = formattedValue;

    miniCounters.forEach(function (miniCounter) {
      miniCounter.textContent = formattedValue;
    });

    if (miniSignal) {
      miniSignal.setAttribute("aria-label", formattedValue + " descargas verificadas. Actualizar contador.");
    }
  }

  function animateCount(target) {
    if (animationFrame) {
      window.cancelAnimationFrame(animationFrame);
    }

    if (prefersReducedMotion) {
      renderCount(target);
      return;
    }

    var startValue = Number(counter.textContent.replace(/\D/g, "")) || 0;
    var startedAt = null;
    var duration = 1200;

    function update(timestamp) {
      if (!startedAt) {
        startedAt = timestamp;
      }

      var progress = Math.min((timestamp - startedAt) / duration, 1);
      var eased = 1 - Math.pow(1 - progress, 4);
      var current = Math.round(startValue + (target - startValue) * eased);
      renderCount(current);

      if (progress < 1) {
        animationFrame = window.requestAnimationFrame(update);
      }
    }

    animationFrame = window.requestAnimationFrame(update);
  }

  function getDownloadTotal(releases) {
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

  async function refreshCount() {
    refreshButton.disabled = true;
    if (miniSignal) {
      miniSignal.disabled = true;
    }
    setStatus("Actualizando desde GitHub...", "loading");

    try {
      var response = await fetch(apiUrl, {
        headers: { Accept: "application/vnd.github+json" }
      });

      if (!response.ok) {
        throw new Error("GitHub API responded with " + response.status);
      }

      var releases = await response.json();
      var total = getDownloadTotal(releases);

      if (!total) {
        throw new Error("No installer downloads found");
      }

      animateCount(total);
      setStatus("Contador en vivo actualizado", "live");
    } catch (error) {
      animateCount(fallbackValue);
      setStatus("Mostrando la última cifra verificada", "fallback");
    } finally {
      refreshButton.disabled = false;
      if (miniSignal) {
        miniSignal.disabled = false;
      }
    }
  }

  counterCard.addEventListener("pointermove", function (event) {
    var bounds = counterCard.getBoundingClientRect();
    counterCard.style.setProperty("--pointer-x", event.clientX - bounds.left + "px");
    counterCard.style.setProperty("--pointer-y", event.clientY - bounds.top + "px");
  });

  counterCard.addEventListener("pointerleave", function () {
    counterCard.style.removeProperty("--pointer-x");
    counterCard.style.removeProperty("--pointer-y");
  });

  refreshButton.addEventListener("click", refreshCount);

  if (miniSignal) {
    miniSignal.addEventListener("click", refreshCount);

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
        refreshCount();
        observer.disconnect();
      }
    }, { threshold: 0.35 });

    observer.observe(counterCard);
  } else {
    counterCard.classList.add("is-visible");
    refreshCount();
  }
}());

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

  document.addEventListener("zaetta:download-started", function () {
    setStatus("Registrando descarga en el servidor...", "loading");

    [900, 2200, 5000].forEach(function (delay) {
      window.setTimeout(function () {
        refreshCount({ silent: true });
      }, delay);
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
  }, 5000);

  refreshCount();
}());

(function () {
  "use strict";

  var downloadApiUrl = "https://ocnoiraaqosfmbluccba.supabase.co/functions/v1/download-counter";
  var gate = document.querySelector("[data-download-gate]");
  var form = document.querySelector("[data-download-form]");
  var downloadLinks = document.querySelectorAll("[data-download-track]");

  if (!gate || !form || !downloadLinks.length) {
    return;
  }

  var closeButton = gate.querySelector("[data-download-close]");
  var emailField = form.querySelector('input[name="email"]');
  var submitButton = form.querySelector(".download-gate-submit");
  var buttonLabel = form.querySelector("[data-download-button-label]");
  var formStatus = form.querySelector("[data-download-form-status]");
  var fallbackLink = form.querySelector("[data-download-fallback]");

  function setFormStatus(message, state) {
    formStatus.textContent = message;
    formStatus.dataset.state = state;
  }

  function showGate() {
    fallbackLink.hidden = true;
    fallbackLink.removeAttribute("href");
    setFormStatus("", "");

    if (typeof gate.showModal === "function") {
      if (!gate.open) gate.showModal();
    } else {
      gate.setAttribute("open", "");
    }

    window.setTimeout(function () {
      emailField.focus();
    }, 80);
  }

  function closeGate() {
    if (typeof gate.close === "function") {
      gate.close();
    } else {
      gate.removeAttribute("open");
    }
  }

  function safeDownloadUrl(value) {
    try {
      var url = new URL(String(value || ""));
      var expectedPath = "/functions/v1/download-counter";
      var valid = url.protocol === "https:" &&
        url.hostname === "ocnoiraaqosfmbluccba.supabase.co" &&
        url.pathname === expectedPath &&
        Boolean(url.searchParams.get("token"));
      return valid ? url.toString() : "";
    } catch (error) {
      return "";
    }
  }

  downloadLinks.forEach(function (downloadLink) {
    downloadLink.addEventListener("click", function (event) {
      event.preventDefault();
      showGate();
    });
  });

  closeButton.addEventListener("click", closeGate);

  gate.addEventListener("click", function (event) {
    if (event.target === gate) {
      closeGate();
    }
  });

  form.addEventListener("submit", async function (event) {
    event.preventDefault();

    if (!form.checkValidity()) {
      form.reportValidity();
      return;
    }

    var formData = new FormData(form);
    var payload = {
      email: String(formData.get("email") || "").trim(),
      marketingConsent: formData.get("marketingConsent") === "on",
      privacyAccepted: formData.get("privacyAccepted") === "on",
      website: String(formData.get("website") || "")
    };

    submitButton.disabled = true;
    buttonLabel.textContent = "Preparando...";
    fallbackLink.hidden = true;
    setFormStatus("Guardando tu correo de forma segura.", "loading");

    var controller = new AbortController();
    var requestTimeout = window.setTimeout(function () {
      controller.abort();
    }, 15000);

    try {
      var response = await fetch(downloadApiUrl, {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json"
        },
        body: JSON.stringify(payload),
        signal: controller.signal
      });
      var result = await response.json().catch(function () {
        return {};
      });

      if (!response.ok) {
        throw new Error(result.error || "No pudimos preparar tu descarga.");
      }

      var downloadUrl = safeDownloadUrl(result.downloadUrl);
      if (!downloadUrl) {
        throw new Error("No pudimos generar un enlace de descarga válido.");
      }

      fallbackLink.href = downloadUrl;
      fallbackLink.hidden = false;
      setFormStatus("¡Listo! La descarga comenzará automáticamente.", "success");
      document.dispatchEvent(new CustomEvent("zaetta:download-started"));

      window.setTimeout(function () {
        window.location.assign(downloadUrl);
      }, 350);
    } catch (error) {
      var message = error && error.name === "AbortError"
        ? "La conexión tardó demasiado. Inténtalo nuevamente."
        : (error && error.message) || "No pudimos preparar tu descarga. Inténtalo nuevamente.";
      setFormStatus(message, "error");
    } finally {
      window.clearTimeout(requestTimeout);
      submitButton.disabled = false;
      buttonLabel.textContent = "Obtener descarga";
    }
  });

  var pageUrl = new URL(window.location.href);
  if (pageUrl.searchParams.get("descargar") === "1") {
    pageUrl.searchParams.delete("descargar");
    window.history.replaceState({}, "", pageUrl.pathname + pageUrl.search + pageUrl.hash);
    window.setTimeout(showGate, 120);
  }
}());

(function () {
  "use strict";

  var ideasMenu = document.querySelector("[data-ideas-menu]");
  var ideasSection = document.querySelector("#ideas");
  var launcher = document.querySelector("[data-feedback-launcher]");
  var feedbackForm = document.querySelector("[data-feedback-form]");

  if (!ideasMenu) {
    return;
  }

  var menuTrigger = ideasMenu.querySelector(".nav-ideas-trigger");
  var menuLinks = ideasMenu.querySelectorAll("a");
  var categoryLinks = ideasMenu.querySelectorAll("[data-feedback-category]");
  var categoryField = feedbackForm ? feedbackForm.querySelector('select[name="category"]') : null;
  var messageField = feedbackForm ? feedbackForm.querySelector('textarea[name="message"]') : null;

  function setMenuOpen(open) {
    ideasMenu.dataset.open = open ? "true" : "false";
    menuTrigger.setAttribute("aria-expanded", open ? "true" : "false");
  }

  menuTrigger.addEventListener("click", function () {
    setMenuOpen(ideasMenu.dataset.open !== "true");
  });

  document.addEventListener("click", function (event) {
    if (!ideasMenu.contains(event.target)) {
      setMenuOpen(false);
    }
  });

  document.addEventListener("keydown", function (event) {
    if (event.key === "Escape" && ideasMenu.dataset.open === "true") {
      setMenuOpen(false);
      menuTrigger.focus();
    }
  });

  menuLinks.forEach(function (link) {
    link.addEventListener("click", function () {
      setMenuOpen(false);
    });
  });

  categoryLinks.forEach(function (link) {
    link.addEventListener("click", function () {
      if (!categoryField) {
        return;
      }

      categoryField.value = link.dataset.feedbackCategory;
      window.setTimeout(function () {
        if (messageField) {
          messageField.focus({ preventScroll: true });
        }
      }, 650);
    });
  });

  if (launcher && ideasSection && "IntersectionObserver" in window) {
    var launcherObserver = new IntersectionObserver(function (entries) {
      launcher.classList.toggle("is-hidden", entries[0].isIntersecting);
    }, { threshold: 0.12 });

    launcherObserver.observe(ideasSection);
  }
}());

(function () {
  "use strict";

  var feedbackApiUrl = "https://ocnoiraaqosfmbluccba.supabase.co/functions/v1/product-feedback";
  var form = document.querySelector("[data-feedback-form]");

  if (!form) {
    return;
  }

  var messageField = form.querySelector('textarea[name="message"]');
  var characterCount = form.querySelector("[data-feedback-count]");
  var submitButton = form.querySelector(".feedback-submit");
  var buttonLabel = form.querySelector("[data-feedback-button-label]");
  var feedbackStatus = form.querySelector("[data-feedback-status]");

  function updateCharacterCount() {
    characterCount.textContent = String(messageField.value.length);
  }

  function setFeedbackStatus(message, state) {
    feedbackStatus.textContent = message;
    feedbackStatus.dataset.state = state;
  }

  messageField.addEventListener("input", updateCharacterCount);

  form.addEventListener("submit", async function (event) {
    event.preventDefault();

    if (!form.checkValidity()) {
      form.reportValidity();
      return;
    }

    var formData = new FormData(form);
    var payload = {
      name: String(formData.get("name") || "").trim(),
      email: String(formData.get("email") || "").trim(),
      category: String(formData.get("category") || ""),
      message: String(formData.get("message") || "").trim(),
      privacyAccepted: formData.get("privacyAccepted") === "on",
      website: String(formData.get("website") || "")
    };

    submitButton.disabled = true;
    buttonLabel.textContent = "Enviando...";
    setFeedbackStatus("Estamos guardando tu propuesta de forma segura.", "loading");

    try {
      var response = await fetch(feedbackApiUrl, {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json"
        },
        body: JSON.stringify(payload)
      });

      var result = await response.json().catch(function () {
        return {};
      });

      if (!response.ok) {
        throw new Error(result.error || "No pudimos enviar tu propuesta.");
      }

      form.reset();
      updateCharacterCount();
      setFeedbackStatus("¡Idea recibida! Gracias por ayudarnos a construir una mejor Zaetta.", "success");
    } catch (error) {
      setFeedbackStatus(error.message || "No pudimos enviar tu propuesta. Inténtalo nuevamente.", "error");
    } finally {
      submitButton.disabled = false;
      buttonLabel.textContent = "Enviar idea";
    }
  });
}());

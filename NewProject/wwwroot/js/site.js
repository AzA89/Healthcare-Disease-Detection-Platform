// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Initialize all components when the DOM is loaded
document.addEventListener("DOMContentLoaded", () => {
  // Initialize tooltips
  const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
  const tooltipList = tooltipTriggerList.map((tooltipTriggerEl) => new bootstrap.Tooltip(tooltipTriggerEl));

  // Initialize popovers
  const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
  const popoverList = popoverTriggerList.map((popoverTriggerEl) => new bootstrap.Popover(popoverTriggerEl));

  // Smooth scrolling for anchor links
  document.querySelectorAll('a[href^="#"]:not([data-bs-toggle])').forEach((anchor) => {
    anchor.addEventListener("click", function (e) {
      e.preventDefault();
      
      const targetId = this.getAttribute("href");
      if (targetId === "#") return;

      const targetElement = document.querySelector(targetId);
      if (targetElement) {
        targetElement.scrollIntoView({
          behavior: "smooth",
          block: "start"
        });
      }
    });
  });

  // Back to top button
  const backToTopButton = document.getElementById("backToTop");
  if (backToTopButton) {
    // Show/hide button based on scroll position
    window.addEventListener("scroll", () => {
      if (window.scrollY > 300) {
        backToTopButton.classList.add("show");
      } else {
        backToTopButton.classList.remove("show");
      }
    });
    
    backToTopButton.addEventListener("click", (e) => {
      e.preventDefault();
      window.scrollTo({
        top: 0,
        behavior: "smooth",
      });
    });
  }

  // Form validation enhancement
  const forms = document.querySelectorAll(".needs-validation");
  Array.from(forms).forEach((form) => {
    form.addEventListener(
      "submit",
      (event) => {
        if (!form.checkValidity()) {
          event.preventDefault();
          event.stopPropagation();
          
          // Scroll to first invalid element
          const firstInvalid = form.querySelector(":invalid");
          if (firstInvalid) {
            firstInvalid.focus();
            firstInvalid.scrollIntoView({
              behavior: "smooth",
              block: "center"
            });
          }
        }
        form.classList.add("was-validated");
      },
      false,
    );
  });

  // Print functionality
  const printButtons = document.querySelectorAll('[onclick="window.print()"]');
  Array.from(printButtons).forEach((button) => {
    button.addEventListener("click", (e) => {
      e.preventDefault();
      window.print();
    });
  });

  // Tab keyboard navigation
  document.addEventListener("keydown", (e) => {
    if (e.altKey && e.key === "1") {
      const heartTab = document.getElementById("heart-tab");
      if (heartTab) {
        heartTab.click();
      }
    } else if (e.altKey && e.key === "2") {
      const diabetesTab = document.getElementById("diabetes-tab");
      if (diabetesTab) {
        diabetesTab.click();
      }
    }
  });
  
  // Fix dropdown menus on mobile
  const dropdownMenus = document.querySelectorAll('.dropdown-menu');
  if (window.innerWidth < 992) {
    dropdownMenus.forEach(menu => {
      menu.classList.add('dropdown-menu-end');
    });
  }
  
  // Initialize any carousels with auto-sliding disabled on hover
  const carousels = document.querySelectorAll('.carousel');
  carousels.forEach(carousel => {
    const carouselInstance = new bootstrap.Carousel(carousel, {
      interval: 5000 // 5 seconds between slides
    });
    
    // Pause carousel on hover
    carousel.addEventListener('mouseenter', () => {
      carouselInstance.pause();
    });
    
    carousel.addEventListener('mouseleave', () => {
      carouselInstance.cycle();
    });
  });

  // Remove login/register elements
  // removeLoginRegisterElements();
});

// Function to format numbers with commas
function formatNumber(number) {
  return number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

// Function to validate form inputs
function validateInput(input, min, max) {
  const value = Number.parseFloat(input.value);
  if (isNaN(value) || value < min || value > max) {
    input.classList.add("is-invalid");
    return false;
  } else {
    input.classList.remove("is-invalid");
    input.classList.add("is-valid");
    return true;
  }
}

// Function to reset form validation states
function resetFormValidation(form) {
  const inputs = form.querySelectorAll("input, select, textarea");
  inputs.forEach((input) => {
    input.classList.remove("is-invalid", "is-valid");
  });
  form.classList.remove("was-validated");
}

// Function to show loading spinner
function showLoading(containerId) {
  const container = document.getElementById(containerId);
  if (container) {
    container.innerHTML = `
      <div class="text-center py-4">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
        <p class="mt-2">Processing your request...</p>
      </div>
    `;
  }
}

// Function to hide loading spinner
function hideLoading(containerId) {
  const container = document.getElementById(containerId);
  if (container) {
    container.innerHTML = "";
  }
}

// Function to show error message
function showError(containerId, message) {
  const container = document.getElementById(containerId);
  if (container) {
    container.innerHTML = `
      <div class="alert alert-danger">
        <i class="fas fa-exclamation-circle me-2"></i>
        ${message}
      </div>
    `;
  }
}

// Function to show success message
function showSuccess(containerId, message) {
  const container = document.getElementById(containerId);
  if (container) {
    container.innerHTML = `
      <div class="alert alert-success">
        <i class="fas fa-check-circle me-2"></i>
        ${message}
      </div>
    `;
  }
}

// Responsive image loading (lazy loading)
if ('loading' in HTMLImageElement.prototype) {
  // Browser supports native lazy loading
  const lazyImages = document.querySelectorAll('img[loading="lazy"]');
  lazyImages.forEach(img => {
    img.src = img.dataset.src;
  });
} else {
  // Fallback for browsers that don't support lazy loading
  const lazyImageObserver = new IntersectionObserver((entries, observer) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        const lazyImage = entry.target;
        lazyImage.src = lazyImage.dataset.src;
        lazyImageObserver.unobserve(lazyImage);
      }
    });
  });
  
  const lazyImages = document.querySelectorAll('img[data-src]');
  lazyImages.forEach(img => {
    lazyImageObserver.observe(img);
  });
}

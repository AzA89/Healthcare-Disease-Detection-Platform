// Theme toggling functionality
document.addEventListener("DOMContentLoaded", () => {
  // Check for saved theme preference or use device preference
  const savedTheme = localStorage.getItem("theme")
  const prefersDarkScheme = window.matchMedia("(prefers-color-scheme: dark)").matches

  // Apply theme based on saved preference or device setting
  if (savedTheme === "dark" || (!savedTheme && prefersDarkScheme)) {
    document.documentElement.classList.add("dark")
    updateThemeToggle(true)
  } else {
    updateThemeToggle(false)
  }

  // Handle theme toggle click
  const themeToggler = document.getElementById("theme-toggle")
  if (themeToggler) {
    themeToggler.addEventListener("click", () => {
      const isDark = document.documentElement.classList.toggle("dark")
      localStorage.setItem("theme", isDark ? "dark" : "light")
      updateThemeToggle(isDark)

      // Update server-side preference using form submit
      document.getElementById("theme-value").value = isDark ? "dark" : "light"
      document.getElementById("theme-form").submit()
    })
  }

  function updateThemeToggle(isDark) {
    const moonIcon = document.getElementById("moon-icon")
    const sunIcon = document.getElementById("sun-icon")

    if (moonIcon && sunIcon) {
      moonIcon.classList.toggle("d-none", isDark)
      sunIcon.classList.toggle("d-none", !isDark)
    }
  }
})


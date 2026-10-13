// Scroll Reveal
const reveals = document.querySelectorAll(".reveal");
const observer = new IntersectionObserver(entries => {
  entries.forEach(entry => {
    if (entry.isIntersecting) entry.target.classList.add("active");
  });
}, { threshold: 0.15 });

reveals.forEach(r => observer.observe(r));

// Active Nav Link
const sections = document.querySelectorAll("section");
const navLinks = document.querySelectorAll("nav a");

window.addEventListener("scroll", () => {
  let current = "";
  sections.forEach(section => {
    const sectionTop = section.offsetTop - 150;
    if (scrollY >= sectionTop) current = section.id;
  });

  navLinks.forEach(a => {
    a.classList.remove("active");
    if (a.getAttribute("href") === `#${current}`) a.classList.add("active");
  });
});

// Theme toggle (unchanged)
const toggle = document.getElementById("themeToggle");
const root = document.documentElement;
const saved = localStorage.getItem("theme");
if (saved) root.setAttribute("data-theme", saved);

toggle.onclick = () => {
  const t = root.getAttribute("data-theme") === "dark" ? "light" : "dark";
  root.setAttribute("data-theme", t);
  localStorage.setItem("theme", t);
};
/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./Views/**/*.{cshtml,html}",
        "./Pages/**/*.{cshtml,html}",      // if you add Razor Pages later
        "./*.cshtml",                      // root views like _ViewStart, etc.
        "./wwwroot/**/*.{js,html}"         // any scripts/HTML in wwwroot
    ],
    theme: { extend: {} },
    plugins: [],
    darkMode: "class"
};
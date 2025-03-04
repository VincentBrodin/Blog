/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        './wwwroot/**/*.{html, js}',
        './views/**/*.hbs'
    ],
    theme: {
    },
    plugins: [
        require('@tailwindcss/typography'),
        require('daisyui'),
    ],
    daisyui: {
        themes: [
            "retro",
            "dim",
        ],
    }
}

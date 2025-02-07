/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        './wwwroot/**/*.{html, js}',
        './views/**/*.hbs'
    ],
    theme: {
        extend: {
            fontFamily: {
                sans: ['InterVariable', '...defaultTheme.fontFamily.sans'],
            },
        },
    },
    plugins: [
        require('@tailwindcss/typography'),
        require('daisyui'),
    ],
    daisyui: {
        themes: [
            "light",
            "dark",
        ],
    }
}
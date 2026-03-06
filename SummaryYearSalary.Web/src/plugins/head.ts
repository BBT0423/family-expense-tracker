import { createHead } from '@vueuse/head';

export const head = createHead({
  titleTemplate: (title) => (title ? `${title} | SummaryYearSalary` : 'SummaryYearSalary'),
  meta: [
    {
      name: 'referrer',
      content: 'no-referrer',
    },
    {
      'http-equiv': 'Content-Security-Policy',
      content:
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self' data:; connect-src 'self' https:; base-uri 'self'; form-action 'self'",
    },
  ],
});

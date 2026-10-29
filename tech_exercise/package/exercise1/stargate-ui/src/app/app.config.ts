import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { PersonRepository } from '@abstractions/person.repository';
import { API_BASE_URL } from '@api/stargate-api';
import { ApiPersonRepository } from '@business/api-person.repository';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(),
    // Empty base URL: requests go to the dev server, which proxies them to the API (proxy.conf.json).
    { provide: API_BASE_URL, useValue: '' },
    { provide: PersonRepository, useClass: ApiPersonRepository },
  ],
};

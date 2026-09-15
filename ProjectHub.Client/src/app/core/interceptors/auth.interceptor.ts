import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { AuthService } from '../services/auth.service';

let isRefreshing = false;

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();
  const isApiUrl = req.url.startsWith(environment.apiUrl);
  const isAuthEndpoint = req.url.includes('/api/auth/');

  let clonedRequest = req;

  // Inyectar Bearer token únicamente a peticiones dirigidas a nuestra API
  // omitiendo los endpoints de login/register/refresh
  if (token && isApiUrl && !isAuthEndpoint) {
    clonedRequest = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(clonedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      // Manejar expiración de token (401 Unauthorized)
      if (error.status === 401 && isApiUrl && !isAuthEndpoint) {
        return handle401Error(clonedRequest, next, authService);
      }

      return throwError(() => error);
    })
  );
};

function handle401Error(
  req: Parameters<HttpInterceptorFn>[0],
  next: Parameters<HttpInterceptorFn>[1],
  authService: AuthService
) {
  if (!isRefreshing) {
    isRefreshing = true;

    return authService.refreshToken().pipe(
      switchMap(response => {
        isRefreshing = false;
        // Reintentar la solicitud original con el nuevo token obtenido
        const newRequest = req.clone({
          setHeaders: {
            Authorization: `Bearer ${response.accessToken}`
          }
        });
        return next(newRequest);
      }),
      catchError(refreshError => {
        isRefreshing = false;
        // Si el refresh token también falló o expiró, cerrar sesión y redirigir a login
        authService.logout();
        return throwError(() => refreshError);
      })
    );
  }

  return next(req);
}
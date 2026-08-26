import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/catalog', pathMatch: 'full' },
  {
    path: 'auth',
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent),
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent),
      },
    ],
  },
  {
    path: 'catalog',
    canActivate: [authGuard],
    loadComponent: () => import('./features/catalog/catalog-page/catalog-page.component').then(m => m.CatalogPageComponent),
  },
  {
    path: 'movie/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/catalog/movie-detail/movie-detail.component').then(m => m.MovieDetailComponent),
  },
  {
    path: 'favorites',
    canActivate: [authGuard],
    loadComponent: () => import('./features/favorites/favorites-page/favorites-page.component').then(m => m.FavoritesPageComponent),
  },
  { path: '**', redirectTo: '/catalog' },
];

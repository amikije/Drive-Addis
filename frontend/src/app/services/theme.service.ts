import { Injectable, signal, effect, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export type ThemeMode = 'dark' | 'light';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private platformId = inject(PLATFORM_ID);
  
  // Theme Signal - defaults to dark mode for rich aesthetic
  public theme = signal<ThemeMode>(this.getInitialTheme());

  constructor() {
    // Reactive effect to update DOM when theme signal changes
    effect(() => {
      const currentTheme = this.theme();
      if (isPlatformBrowser(this.platformId)) {
        document.documentElement.setAttribute('data-theme', currentTheme);
        localStorage.setItem('driveaddis_theme', currentTheme);
      }
    });
  }

  private getInitialTheme(): ThemeMode {
    if (isPlatformBrowser(this.platformId)) {
      const savedTheme = localStorage.getItem('driveaddis_theme') as ThemeMode;
      if (savedTheme === 'light' || savedTheme === 'dark') {
        return savedTheme;
      }
      if (window.matchMedia && window.matchMedia('(prefers-color-scheme: light)').matches) {
        return 'light';
      }
    }
    return 'dark';
  }

  public toggleTheme(): void {
    const nextTheme: ThemeMode = this.theme() === 'dark' ? 'light' : 'dark';
    this.theme.set(nextTheme);
  }

  public setTheme(mode: ThemeMode): void {
    this.theme.set(mode);
  }

  public isDark(): boolean {
    return this.theme() === 'dark';
  }
}

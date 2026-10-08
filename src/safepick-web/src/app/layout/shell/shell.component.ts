import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '@app/core/auth.service';
import { homeForRole } from '@app/core/role-home';
import { navLinksForRole } from '@app/core/navigation';

// ============================================================
// ShellComponent
//
// The frame for every authenticated page.
//
// Provides:
//   - A branded header (logo, brand, user, logout)
//   - Role-aware navigation links
//   - The <router-outlet> for the routed page
// ============================================================

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
    MatIconModule,
  ],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
})
export class ShellComponent {
  private readonly auth = inject(AuthService);

  readonly session = this.auth.session;
  readonly role = this.auth.role;
  readonly email = this.auth.email;

  /** The user's role home path — where the brand link navigates. */
  readonly homePath = computed(() => homeForRole(this.role()));

  /** The role-appropriate navigation links. */
  readonly navLinks = computed(() => navLinksForRole(this.role()));

  logout(): void {
    this.auth.logout('/login');
  }
}
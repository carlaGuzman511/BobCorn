import { Component, OnDestroy } from '@angular/core';
import { CornService } from '../../core/services/corn.service';
import { interval, Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-corn-shop',
  imports: [CommonModule],
  standalone: true,
  templateUrl: './corn-shop.component.html',
  styleUrl: './corn-shop.component.scss'
})
export class CornShopComponent implements OnDestroy {
  totalPurchased = 0;
  isLoading = false;

  nextAllowedAt: Date | null = null;
  secondsRemaining = 0;

  private timerSub?: Subscription;

  clientId = this.getOrCreateClientId();

  constructor(private cornService: CornService) {}

  buyCorn() {
    if (this.isCooldownActive()) return;

    this.isLoading = true;

    this.cornService.purchase(this.clientId).subscribe({
      next: (res) => {
        this.totalPurchased = res.totalPurchased;
        this.startCooldown(new Date(res.nextAllowedAt));
      },
      error: (err) => {
        if (err.status === 429) {
          const nextAllowedAt = err.error?.nextAllowedAt;
          if (nextAllowedAt) {
            this.startCooldown(new Date(nextAllowedAt));
          }
        }
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  private startCooldown(date: Date) {
    this.nextAllowedAt = date;

    this.timerSub?.unsubscribe();

    this.timerSub = interval(1000).subscribe(() => {
      const now = new Date().getTime();
      const diff = Math.floor((date.getTime() - now) / 1000);

      this.secondsRemaining = diff > 0 ? diff : 0;

      if (diff <= 0) {
        this.timerSub?.unsubscribe();
        this.nextAllowedAt = null;
      }
    });
  }

  isCooldownActive(): boolean {
    return this.nextAllowedAt !== null && this.secondsRemaining > 0;
  }

  private getOrCreateClientId(): string {
    let id = localStorage.getItem('clientId');
    if (!id) {
      id = crypto.randomUUID();
      localStorage.setItem('clientId', id);
    }
    return id;
  }

  ngOnDestroy(): void {
    this.timerSub?.unsubscribe();
  }
}
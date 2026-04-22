import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface PurchaseResponse {
  message: string;
  totalPurchased: number;
  nextAllowedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class CornService {
  private baseUrl = 'https://localhost:7082/api/corn';

  constructor(private http: HttpClient) {}

  purchase(clientId: string): Observable<PurchaseResponse> {
    const headers = new HttpHeaders({
      'X-Client-Id': clientId
    });

    return this.http.post<PurchaseResponse>(
      `${this.baseUrl}/purchase`,
      {},
      { headers }
    );
  }
}
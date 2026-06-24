import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ClientPortalService {
  private apiUrl = `${environment.apiUrl}/ClientPortal`;

  constructor(private http: HttpClient) {}

  getLives(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/lives`);
  }

  getMeusArremates(liveId: number, nickInstagram: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/meus-arremates`, {
      params: { liveId: liveId.toString(), nickInstagram }
    });
  }
}

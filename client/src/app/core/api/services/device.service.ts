import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from "@angular/common/http";
import { Device } from '../interfaces/device';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DeviceService {

  constructor(
    protected http: HttpClient,
  ) { }

  get(): Observable<Device[]> {
    return this.http.get<Device[]>(`${environment.apiUrl}device`);
  }

  setThresholds(id: number, hiVal: number, loVal: number){
    const body = { hiVal: hiVal, loVal: loVal };
    return this.http.put<any>(`${environment.apiUrl}device/${id}`, body);
  }
}

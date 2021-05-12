import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { catchError } from 'rxjs/operators';
import { environment } from '@environments/environment';
import { BaseService } from './base.services';
import { User } from '../models';



@Injectable({ providedIn: 'root' })
export class UserService extends BaseService {
    constructor(private http: HttpClient) {
        super();
    }
    getAll() {
        const httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json'
            })
        };
        return this.http.get<User[]>(`${environment.apiUrl}/api/users`, httpOptions)
            .pipe(catchError(this.handleError));
    }
}
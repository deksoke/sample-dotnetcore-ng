import { Injectable } from '@angular/core';
import { Http, Headers } from '@angular/http';
import 'rxjs/add/operator/toPromise';
import { RequestResult } from '../models/request-result.model';
import { JwtService } from './jwt.service';

@Injectable()
export class AuthService {
    private tokeyKey = "token";
    private token: string;

    constructor(
        private http: Http,
        private jwtService: JwtService
    ) { }

    login(userName: string, password: string): Promise<RequestResult> {
        return this.http.post("/api/TokenAuth", { Username: userName, Password: password }).toPromise()
            .then(response => {
                let result = response.json() as RequestResult;
                if (result.State == 1) {
                    let json = result.Data as any;

                    this.jwtService.saveToken(json.accessToken);
                }
                return result;
            })
            .catch(this.handleError);
    }

    logout() { 
        this.token = '';
        this.jwtService.detroyToken();
    }

    checkLogin(): boolean {
        return this.jwtService.getToken() != null;
    }

    getUserInfo(): Promise<RequestResult> {
        return this.authGet("/api/TokenAuth");
    }

    authPost(url: string, body: any): Promise<RequestResult> {
        let headers = this.initAuthHeaders();
        return this.http.post(url, body, { headers: headers })
            .toPromise()
            .then(response => response.json() as RequestResult)
            .catch(this.handleError);
    }

    authGet(url): Promise<RequestResult> {
        let headers = this.initAuthHeaders();
        return this.http.get(url, { headers: headers })
            .toPromise()
            .then(response => response.json() as RequestResult)
            .catch(this.handleError);
    }

    private getLocalToken(): string {
        if (!this.token) {
            this.token = this.jwtService.getToken();
        }
        return this.token;
    }

    private initAuthHeaders(): Headers {
        let token = this.getLocalToken();
        if (token == null) throw "No token";

        var headers = new Headers();
        headers.append("Authorization", "Bearer " + token);

        return headers;
    }

    private handleError(error: any): Promise<any> {
        console.error('An error occurred', error);
        return Promise.reject(error.message || error);
    }
}
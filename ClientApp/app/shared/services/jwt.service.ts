import { Injectable } from '@angular/core';

@Injectable()
export class JwtService {
    private tokenKey:string = 'token';

    constructor() { }

    saveToken(token:string){
        window.localStorage.setItem(this.tokenKey, token);
    }

    detroyToken(){
        window.localStorage.setItem(this.tokenKey, null);
    }

    getToken():string {
        return window.localStorage.getItem(this.tokenKey);
    }
}
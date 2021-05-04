import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class IpDomainInfoService {

  constructor(private http: HttpClient) { }

  getIPDomainInfo(nameOrAddress: string, serviceList?:string[] ){
    let serviceUrl = "";
    if(serviceList && serviceList?.length !=0){
      serviceUrl = "http://localhost:5101/api/IPDomainInfo/" + nameOrAddress +"?serviceList="+   serviceList.join(',');
    } else{
      serviceUrl = "http://localhost:5101/api/IPDomainInfo/" + nameOrAddress;
    }
   
    return this.http.get(encodeURI(serviceUrl));
  }
}

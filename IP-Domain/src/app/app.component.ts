import { stringify } from '@angular/compiler/src/util';
import { Component } from '@angular/core';
import { FormControl, FormGroupDirective, NgForm, Validators } from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';
import { IpDomainInfoService } from './services/ip-domain-info.service';

const ipPattern = 
    "((^\s*((([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]))\s*$)|(^\s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:)))(%.+)?\s*$))";

const domainPattern = "^(?!-)[A-Za-z0-9-]+([\\-\\.]{1}[a-z0-9]+)*\\.[A-Za-z]{2,6}$";



export class MyErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || control.touched || isSubmitted));
  }
}

export interface IPDomainInfo {
  serviceName: string
  result: any
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})

export class AppComponent {
  constructor(private ipDomainInfoService: IpDomainInfoService) { }

  title = 'IP-Domain';
  selectedType = '';
  hostOrAddressInfo: IPDomainInfo[] = [];
  loading = false;

  typeList: string[] = ['IP Address', 'Domain'];
  serviceList: string[] = ['RDAP', 'GeoLocation', 'Ping', 'ReverseDns', 'IsDomainAvailable'];

  ipFormControl = new FormControl('', [
    Validators.required,
    Validators.pattern(ipPattern),
  ]);

  domainFormControl = new FormControl('', [
    Validators.required,
    Validators.pattern(domainPattern),
  ]);

  serviceFormControl = new FormControl(this.serviceList);

  matcher = new MyErrorStateMatcher();

  submit(){
    let tempInfoArray:IPDomainInfo[] = [];
    this.hostOrAddressInfo = [];
   if(this.selectedType === 'Domain'){
    this.loading = true;
    this.ipDomainInfoService.getIPDomainInfo(this.domainFormControl.value, this.serviceFormControl.value).subscribe(responses => {
      Object.values(responses).forEach((response:IPDomainInfo) => {
        response.result = JSON.parse(response.result);
        tempInfoArray.push(response);
      });
      this.loading = false;
      this.hostOrAddressInfo = tempInfoArray;
    });
   } else{
    this.loading = true;
    this.ipDomainInfoService.getIPDomainInfo(this.ipFormControl.value, this.serviceFormControl.value).subscribe(responses => {
      Object.values(responses).forEach((response:IPDomainInfo) => {
        response.result = JSON.parse(response.result);
        tempInfoArray.push(response);
      });
      this.loading = false;
      this.hostOrAddressInfo = tempInfoArray;
    });
   }
  }
}

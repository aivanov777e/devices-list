import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { interval, switchMap } from 'rxjs';
import { DeviceService } from 'src/app/core/api/services/device.service';

@Component({
  selector: 'app-devices',
  templateUrl: './devices.component.html',
  styleUrls: ['./devices.component.scss']
})
export class DevicesComponent implements OnInit {
  devices$: any;
  deviceGroup: FormGroup;

  constructor(private deviceService: DeviceService) {
    this.deviceGroup = new FormGroup({
      id: new FormControl('', [Validators.required, Validators.pattern('^\d+$')]),
      Hi: new FormControl(),
      Lo: new FormControl(),
    });
  }

  ngOnInit(): void {
    this.devices$ = interval(1000).pipe(
      switchMap(()=>this.deviceService.get())
    );
    // this.devices$ = this.deviceService.get();
  }

}

import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { catchError, interval, of, switchMap } from 'rxjs';
import { DeviceService } from 'src/app/core/api/services/device.service';

@Component({
  selector: 'app-devices',
  templateUrl: './devices.component.html',
  styleUrls: ['./devices.component.scss']
})
export class DevicesComponent implements OnInit {
  devices$: any;
  deviceGroup: FormGroup;
  showForm = false;

  constructor(private deviceService: DeviceService) {
    this.deviceGroup = new FormGroup({
      id: new FormControl('', [Validators.required, Validators.pattern('^\\d+$')]),
      hiVal: new FormControl('', [Validators.required, Validators.pattern('^\\d+$')]),
      loVal: new FormControl('', [Validators.required, Validators.pattern('^\\d+$')]),
    });
  }

  ngOnInit(): void {
    this.devices$ = interval(1000).pipe(
      switchMap(()=>this.deviceService.get())
    );
  }

  submit() {
    this.deviceService.setThresholds(
      this.deviceGroup.controls['id'].value,
      +this.deviceGroup.controls['hiVal'].value,
      +this.deviceGroup.controls['loVal'].value).subscribe(
        () => {
          this.showForm = false;
          this.deviceGroup.reset();
        }
      )
    this.deviceGroup;
  }
}

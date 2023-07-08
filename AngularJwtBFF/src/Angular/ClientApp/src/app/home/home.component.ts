import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from '../shared/authentication.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {

  public currentUser: any;

  constructor(
    private authService: AuthenticationService
  ) {

  }

  ngOnInit(): void {
    this.currentUser = this.authService.getUser();
  }
}

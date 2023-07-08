import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from '../shared/authentication.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  public isBusy = false;
  public loginModel: {
    username?: string,
    password?: string
  } = {};

  constructor(
    private authService: AuthenticationService
  ) { }

  ngOnInit(): void {
  }

  onSubmit() {
    if (this.isBusy) {
      return;
    }

    this.isBusy = true;

    // Post loginModel a /login
    this.authService.login(this.loginModel.username!, this.loginModel.password!)
      .subscribe(_ => {
        window.location.href = '/home';
      })
      .add(() => this.isBusy = false);
  }
}

import { NgxSpinnerService } from "ngx-spinner";
import { Component, OnInit } from "@angular/core";
import { Router, ActivatedRoute } from "@angular/router";
import { AuthService } from "src/app/shared/services";

@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.scss"],
})
export class LoginComponent implements OnInit {
  returnUrl: any;

  constructor(
    private authService: AuthService,
    private spinner: NgxSpinnerService
  ) {}

  ngOnInit() {}

  login() {
    this.spinner.show();
    this.authService.login();
  }
}

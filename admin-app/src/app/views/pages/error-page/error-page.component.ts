import { Component, OnInit, OnDestroy } from "@angular/core";
import { Subscription } from "rxjs";
import { ActivatedRoute, Router } from "@angular/router";
import { AuthService } from "src/app/shared/services";

@Component({
  selector: "app-error-page",
  templateUrl: "./error-page.component.html",
  styleUrls: ["./error-page.component.scss"],
})
export class ErrorPageComponent implements OnInit, OnDestroy {
  type: any;
  title: any;
  desc: any;
  error: boolean;
  private sub: Subscription;

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  async ngOnInit() {
    // check for error
    if (this.route.snapshot.queryParams.error) {
      this.error = true;
      
      this.type = this.route.snapshot.paramMap.get("type");
      this.sub = this.route.data.subscribe((param) => {
        if (param.type) {
          this.type = param.type;
        }
        if (param.title) {
          this.title = param.title;
        }
        if (param.desc) {
          this.desc = param.desc;
        }
      });

      switch (this.type) {
        case "404":
          if (!this.title) {
            this.title = "Page Not Found";
          }
          if (!this.desc) {
            this.desc = "Oopps!! The page you were looking for doesn't exist.";
          }
          break;
        case "500":
          if (!this.title) {
            this.title = "Internal server error";
          }
          if (!this.desc) {
            this.desc = "Oopps!! There wan an error. Please try agin later.";
          }
          break;
        default:
          // if (!this.type) {
          this.type = "Ooops..";
          // }
          if (!this.title) {
            this.title = "Something went wrong";
          }
          if (!this.desc) {
            this.desc =
              "Looks like something went wrong.<br>" + "We're working on it";
          }
      }
      return;
    }

    await this.authService.completeAuthentication();
    this.router.navigate(["/"]);
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }
}

import { Injectable } from '@angular/core';
import { CanActivate, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Router } from '@angular/router';
import { SystemConstants } from 'src/app/shared/constants';
import { AuthService } from 'src/app/shared/services';

@Injectable()
export class AuthGuard implements CanActivate {
  constructor(private router: Router, private authService: AuthService) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (this.authService.isAuthenticated()) {
      const functionCode = route.data['functionCode'] as string;
      const permissions = JSON.parse(this.authService.profile.permissions);
      if (permissions && permissions.filter(x => x === functionCode + '_' + SystemConstants.VIEW_ACTION).length > 0) {
        return true;
      } else {
        this.router.navigate(['/access-denied'], {
          queryParams: { redirect: state.url }
        });
        return false;
      }
    }
    this.router.navigate(['/auth/login'], { queryParams: { redirect: state.url }, replaceUrl: true });
    return false;
  }
}
import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { ViewsComponent } from "./views.component";
import { AuthGuard } from "../core/guard/auth.guard";
import { BaseComponent } from "./layout/base/base.component";

const routes: Routes = [
  {
    path: "",
    component: BaseComponent,
    children: [
      { path: "", redirectTo: "dashboard", pathMatch: "prefix" },
      {
        path: "dashboard",
        loadChildren: () =>
          import("./pages/dashboard/dashboard.module").then(
            (m) => m.DashboardModule
          ),
        data: {
          functionCode: "DASHBOARD",
        },
        canActivate: [AuthGuard],
      },
      {
        path: "tables",
        loadChildren: () =>
          import("./pages/tables/tables.module").then((m) => m.TablesModule),
      },
      {
        path: "icons",
        loadChildren: () =>
          import("./pages/icons/icons.module").then((m) => m.IconsModule),
      },
      {
        path: "general",
        loadChildren: () =>
          import("./pages/general/general.module").then((m) => m.GeneralModule),
      },

      {
        path: "systems",
        loadChildren: () =>
          import("./pages/systems/systems.module").then((m) => m.SystemsModule),
        data: {
          functionCode: "SYSTEM",
        },
        canActivate: [AuthGuard],
      },
    ],
  },
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ViewsRoutingModule {}

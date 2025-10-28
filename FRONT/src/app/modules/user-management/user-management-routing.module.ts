import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UserListComponent } from './components/user-list/user-list.component';
import { RoleListComponent } from './components/role-list/role-list.component';
import { TestUsersComponent } from './components/test-users/test-users.component';

const routes: Routes = [
  { path: '', component: UserListComponent },
  { path: 'roles', component: RoleListComponent },
  { path: 'test', component: TestUsersComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UserManagementRoutingModule { }

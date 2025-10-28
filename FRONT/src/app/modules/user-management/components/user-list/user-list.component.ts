
import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { GridModule, PageService, SortService, FilterService, GroupService, ToolbarService, ExcelExportService, PdfExportService } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToolbarModule } from '@syncfusion/ej2-angular-navigations';
import { ToastComponent, ToastModule } from '@syncfusion/ej2-angular-notifications';
import { UserService, User } from '../../services/user.service';
import { ToastService } from '../../../../services/toast.service';
import { UserFormComponent } from '../user-form/user-form.component';

@Component({
  selector: 'app-user-list',
  imports: [CommonModule, RouterModule, GridModule, ButtonModule, ToolbarModule, UserFormComponent, ToastModule],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss',
  providers: [PageService, SortService, FilterService, GroupService, ToolbarService, ExcelExportService, PdfExportService]
})
export class UserListComponent implements OnInit, AfterViewInit {
  users: User[] = [];
  loading = false;

  // Configurações da grid
  public pageSettings = { pageSize: 10 };
  public filterSettings = { type: 'Excel' };
  public toolbar = ['Search', 'ExcelExport', 'PdfExport'];

  // Propriedades para controle de estado
  showUserForm = false;
  selectedUser: User | null = null;
  isEditMode = false;

  @ViewChild('toast') private toastObj!: ToastComponent;

  constructor(
    private userService: UserService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  ngAfterViewInit(): void {
    if (this.toastObj) {
      this.toastService.setToastComponent(this.toastObj);
    }
  }

  loadUsers(): void {
    this.loading = true;
    this.userService.getUsers().subscribe({
      next: (users) => {
        this.users = users;
        this.loading = false;
      },
      error: () => {
        this.toastService.showError('Erro ao carregar usuários');
        this.loading = false;
      }
    });
  }

  deleteUser(user: User): void {
    if (confirm(`Tem certeza que deseja excluir o usuário ${user.username}?`)) {
      this.userService.deleteUser(user.id).subscribe({
        next: () => {
          this.toastService.showSuccess('Usuário excluído com sucesso');
          this.loadUsers();
        },
        error: () => {
          this.toastService.showError('Erro ao excluir usuário');
        }
      });
    }
  }

  getUserRoles(user: User): string {
    const fromUserRoles = user.userRoles?.map(ur => ur.role.name).filter(Boolean) || [];
    if (fromUserRoles.length > 0) return fromUserRoles.join(', ');
    const roles: any[] = (user as any).roles || (user as any).Roles || [];
    const names = roles.map(r => r?.name || r?.Name).filter(Boolean);
    return names.length > 0 ? names.join(', ') : 'Sem funções';
  }

  createUser(): void {
    this.selectedUser = null;
    this.isEditMode = false;
    this.showUserForm = true;
  }

  editUser(user: User): void {
    this.selectedUser = user;
    this.isEditMode = true;
    this.showUserForm = true;
  }

  toolbarClickHandler(args: any): void {
    if (args.item.text === 'Excel Export') {
      // Implementar exportação Excel
    } else if (args.item.text === 'PDF Export') {
      // Implementar exportação PDF
    }
  }

  onUserFormClose(): void {
    this.showUserForm = false;
    this.selectedUser = null;
    this.isEditMode = false;
    this.loadUsers();
  }
}

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { GridModule, PageService, SortService, FilterService, ToolbarService, ExcelExportService, PdfExportService } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToolbarModule } from '@syncfusion/ej2-angular-navigations';
import { UserService, Role, Permission } from '../../services/user.service';
import { ToastService } from '../../../../services/toast.service';
import { RoleFormComponent } from '../role-form/role-form.component';

@Component({
  selector: 'app-role-list',
  imports: [CommonModule, RouterModule, GridModule, ButtonModule, ToolbarModule, RoleFormComponent],
  templateUrl: './role-list.component.html',
  styleUrl: './role-list.component.scss',
  providers: [PageService, SortService, FilterService, ToolbarService, ExcelExportService, PdfExportService]
})
export class RoleListComponent implements OnInit {
  roles: Role[] = [];
  permissions: Permission[] = [];
  loading = false;
  
  // Configurações da grid
  public pageSettings = { pageSize: 10 };
  public filterSettings = { type: 'Excel' };
  public toolbar = ['Search', 'ExcelExport', 'PdfExport'];
  
  // Propriedades para controle de estado
  showRoleForm = false;
  selectedRole: Role | null = null;
  isEditMode = false;

  constructor(
    private userService: UserService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadRoles();
    this.loadPermissions();
  }

  loadRoles(): void {
    this.loading = true;
    this.userService.getRoles().subscribe({
      next: (roles) => {
        this.roles = roles;        
        this.loading = false;
      },
      error: (error) => {
        this.toastService.showError('Erro ao carregar funções');
        this.loading = false;
      }
    });
  }

  loadPermissions(): void {
    this.userService.getPermissions().subscribe({
      next: (permissions) => {
        this.permissions = permissions;
      },
      error: (error) => {
        this.toastService.showError('Erro ao carregar permissões');
      }
    });
  }

  createRole(): void {
    this.selectedRole = null;
    this.isEditMode = false;
    this.showRoleForm = true;
  }

  editRole(role: Role): void {
    this.selectedRole = role;
    this.isEditMode = true;
    this.showRoleForm = true;
  }

  deleteRole(role: Role): void {
    if (confirm(`Confirma a exclusão da função "${role.name}"?`)) {
      this.userService.deleteRole(role.id).subscribe({
        next: () => {
          this.toastService.showSuccess('Função excluída com sucesso!');
          this.loadRoles();
        },
        error: (error) => {
          this.toastService.showError('Erro ao excluir função');
        }
      });
    }
  }

  getRolePermissions(role: Role): string {
    // Verificar se existe a coleção Permissions (estrutura do backend)
    if (role.permissions && role.permissions.length > 0) {
      const permissionNames = role.permissions.map(p => p.name);
      return permissionNames.join(', ');
    }
    
    // Verificar se existe a coleção rolePermissions (estrutura esperada pelo frontend)
    if (role.rolePermissions && role.rolePermissions.length > 0) {
      const permissionNames = role.rolePermissions.map(rp => {
        return rp.permission?.name || 'Permissão inválida';
      });
      return permissionNames.join(', ');
    }
    
    return 'Sem permissões';
  }

  toolbarClickHandler(args: any): void {
    if (args.item.text === 'Excel Export') {
      // Implementar exportação Excel
    } else if (args.item.text === 'PDF Export') {
      // Implementar exportação PDF
    }
  }

  onRoleFormClose(): void {
    this.showRoleForm = false;
    this.selectedRole = null;
    this.isEditMode = false;
    this.loadRoles();
  }
}

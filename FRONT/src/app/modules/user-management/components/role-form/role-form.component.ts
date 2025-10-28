import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { ButtonModule, CheckBoxModule } from '@syncfusion/ej2-angular-buttons';
// import { DialogModule } from '@syncfusion/ej2-angular-popups';
import { DropDownListModule, MultiSelectModule } from '@syncfusion/ej2-angular-dropdowns';
import { forkJoin } from 'rxjs';
import { UserService } from '../../services/user.service';
import { ToastService } from '../../../../services/toast.service';
import { Role, Permission } from '../../services/user.service';

@Component({
  selector: 'app-role-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TextBoxModule,
    ButtonModule,
    CheckBoxModule,
    DropDownListModule,
    MultiSelectModule
  ],
  templateUrl: './role-form.component.html',
  styleUrl: './role-form.component.scss'
})
export class RoleFormComponent implements OnInit {
  @Input() role?: Role;
  @Input() isEditMode: boolean = false;
  @Output() onClose = new EventEmitter<void>();

  roleForm!: FormGroup;
  permissions: Permission[] = [];
  loading: boolean = false;
  submitted: boolean = false;

  permissionFields = { text: 'name', value: 'id' };
  
  // Tipos de role válidos
  roleTypes = [
    { name: 'ADMIN', description: 'Administrador' },
    { name: 'MANAGER', description: 'Gerente' },
    { name: 'SELLER', description: 'Vendedor' },
    { name: 'VIEWER', description: 'Visualizador' }
  ];

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadPermissions();
    
    if (this.isEditMode && this.role) {
      this.populateForm();
    }
  }
  
  ngOnChanges(changes: any): void {
    if (changes.role && !changes.role.firstChange) {
      if (this.isEditMode && changes.role.currentValue) {
        this.populateForm();
      }
    }
  }

  initForm(): void {
    this.roleForm = this.fb.group({
      name: ['', [Validators.required]], // Dropdown selection - no minLength needed
      description: ['', [Validators.required, Validators.minLength(5)]],
      permissions: [[]]
    });
  }

  loadPermissions(): void {
    this.userService.getPermissions().subscribe({
      next: (permissions) => {
        // Mapear permissões para garantir estrutura correta
        const mappedPermissions = permissions.map(perm => ({
          id: perm.id,
          name: perm.name,
          description: perm.description
        }));
        
        this.permissions = mappedPermissions;
      },
      error: (error) => {
        this.toastService.showError('Erro ao carregar permissões');
      }
    });
  }

  populateForm(): void {
    if (this.role) {
      const permissionIds = this.role.rolePermissions?.map(rp => rp.permissionId) || [];
      
      // Para edição, garantir que o nome corresponda a um dos tipos válidos
      let roleName = this.role.name;
      const validRoleTypes = ['ADMIN', 'MANAGER', 'SELLER', 'VIEWER'];
      
      // Se o nome não for um tipo válido, tentar converter ou usar ADMIN como padrão
      if (!validRoleTypes.includes(roleName)) {
        roleName = 'ADMIN'; // Valor padrão se não for válido
      }
      
      this.roleForm.patchValue({
        name: roleName,
        description: this.role.description,
        permissions: permissionIds
      });
    }
  }

  onSubmit(): void {
    this.submitted = true;
    
    if (this.roleForm.valid) {
      this.loading = true;
      
      // Preparar os dados no formato esperado pelo backend
      const roleData = {
        name: this.roleForm.value.name, // Agora vem do dropdown como enum válido
        description: this.roleForm.value.description,
        isActive: true // Sempre ativo ao criar/atualizar
      };

      const operation = this.isEditMode && this.role
        ? this.userService.updateRole(this.role.id, roleData)
        : this.userService.createRole(roleData);

      operation.subscribe({
        next: (savedRole) => {
          // Determinar o ID da role (para create o backend retorna a role, para update usamos o ID existente)
          const roleId = this.isEditMode ? this.role!.id : savedRole?.id;
          
          if (!roleId) {
            this.toastService.showError('Erro: ID da role não disponível');
            this.loading = false;
            return;
          }
          
          // Se houver permissões selecionadas, adicionar uma por uma
          const permissionIds = this.roleForm.value.permissions || [];
          if (permissionIds.length > 0) {
            this.addPermissionsToRole(roleId, permissionIds);
          } else {
            this.toastService.showSuccess(
              this.isEditMode ? 'Função atualizada com sucesso!' : 'Função criada com sucesso!'
            );
            this.onClose.emit();
          }
        },
        error: (error) => {
          this.toastService.showError(`Erro ao salvar função: ${error.status} - ${error.message}`);
          this.loading = false;
        }
      });
    }
  }

  private addPermissionsToRole(roleId: number, permissionIds: number[]): void {
    // Adicionar permissões uma por uma (como o backend espera)
    const addPermissionCalls = permissionIds.map(permissionId => 
      this.userService.addPermissionToRole(roleId, permissionId)
    );

    // Usar forkJoin para combinar múltiplos Observables
    forkJoin(addPermissionCalls).subscribe({
      next: () => {
        this.toastService.showSuccess(
          this.isEditMode ? 'Função atualizada com sucesso!' : 'Função criada com sucesso!'
        );
        this.onClose.emit();
      },
      error: (error) => {
        this.toastService.showError('Erro ao adicionar permissões à função');
        this.loading = false;
      }
    });
  }

  onCancel(): void {
    this.onClose.emit();
  }

  get f() {
    return this.roleForm.controls;
  }
}

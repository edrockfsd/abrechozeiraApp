import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { DropDownListModule, MultiSelectModule } from '@syncfusion/ej2-angular-dropdowns';
import { CheckBoxModule } from '@syncfusion/ej2-angular-buttons';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { HttpErrorResponse } from '@angular/common/http';
import { UserService, User, Role } from '../../services/user.service';
import { PessoaService } from '../../../pessoas/services/pessoa.service';
import { Pessoa } from '../../../pessoas/models/pessoa';
import { ToastService } from '../../../../services/toast.service';

@Component({
  selector: 'app-user-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TextBoxModule,
    DropDownListModule,
    MultiSelectModule,
    CheckBoxModule,
    ButtonModule
  ],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.scss'
})
export class UserFormComponent implements OnInit {
  @Input() user: User | null = null;
  @Input() isEditMode = false;
  @Output() onClose = new EventEmitter<void>();

  userForm!: FormGroup;
  roles: Role[] = [];
  pessoas: Pessoa[] = [];
  loading = false;
  submitted = false;

  public roleFields: Object = { text: 'name', value: 'id' };
  public pessoaFields: Object = { text: 'nome', value: 'id' };

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private pessoaService: PessoaService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadRoles();
    this.loadPessoas();

    if (this.isEditMode && this.user) {
      this.populateForm();
    }
  }

  ngOnChanges(changes: any): void {
    if (changes.user && !changes.user.firstChange) {
      if (this.isEditMode && changes.user.currentValue) {
        this.populateForm();
      }
    }
  }

  private loadPessoas(): void {
    this.pessoaService.listar().subscribe({
      next: (pessoas) => {
        this.pessoas = pessoas.filter(p => !p.userId);
      },
      error: (err: HttpErrorResponse) => {
        const msg = this.extractErrorMessage(err, 'Erro ao carregar pessoas');
        this.toastService.showError(msg);
      }
    });
  }

  onPessoaChange(event: any): void {
    const selectedPessoa = this.pessoas.find(p => p.id === event.value);
    if (selectedPessoa) {
      this.userForm.patchValue({
        firstName: selectedPessoa.nome,
        lastName: selectedPessoa.sobrenome
      });
    }
  }

  private initForm(): void {
    this.userForm = this.fb.group({
      pessoaId: [null, !this.isEditMode ? [Validators.required] : []],
      username: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      firstName: [{ value: '', disabled: true }],
      lastName: [{ value: '', disabled: true }],
      password: ['', this.isEditMode ? [] : [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', this.isEditMode ? [] : [Validators.required]],
      isActive: [true],
      roleIds: [[]]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  private passwordMatchValidator = (form: FormGroup): { [key: string]: boolean } | null => {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    const pwd = (password?.value ?? '') as string;
    const conf = (confirmPassword?.value ?? '') as string;

    if (this.isEditMode) {
      if (pwd && pwd.length < 6) {
        return { passwordTooShort: true };
      }
    }

    if ((pwd || conf) && pwd !== conf) {
      return { passwordMismatch: true };
    }
    return null;
  }

  private loadRoles(): void {
    this.userService.getRoles().subscribe({
      next: (roles) => {
        this.roles = roles;
      },
      error: (err: HttpErrorResponse) => {
        const msg = this.extractErrorMessage(err, 'Erro ao carregar funções');
        this.toastService.showError(msg);
      }
    });
  }

  private populateForm(): void {
    if (this.user) {
      this.userForm.patchValue({
        pessoaId: this.user.pessoaId,
        username: this.user.username,
        email: this.user.email,
        firstName: this.user.firstName,
        lastName: this.user.lastName,
        isActive: this.user.isActive,
        roleIds: this.user.userRoles?.map(ur => ur.roleId) || []
      });
    }
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.userForm.valid) {
      this.loading = true;

      const formData = this.userForm.getRawValue();
      const userData = {
        pessoaId: formData.pessoaId,
        username: formData.username,
        email: formData.email,
        isActive: formData.isActive,
        password: formData.password || undefined,
        roleIds: formData.roleIds
      };

      const request = this.isEditMode && this.user
        ? this.userService.updateUser(this.user.id, userData)
        : this.userService.createUser(userData);

      request.subscribe({
        next: () => {
          this.toastService.showSuccess(
            this.isEditMode ? 'Usuário atualizado com sucesso!' : 'Usuário criado com sucesso!'
          );
          this.onClose.emit();
        },
        error: (err: HttpErrorResponse) => {
          const msg = this.extractErrorMessage(err, 'Erro ao salvar usuário');
          this.toastService.showError(msg);
          this.loading = false;
        }
      });
    } else {
      this.toastService.showWarning('Por favor, preencha todos os campos obrigatórios');
    }
  }

  onCancel(): void {
    this.onClose.emit();
  }

  get f() { return this.userForm.controls; }

  private extractErrorMessage(err: HttpErrorResponse, fallback: string): string {
    if (!err) return fallback;
    const e = err.error;
    if (typeof e === 'string' && e.trim().length > 0) return e;
    if (e && typeof e === 'object') {
      if ((e as any).message) return (e as any).message;
      if ((e as any).title) return (e as any).title;
      if ((e as any).errors) {
        try {
          const entries = Object.entries((e as any).errors as Record<string, string[]>)
            .map(([k, v]) => `${k}: ${(v || []).join(', ')}`)
            .join(' | ');
          if (entries) return entries;
        } catch {}
      }
    }
    return err.statusText || fallback;
  }
}


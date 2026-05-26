import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl } from '@angular/forms';
import { HttpClient, HttpClientModule, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-cadastro-cliente',
  templateUrl: './cadastro-cliente.component.html',
  styleUrls: ['./cadastro-cliente.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, HttpClientModule]
})
export class CadastroClienteComponent implements OnInit {
  form!: FormGroup;
  loginForm!: FormGroup;
  
  // 'register' | 'login' | 'edit'
  mode: 'register' | 'login' | 'edit' = 'register';
  
  loading = false;
  success = false;
  errorMessage = '';
  successMessage = '';
  cepLoading = false;

  // Acessos gerados pós-cadastro
  generatedEmail = '';
  generatedPassword = '';

  constructor(
    private fb: FormBuilder,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.initLoginForm();
    
    // Verifica se já está logada no portal
    const savedToken = localStorage.getItem('portal_token');
    if (savedToken) {
      this.carregarDadosPerfil(savedToken);
    }
  }

  private initForm(): void {
    this.form = this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]], // Obrigatório para criar o Login
      telefone: ['', [Validators.required, Validators.minLength(14)]], // (00) 00000-0000
      cpf: ['', [Validators.required, Validators.minLength(14), this.cpfValidator]], // 000.000.000-00
      nickName: ['', [Validators.required, Validators.minLength(2)]], // Nickname Instagram
      cep: ['', [Validators.required, Validators.minLength(8)]],
      logradouro: ['', [Validators.required]],
      numero: ['', [Validators.required]],
      complemento: [''],
      bairro: ['', [Validators.required]],
      localidade: ['', [Validators.required]], // Cidade
      estado: ['', [Validators.required, Validators.maxLength(2)]]
    });
  }

  private initLoginForm(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  // Alternar modos da tela
  setMode(newMode: 'register' | 'login' | 'edit'): void {
    this.mode = newMode;
    this.errorMessage = '';
    this.successMessage = '';
    
    if (newMode === 'login') {
      this.loginForm.reset();
    } else if (newMode === 'register') {
      this.success = false;
      this.form.reset();
      // Se tiver token salvo ao mudar pra registrar, remove
      localStorage.removeItem('portal_token');
    }
  }

  // Máscaras aplicadas enquanto digita
  onCPFInput(event: any): void {
    let value = event.target.value.replace(/\D/g, '');
    if (value.length > 11) value = value.substring(0, 11);
    
    if (value.length > 9) {
      value = value.replace(/^(\d{3})(\d{3})(\d{3})(\d{1,2})$/, '$1.$2.$3-$4');
    } else if (value.length > 6) {
      value = value.replace(/^(\d{3})(\d{3})(\d{1,3})$/, '$1.$2.$3');
    } else if (value.length > 3) {
      value = value.replace(/^(\d{3})(\d{1,3})$/, '$1.$2');
    }
    
    this.form.get('cpf')?.setValue(value, { emitEvent: false });
  }

  onTelefoneInput(event: any): void {
    let value = event.target.value.replace(/\D/g, '');
    if (value.length > 11) value = value.substring(0, 11);
    
    if (value.length > 10) {
      value = value.replace(/^(\d{2})(\d{5})(\d{4})$/, '($1) $2-$3');
    } else if (value.length > 6) {
      value = value.replace(/^(\d{2})(\d{4})(\d{0,4})$/, '($1) $2-$3');
    } else if (value.length > 2) {
      value = value.replace(/^(\d{2})(\d{0,5})$/, '($1) $2');
    } else if (value.length > 0) {
      value = `(${value}`;
    }
    
    this.form.get('telefone')?.setValue(value, { emitEvent: false });
  }

  onCEPInput(event: any): void {
    let value = event.target.value.replace(/\D/g, '');
    if (value.length > 8) value = value.substring(0, 8);
    this.form.get('cep')?.setValue(value, { emitEvent: false });

    if (value.length === 8) {
      this.consultarCEP(value);
    }
  }

  onNickNameInput(event: any): void {
    let value = event.target.value.trim();
    // Adiciona o arroba automaticamente se digitar o nick sem ele
    if (value.length > 0 && !value.startsWith('@')) {
      value = '@' + value;
      this.form.get('nickName')?.setValue(value, { emitEvent: false });
    }
  }

  private consultarCEP(cep: string): void {
    this.cepLoading = true;
    this.errorMessage = '';
    
    this.http.get<any>(`https://viacep.com.br/ws/${cep}/json/`).subscribe({
      next: (data) => {
        this.cepLoading = false;
        if (data.erro) {
          this.errorMessage = 'CEP não encontrado.';
          return;
        }
        
        this.form.patchValue({
          logradouro: data.logradouro,
          bairro: data.bairro,
          localidade: data.localidade,
          estado: data.uf
        });
      },
      error: () => {
        this.cepLoading = false;
        this.errorMessage = 'Erro ao consultar o CEP. Preencha o endereço manualmente.';
      }
    });
  }

  // Validador Customizado de CPF
  private cpfValidator(control: AbstractControl): { [key: string]: any } | null {
    const value = control.value ? control.value.replace(/\D/g, '') : '';
    if (!value) return null;
    if (value.length !== 11) return { cpfInvalid: true };
    if (/^(.)\1+$/.test(value)) return { cpfInvalid: true };
    
    let soma = 0;
    let resto;
    
    for (let i = 1; i <= 9; i++) {
      soma += parseInt(value.substring(i - 1, i)) * (11 - i);
    }
    
    resto = (soma * 10) % 11;
    if (resto === 10 || resto === 11) resto = 0;
    if (resto !== parseInt(value.substring(9, 10))) return { cpfInvalid: true };
    
    soma = 0;
    for (let i = 1; i <= 10; i++) {
      soma += parseInt(value.substring(i - 1, i)) * (12 - i);
    }
    
    resto = (soma * 10) % 11;
    if (resto === 10 || resto === 11) resto = 0;
    if (resto !== parseInt(value.substring(10, 11))) return { cpfInvalid: true };
    
    return null;
  }

  // AÇÃO: Efetuar Login
  onLoginSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    
    const payload = this.loginForm.value;
    
    this.http.post<any>(`${environment.apiUrl}/Auth/login`, payload).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.token) {
          localStorage.setItem('portal_token', res.token);
          this.carregarDadosPerfil(res.token);
        } else {
          this.errorMessage = 'Erro na resposta do servidor.';
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Login ou senha incorretos.';
      }
    });
  }

  // Carrega os dados da cliente para edição
  private carregarDadosPerfil(token: string): void {
    this.loading = true;
    this.errorMessage = '';
    
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    
    this.http.get<any>(`${environment.apiUrl}/Pessoas/ObterDadosPerfil`, { headers }).subscribe({
      next: (data) => {
        this.loading = false;
        this.mode = 'edit';
        
        // Carrega formulário com os dados cadastrados e formatados
        this.form.patchValue({
          nome: data.nome,
          email: data.email,
          telefone: this.formatarTelefone(data.telefone),
          cpf: this.formatarCPF(data.cpf),
          nickName: data.nickName,
          cep: data.cep,
          logradouro: data.logradouro,
          numero: data.numero,
          complemento: data.complemento,
          bairro: data.bairro,
          localidade: data.localidade,
          estado: data.estado
        });

        // Desabilita CPF na edição por segurança
        this.form.get('cpf')?.disable();
      },
      error: (err) => {
        this.loading = false;
        localStorage.removeItem('portal_token');
        this.mode = 'login';
        this.errorMessage = 'Sessão expirada. Faça login novamente para alterar seus dados.';
      }
    });
  }

  // AÇÃO: Enviar Ficha (Cadastrar ou Atualizar)
  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';
    
    // Na edição, o CPF está desabilitado, mas precisamos enviar o valor dele ou reativar temporariamente
    const rawValue = this.form.getRawValue();
    
    if (this.mode === 'register') {
      // Registrar novo cliente
      this.http.post<any>(`${environment.apiUrl}/Pessoas/RegistrarCliente`, rawValue).subscribe({
        next: (res) => {
          this.loading = false;
          if (res.success) {
            this.generatedEmail = res.email;
            this.generatedPassword = res.password;
            this.success = true;
          } else {
            this.errorMessage = res.message || 'Erro ao realizar o cadastro.';
          }
        },
        error: (err) => {
          this.loading = false;
          this.errorMessage = err.error || 'Erro ao conectar com o servidor. Tente novamente mais tarde.';
        }
      });
    } else if (this.mode === 'edit') {
      // Atualizar perfil do cliente logado
      const token = localStorage.getItem('portal_token');
      if (!token) {
        this.mode = 'login';
        this.loading = false;
        return;
      }

      const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
      
      this.http.post<any>(`${environment.apiUrl}/Pessoas/AtualizarPerfil`, rawValue, { headers }).subscribe({
        next: (res) => {
          this.loading = false;
          if (res.success) {
            this.successMessage = 'Seus dados foram atualizados com sucesso!';
            // Rola para o topo para ver a mensagem
            window.scrollTo({ top: 0, behavior: 'smooth' });
          } else {
            this.errorMessage = res.message || 'Erro ao atualizar dados.';
          }
        },
        error: (err) => {
          this.loading = false;
          this.errorMessage = err.error || 'Erro ao conectar com o servidor. Tente novamente.';
        }
      });
    }
  }

  // Compartilhamento via WhatsApp
  compartilharAcessoWhatsApp(): void {
    const portalUrl = `${window.location.origin}/cadastro-cliente`;
    const mensagem = `Olá! Salvei meus dados de acesso ao portal do cliente do brechó *A Brechozeira*:\n\n*Usuário (E-mail):* ${this.generatedEmail}\n*Senha Temporária:* ${this.generatedPassword}\n\nPara alterar meus dados ou endereço futuramente, posso acessar este link:\n${portalUrl}`;
    
    const url = `https://api.whatsapp.com/send?text=${encodeURIComponent(mensagem)}`;
    window.open(url, '_blank');
  }

  logout(): void {
    localStorage.removeItem('portal_token');
    this.setMode('register');
  }

  private formatarTelefone(telefoneRaw: string): string {
    if (!telefoneRaw) return '';
    let value = telefoneRaw.replace(/\D/g, '');
    if (value.length > 11) value = value.substring(0, 11);
    
    if (value.length > 10) {
      return value.replace(/^(\d{2})(\d{5})(\d{4})$/, '($1) $2-$3');
    } else if (value.length > 6) {
      return value.replace(/^(\d{2})(\d{4})(\d{0,4})$/, '($1) $2-$3');
    } else if (value.length > 2) {
      return value.replace(/^(\d{2})(\d{0,5})$/, '($1) $2');
    } else if (value.length > 0) {
      return `(${value}`;
    }
    return value;
  }

  private formatarCPF(cpfRaw: string): string {
    if (!cpfRaw) return '';
    let value = cpfRaw.replace(/\D/g, '');
    if (value.length > 11) value = value.substring(0, 11);
    
    if (value.length > 9) {
      return value.replace(/^(\d{3})(\d{3})(\d{3})(\d{1,2})$/, '$1.$2.$3-$4');
    } else if (value.length > 6) {
      return value.replace(/^(\d{3})(\d{3})(\d{1,3})$/, '$1.$2.$3');
    } else if (value.length > 3) {
      return value.replace(/^(\d{3})(\d{1,3})$/, '$1.$2');
    }
    return value;
  }
}

import { Injectable } from '@angular/core';
import { ToastComponent } from '@syncfusion/ej2-angular-notifications';

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toastComponent: ToastComponent;

  public setToastComponent(toast: ToastComponent) {
    this.toastComponent = toast;
  }

  private show(message: string, title: string, cssClass: string) {
    if (this.toastComponent) {
      this.toastComponent.show({
        title: title,
        content: message,
        cssClass: cssClass,
        position: { X: 'Right', Y: 'Top' },
        showCloseButton: true,
        timeOut: 5000
      });
    }
  }

  public showSuccess(message: string) {
    this.show(message, 'Sucesso!', 'e-toast-success');
  }

  public showError(message: string) {
    this.show(message, 'Erro!', 'e-toast-danger');
  }

  public showWarning(message: string) {
    this.show(message, 'Aviso!', 'e-toast-warning');
  }

  public showInfo(message: string) {
    this.show(message, 'Informação', 'e-toast-info');
  }
} 
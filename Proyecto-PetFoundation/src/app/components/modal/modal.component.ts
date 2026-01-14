import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './modal.component.html',
  styleUrl: './modal.component.css'
})
export class ModalComponent {
  @Input() isOpen = false;
  @Input() title = '';
  @Input() message = '';
  @Input() showInput = false;
  @Input() inputLabel = '';
  @Input() inputPlaceholder = '';
  @Input() inputType: 'text' | 'textarea' = 'text';
  @Input() confirmText = 'Confirmar';
  @Input() cancelText = 'Cancelar';
  @Input() showCancel = true;
  @Input() isLoading = false;
  
  @Output() confirm = new EventEmitter<string>();
  @Output() cancel = new EventEmitter<void>();

  inputValue = '';

  onConfirm(): void {
    if (this.showInput && !this.inputValue.trim()) {
      return;
    }
    this.confirm.emit(this.inputValue);
    this.reset();
  }

  onCancel(): void {
    this.cancel.emit();
    this.reset();
  }

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal-backdrop')) {
      this.onCancel();
    }
  }

  private reset(): void {
    this.inputValue = '';
  }
}

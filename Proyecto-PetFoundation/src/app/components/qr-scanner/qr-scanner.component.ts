import { Component, EventEmitter, Output, OnDestroy, ElementRef, ViewChild, NgZone, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-qr-scanner',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './qr-scanner.component.html',
  styleUrl: './qr-scanner.component.css'
})
export class QrScannerComponent implements OnDestroy {
  @ViewChild('video') videoRef!: ElementRef<HTMLVideoElement>;
  @ViewChild('canvas') canvasRef!: ElementRef<HTMLCanvasElement>;

  @Output() petFound = new EventEmitter<number>();
  @Output() close = new EventEmitter<void>();

  cameraActive = false;
  cameraError = '';
  manualId = '';
  manualIdError = '';
  hasBarcodeDetector = false;
  isScanning = false;

  private stream: MediaStream | null = null;
  private animationFrameId: number | null = null;
  private barcodeDetector: any = null;

  constructor(private ngZone: NgZone, private cdr: ChangeDetectorRef) {
    this.hasBarcodeDetector = 'BarcodeDetector' in window;
  }

  async startCamera(): Promise<void> {
    this.cameraError = '';
    try {
      this.stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' }
      });

      // Set cameraActive first so the <video> element renders via *ngIf
      this.cameraActive = true;
      this.cdr.detectChanges();

      const video = this.videoRef.nativeElement;
      video.srcObject = this.stream;
      await video.play();

      if (this.hasBarcodeDetector) {
        this.barcodeDetector = new (window as any).BarcodeDetector({ formats: ['qr_code'] });
        this.startScanning();
      }
    } catch (err: any) {
      this.cameraActive = false;
      this.cameraError = 'No se pudo acceder a la cámara. Usa la búsqueda manual.';
      console.error('Camera error:', err);
    }
  }

  private startScanning(): void {
    if (!this.barcodeDetector || !this.cameraActive) return;
    this.isScanning = true;

    const scan = async () => {
      if (!this.cameraActive || !this.isScanning) return;

      try {
        const video = this.videoRef.nativeElement;
        if (video.readyState === video.HAVE_ENOUGH_DATA) {
          const barcodes = await this.barcodeDetector.detect(video);
          if (barcodes.length > 0) {
            const rawValue = barcodes[0].rawValue;
            const petId = this.extractPetId(rawValue);
            if (petId) {
              this.ngZone.run(() => {
                this.stopCamera();
                this.petFound.emit(petId);
              });
              return;
            }
          }
        }
      } catch (err) {
        // Detection frame error, continue scanning
      }

      this.animationFrameId = requestAnimationFrame(scan);
    };

    this.animationFrameId = requestAnimationFrame(scan);
  }

  private extractPetId(value: string): number | null {
    // Support raw ID number or URL containing /pets/{id}
    const trimmed = value.trim();

    // Try direct numeric ID
    const directId = parseInt(trimmed, 10);
    if (!isNaN(directId) && directId > 0 && String(directId) === trimmed) {
      return directId;
    }

    // Try URL pattern /pets/{id}
    const urlMatch = trimmed.match(/\/pets\/(\d+)/);
    if (urlMatch) {
      return parseInt(urlMatch[1], 10);
    }

    return null;
  }

  stopCamera(): void {
    this.isScanning = false;
    this.cameraActive = false;

    if (this.animationFrameId !== null) {
      cancelAnimationFrame(this.animationFrameId);
      this.animationFrameId = null;
    }

    if (this.stream) {
      this.stream.getTracks().forEach(track => track.stop());
      this.stream = null;
    }
  }

  searchManualId(): void {
    this.manualIdError = '';
    const id = parseInt(this.manualId.trim(), 10);
    if (isNaN(id) || id <= 0) {
      this.manualIdError = 'Ingresa un ID válido (número positivo)';
      return;
    }
    this.stopCamera();
    this.petFound.emit(id);
  }

  onClose(): void {
    this.stopCamera();
    this.close.emit();
  }

  ngOnDestroy(): void {
    this.stopCamera();
  }
}

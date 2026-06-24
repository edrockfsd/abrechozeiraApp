import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConsultaArrematesComponent } from './consulta-arremates.component';

describe('ConsultaArrematesComponent', () => {
  let component: ConsultaArrematesComponent;
  let fixture: ComponentFixture<ConsultaArrematesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConsultaArrematesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConsultaArrematesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

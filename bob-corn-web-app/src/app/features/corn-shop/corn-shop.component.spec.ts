import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CornShopComponent } from './corn-shop.component';

describe('CornShopComponent', () => {
  let component: CornShopComponent;
  let fixture: ComponentFixture<CornShopComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CornShopComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CornShopComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

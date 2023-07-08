import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';

@Component({
  selector: 'app-products',
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.css']
})
export class ProductsComponent {
  public products: any[] = [];

  constructor(private httpClient: HttpClient) {
    this.getProducts();
  }

  getProducts() {
    this.httpClient.get('/api/products').subscribe((response) => {
      this.products = response as any[];
    });
  }
}

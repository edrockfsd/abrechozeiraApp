import { Component, OnInit } from '@angular/core';
import { MENU_ITEMS } from '../loja-menu';

@Component({
  selector: 'ngx-vendas',
  templateUrl: './vendas.component.html',
  styleUrls: ['./vendas.component.scss']
})
export class VendasComponent implements OnInit {

  menu = MENU_ITEMS;
  
  constructor() { }

  ngOnInit() {
  }

}

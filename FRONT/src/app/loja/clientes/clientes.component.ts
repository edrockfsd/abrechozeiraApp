import { Component, OnInit } from '@angular/core';
import { MENU_ITEMS } from '../loja-menu';

@Component({
  selector: 'ngx-clientes',
  templateUrl: './clientes.component.html',
  styleUrls: ['./clientes.component.scss']
})
export class ClientesComponent implements OnInit {

  menu = MENU_ITEMS;
  
  constructor() { }

  ngOnInit() {
    console.log('entrei na clientes component');
  }

}

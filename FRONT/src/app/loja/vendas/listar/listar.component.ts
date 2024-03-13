import { Component, OnInit } from '@angular/core';
import { LocalDataSource } from 'ng2-smart-table';

import { AbrechozeiraApiService } from '../../abrechozeira-api.service';
import { Observable } from 'rxjs';
import * as rxops from 'rxjs/operators'
import * as rxjs from 'rxjs'

@Component({
  selector: 'ngx-listar',
  templateUrl: './listar.component.html',
  styleUrls: ['./listar.component.scss']
})
export class ListarComponent implements OnInit {

  VendasList: any[];

  constructor(private service: AbrechozeiraApiService) {
    
    this.service.getVendasCompleta().subscribe(data =>{
      console.log('data');
      console.log(data);  
      const _data = data;
      this.source.load(_data);
    });

    console.log(this.source);
   }  

  ngOnInit() {
    
  }

  onDeleteConfirm(event): void {
    if (window.confirm('Are you sure you want to delete?')) {
      event.confirm.resolve();
    } else {
      event.confirm.reject();
    }
  }

  settings = {
    add: {
      addButtonContent: '<i class="nb-plus"></i>',
      createButtonContent: '<i class="nb-checkmark"></i>',
      cancelButtonContent: '<i class="nb-close"></i>',
    },
   //hideSubHeader: true,
   noDataMessage: 'A pesquisa não retornou resultados.', 
   actions:{
     position: "right",
     columnTitle: "Ações",      
     // add: false,
     // edit: false,
     // delete: false
   },
   edit: {
     editButtonContent: '<i class="nb-edit"></i>',
     saveButtonContent: '<i class="nb-checkmark"></i>',
     cancelButtonContent: '<i class="nb-close"></i>',
   },
   delete: {
     deleteButtonContent: '<i class="nb-trash"></i>',
     confirmDelete: true,
   },
   columns: {
     id: {
       title: 'ID',
       type: 'number',
     },
     codigoEstoque: {
      title: 'Código Produto',
      type: 'int',
    },
    codigoLive: {
      title: 'Código Live',
      type: 'int',
    },
    descricao: {
       title: 'Produto',
       type: 'string',
     },     
     valorVenda: {
       title: 'Valor Venda',
       type: 'decimal',
     },
      nickName: {
       title: 'Comprador',
       type: 'string',
     },
     origem: {
      title: 'Origem',
      type: 'string',
      class: 'cell_right'
    },
   },
 };

 source: LocalDataSource = new LocalDataSource();

}

import { ChangeDetectionStrategy, Component, ViewChild, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { UploadModule } from './upload.module';
import * as XLSX from 'xlsx'
import { AbrechozeiraApiService } from '../../abrechozeira-api.service';
import { Console } from 'console';



@Component({
  selector: 'ngx-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.scss']
})
export class UploadComponent implements OnInit {
  
  ExcelData: any;
  ExcelCols: any;
  dataPrep: any;
  objCliente: any=[];
  isDataReady: boolean;
  
  constructor(private service: AbrechozeiraApiService){
    
  }
  

  ngOnInit(): void {
    
  }

   ReadExcel(event:any){
    console.log('entrei no ReadExcel');
    this.isDataReady = false;
    let file = event.target.files[0];

    let fileReader = new FileReader();
    fileReader.readAsBinaryString(file);

    fileReader.onload = async (e) => {
      var workBook = XLSX.read(fileReader.result,{type:'binary'});
      var sheetNames = workBook.SheetNames;
      this.ExcelData = XLSX.utils.sheet_to_json(workBook.Sheets[sheetNames[0]],{defval:""});    
      
      console.log(workBook.Sheets[sheetNames[0]]['C4'].v);

      var jsonobj = this.ExcelData[0];
      let tempCols: string[] = [];
      Object.keys(jsonobj).forEach(function(key) {
        tempCols.push(key);        
        var value = jsonobj[key];
        //console.log('key:' + key);
        //console.log('value:' + value);
      });     
      this.ExcelCols = tempCols;    
      
      this.isDataReady = false;
    }

    
  }

  UploadDataToDB(){
    //console.log(this.ExcelData);

    for (let index = 0; index < this.ExcelData.lenght - 1; index++) {
      var objVenda = {
       Quantidade: 1,
       ValorVenda: this.ExcelData[index].valorvenda,
       Desconto: 0,
       ClienteId: 1,
       ProdutoId: this.ExcelData[index].codprod,
       CodigoLive: this.ExcelData[index].codigolive,
       OrigemID: 123,
       OrdemVendaLive: this.ExcelData[index].ordemvendalive,
       LiveId: 123456789      
      }
    }
  }

  private async isClienteExistente(Nick: string): Promise<boolean>{
    let bRetorno: boolean = false;
    
    try {
      const cliente = await this.service.getPessoaPorNick(Nick);
      if (cliente != null){
        if (cliente.id != 0) {
          bRetorno = true;
          return bRetorno;
        }
      }
    } catch (error) {
      console.log('cliente não encontrado');
      return bRetorno;
    }
  }

  
 public async clicarBotao(){        
  for (let index = 0; index < Object.keys(this.ExcelData).length - 1; index++) {                    
    console.log(this.ExcelData[index]['comprador']);
    if (this.ExcelData[index]['comprador'] == '') {
      console.log('Cliente Vazio');
      this.ExcelData[index]['comprador'] = '**VAZIO';  
    }
    else if (await this.isClienteExistente(this.ExcelData[index]['comprador'])){
      console.log('cliente existe');          
    }else{
      console.log('cliente não encontrado');
      this.ExcelData[index]['comprador'] = '**' + this.ExcelData[index]['comprador'];
    }
    
    console.log(this.ExcelData[index]['codprod']);
    if (this.ExcelData[index]['codprod'] == '') {
      console.log('Produto vazio');
      this.ExcelData[index]['comprador'] = '**VAZIO';  
    }
    else if (await this.service.isProdutoExistentePorCodigoEstoque(this.ExcelData[index]['codprod'])){
      console.log('produto existe');          
    }else{
      console.log('produto não existe');    
      this.ExcelData[index]['codprod'] = '**' + this.ExcelData[index]['codprod'];
    }
  }
  this.isDataReady = true;
 }
}

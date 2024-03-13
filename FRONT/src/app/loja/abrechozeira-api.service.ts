import  {HttpClient}  from '@angular/common/http';
import { Injectable } from '@angular/core';
import { error } from 'console';
import { Observable } from 'rxjs';
    
@Injectable({
  providedIn: 'root'
})
export class AbrechozeiraApiService {

  readonly abrechozeiraAPIUrl = "https://localhost:7194/api";
  constructor(private http:HttpClient) { }

  //Tarefas
  addTarefa(data:any){
    return this.http.post(this.abrechozeiraAPIUrl + '/tarefas', data)
  }

  updateTarefa(id:number|string, data:any){
    return this.http.put(this.abrechozeiraAPIUrl + `/tarefas/${id}`, data)
  }

  deleteTarefa(id:number|string){
    return this.http.delete(this.abrechozeiraAPIUrl + `/tarefas/${id}`)
  }

  //TarefaTipos
  getTarefaTiposList():Observable<any[]>{
    return this.http.get<any>(this.abrechozeiraAPIUrl + '/tarefatipos');
  }

  addTarefaTipos(data:any){
    return this.http.post(this.abrechozeiraAPIUrl + '/tarefatipos', data)
  }

  updateTarefaTipos(id:number|string, data:any){
    return this.http.put(this.abrechozeiraAPIUrl + `/tarefatipos/${id}`, data)
  }

  deleteTarefaTipos(id:number|string){
    return this.http.delete(this.abrechozeiraAPIUrl + `/tarefatipos/${id}`)
  }

  //Status
  getStatusList():Observable<any[]>{
    return this.http.get<any>(this.abrechozeiraAPIUrl + '/status');
  }

  addStatus(data:any){
    return this.http.post(this.abrechozeiraAPIUrl + '/status', data)
  }

  updateStatus(id:number|string, data:any){
    return this.http.put(this.abrechozeiraAPIUrl + `/status/${id}`, data)
  }

  deleteStatus(id:number|string){
    return this.http.delete(this.abrechozeiraAPIUrl + `/status/${id}`)
  }

  //Vendas
  getVendasCompleta():Observable<any[]>{    
    return this.http.get<any>(this.abrechozeiraAPIUrl + '/Vendas/GetVendasCompleta');
  }

  getVendas():Observable<any[]>{    
    return this.http.get<any>(this.abrechozeiraAPIUrl + '/Vendas');
  }

  addVendas(data:any){
    return this.http.post(this.abrechozeiraAPIUrl + '/Vendas', data)
  }

  updateVendas(id:number|string, data:any){
    return this.http.put(this.abrechozeiraAPIUrl + `/Vendas/${id}`, data)
  }

  deleteVendas(id:number|string){
    return this.http.delete(this.abrechozeiraAPIUrl + `/Vendas/${id}`)
  }

  //Pessoa
  getPessoa():Observable<any[]>{    
    return this.http.get<any>(this.abrechozeiraAPIUrl + '/Pessoas');
  }

  public async getPessoaPorNick(nickName: string):Promise<any>{    
    return await this.http.get<any>(this.abrechozeiraAPIUrl + `/Pessoas/GetPessoaPorNick?nickName=${nickName}`).pipe().toPromise();
  }

  public async isClienteExistente(nickName: string):Promise<Boolean>{
    console.log('isClienteExistente início' +  nickName);
    try {
      this.http.get<any>(this.abrechozeiraAPIUrl + `/Pessoas/GetPessoaPorNick?nickName=${nickName}`).pipe().toPromise().then(data =>{
        if(data != null){
          console.log('data não é nula');
          if(data.id == 0){
            console.log('id == 0');
            return false;
          }
          else{
            console.log('id != 0');
            return true;
          }
        }else{
          return false;
        }
      });
    } catch (error) {
      return false;
    }    
  }

  addPessoas(data:any){
    return this.http.post(this.abrechozeiraAPIUrl + '/Pessoas', data);
  }

  updatePessoas(id:number|string, data:any){
    return this.http.put(this.abrechozeiraAPIUrl + `/Pessoas/${id}`, data)
  }

  deletePessoas(id:number|string){
    return this.http.delete(this.abrechozeiraAPIUrl + `/Pessoas/${id}`)
  }

  //Produto
  getProdutoList():Observable<any[]>{    
    return this.http.get<any>(this.abrechozeiraAPIUrl + '/Produtos');
  }

  public async getProduto(id:number|string):Promise<any>{    
    return await this.http.get<any>(this.abrechozeiraAPIUrl + `/Produtos/${id}`).pipe().toPromise();
  }

  public async getProdutoPorDescricao(descricao: string):Promise<any>{    
    return await this.http.get<any>(this.abrechozeiraAPIUrl + `/Pessoas/GetProdutoPorDescricao?descricao=${descricao}`).pipe().toPromise();
  }

  public async isProdutoExistentePorCodigoEstoque(codigoEstoque:number|string):Promise<Boolean>{    
     try {
      return await this.http.get<any>(this.abrechozeiraAPIUrl + `/Produtos/ProdutoExistsByCodigoEstoque?codigoEstoque=${codigoEstoque}`).pipe().toPromise();
    } catch (error) {
      return false;
    }
  }

  addProdutos(data:any){
    return this.http.post(this.abrechozeiraAPIUrl + '/Produtos', data)
  }

  updateProdutos(id:number|string, data:any){
    return this.http.put(this.abrechozeiraAPIUrl + `/Produtos/${id}`, data)
  }

  deleteProdutos(id:number|string){
    return this.http.delete(this.abrechozeiraAPIUrl + `/Produtos/${id}`)
  }
}

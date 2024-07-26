import { CommonModule } from '@angular/common';
import { HttpClient,  HttpClientModule,  provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet,CommonModule,HttpClientModule,FormsModule],
  providers:[HttpClient],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  /**
   *
   */
  constructor(private http:HttpClient) {
  }
  ngOnInit(): void {
    this.fetchData(this.pageNumber,this.pageSize);
    
  }
  showSpinner():boolean{
    return !this.pageData || this.pageData.length === 0;
  }
  fetchData(pageNum:number,pageSize:number){
    this.http.get("http://localhost:5232/Stories").subscribe({
      next: (val:any)=>{
         this.data=val;
         console.log(val);
         this.totalCount=this.data.length;
       },
     error:  (err)=>{
         console.log(err);
       },
     complete:  ()=>{
         console.log('completed');
         this.pageData=this.data.slice(pageNum*pageSize,(pageNum*pageSize)+pageSize);
       }
   })
  }

  prev(){
    this.pageNumber--;
    if(this.pageNumber<0){
      this.pageNumber=0;
    }
   this.pageData=this.data.slice(this.pageNumber*this.pageSize,(this.pageNumber*this.pageSize)+this.pageSize);
  }
  next(){
    this.pageNumber++;
    this.pageData=this.data.slice(this.pageNumber*this.pageSize,(this.pageNumber*this.pageSize)+this.pageSize);
  }
  search(){
    this.pageNumber=0;
    this.data=[];
    this.pageData=[];
    console.log("clicked");
    var url="http://localhost:5232/Stories/Search";
    if(this.searchVal!=='')
      url+="?name="+this.searchVal;
      this.http.get(url).subscribe({
  next: (val:any)=>{
    this.data=val;
    
    this.totalCount=this.data.length;
  },
error:  (err)=>{
    console.log(err);
  },
complete:  ()=>{
    console.log('completed');
    this.pageData=this.data.slice(this.pageNumber*this.pageSize,(this.pageNumber*this.pageSize)+this.pageSize);
  }
})
  }
  
  title = 'Story-App';
  data:Story[]=[];
  pageData:Story[]=[];
  totalCount:number=0;
  pageNumber:number=0;
  pageSize:number=10;
  searchVal:string='';
  temp!:{id:number; title:string;url:string;};
}
class Story{
  id!:number;
  title!:string;
  url!:string
}

'use strict';
const fs=require('fs'),os=require('os'),path=require('path');
const root=fs.mkdtempSync(path.join(os.tmpdir(),'pkc-event-bridge-'));
try{
 fs.mkdirSync(path.join(root,'child'));fs.mkdirSync(path.join(root,'parent'));fs.mkdirSync(path.join(root,'services'));
 fs.writeFileSync(path.join(root,'child','card.ts'),`import {Component,EventEmitter,Input,Output} from '@angular/core';\n@Component({selector:'app-card',templateUrl:'./card.html'}) export class CardComponent { @Input() item!: any; @Output() changed=new EventEmitter<[string,string]>(); onRun(){this.changed.emit(['run',this.item.id]);}}`);
 fs.writeFileSync(path.join(root,'child','card.html'),`<button (click)="onRun()">Run</button>`);
 fs.writeFileSync(path.join(root,'services','api.ts'),`export class ApiService { run(id:string){return this.http.put('/api/items/'+id+'/run',{})}}`);
 fs.writeFileSync(path.join(root,'parent','page.ts'),`import {Component,inject} from '@angular/core'; import {CardComponent} from '../child/card'; import {ApiService} from '../services/api'; @Component({selector:'app-page',templateUrl:'./page.html',imports:[CardComponent]}) export class PageComponent { private api=inject(ApiService); handle([event,id]:[string,string]){switch(event){case 'run':this.run(id);break;}} private run(id:string){this.api.run(id).subscribe();}}`);
 fs.writeFileSync(path.join(root,'parent','page.html'),`<app-card [item]="item" (changed)="handle($event)"></app-card>`);
 const api=[{id:'api1',ownerClass:'services/api.ts#ApiService',container:'run'}]; fs.writeFileSync(path.join(root,'api.json'),JSON.stringify(api));
 const {analyze}=require('../../src/Pkc.Frontend/AngularOutputEventBridge.cjs'); const out=analyze(root,process.env.PKC_TYPESCRIPT_PATH,path.join(root,'api.json'));
 if(out.facts.length!==1)throw new Error('fact count '+JSON.stringify(out)); if(out.facts[0].name!=='Run')throw new Error('label'); if(out.relations.length!==1||out.relations[0].target!=='api1')throw new Error('relation');
 fs.writeFileSync(path.join(root,'parent','page2.ts'),`import {Component,inject} from '@angular/core'; import {CardComponent} from '../child/card'; import {ApiService} from '../services/api'; @Component({selector:'app-page2',templateUrl:'./page2.html',imports:[CardComponent]}) export class Page2Component { private api=inject(ApiService); handle([event,id]:[string,string]){switch(event){case 'run':this.run(id);break;}} run(id:string){this.api.run(id).subscribe();}}`); fs.writeFileSync(path.join(root,'parent','page2.html'),`<app-card (changed)="handle($event)"></app-card>`);
 const amb=analyze(root,process.env.PKC_TYPESCRIPT_PATH,path.join(root,'api.json')); if(amb.facts.length!==0)throw new Error('ambiguous parent must fail closed '+JSON.stringify(amb));
 fs.rmSync(path.join(root,'parent','page2.ts'),{force:true}); fs.rmSync(path.join(root,'parent','page2.html'),{force:true});
 fs.writeFileSync(path.join(root,'parent','page.ts'),`import {Component,inject} from '@angular/core'; import {ApiService} from '../services/api'; @Component({selector:'app-page',templateUrl:'./page.html'}) export class PageComponent { private api=inject(ApiService); handle([event,id]:[string,string]){switch(event){case 'run':this.run(id);break;}} private run(id:string){this.api.run(id).subscribe();}}`);
 const noImport=analyze(root,process.env.PKC_TYPESCRIPT_PATH,path.join(root,'api.json')); if(noImport.facts.length!==0)throw new Error('unimported child selector must fail closed '+JSON.stringify(noImport));
 console.log('angular output event bridge regression PASS');
}finally{fs.rmSync(root,{recursive:true,force:true});}

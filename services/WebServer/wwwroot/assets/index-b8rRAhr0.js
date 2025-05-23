var dc=Object.defineProperty;var hc=(e,t,o)=>t in e?dc(e,t,{enumerable:!0,configurable:!0,writable:!0,value:o}):e[t]=o;var G=(e,t,o)=>hc(e,typeof t!="symbol"?t+"":t,o);(function(){const t=document.createElement("link").relList;if(t&&t.supports&&t.supports("modulepreload"))return;for(const s of document.querySelectorAll('link[rel="modulepreload"]'))i(s);new MutationObserver(s=>{for(const r of s)if(r.type==="childList")for(const a of r.addedNodes)a.tagName==="LINK"&&a.rel==="modulepreload"&&i(a)}).observe(document,{childList:!0,subtree:!0});function o(s){const r={};return s.integrity&&(r.integrity=s.integrity),s.referrerPolicy&&(r.referrerPolicy=s.referrerPolicy),s.crossOrigin==="use-credentials"?r.credentials="include":s.crossOrigin==="anonymous"?r.credentials="omit":r.credentials="same-origin",r}function i(s){if(s.ep)return;s.ep=!0;const r=o(s);fetch(s.href,r)}})();/**
 * @license
 * Copyright 2019 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Qi=globalThis,Er=Qi.ShadowRoot&&(Qi.ShadyCSS===void 0||Qi.ShadyCSS.nativeShadow)&&"adoptedStyleSheets"in Document.prototype&&"replace"in CSSStyleSheet.prototype,Tr=Symbol(),wa=new WeakMap;let Rn=class{constructor(t,o,i){if(this._$cssResult$=!0,i!==Tr)throw Error("CSSResult is not constructable. Use `unsafeCSS` or `css` instead.");this.cssText=t,this.t=o}get styleSheet(){let t=this.o;const o=this.t;if(Er&&t===void 0){const i=o!==void 0&&o.length===1;i&&(t=wa.get(o)),t===void 0&&((this.o=t=new CSSStyleSheet).replaceSync(this.cssText),i&&wa.set(o,t))}return t}toString(){return this.cssText}};const uc=e=>new Rn(typeof e=="string"?e:e+"",void 0,Tr),O=(e,...t)=>{const o=e.length===1?e[0]:t.reduce((i,s,r)=>i+(a=>{if(a._$cssResult$===!0)return a.cssText;if(typeof a=="number")return a;throw Error("Value passed to 'css' function must be a 'css' function result: "+a+". Use 'unsafeCSS' to pass non-literal values, but take care to ensure page security.")})(s)+e[r+1],e[0]);return new Rn(o,e,Tr)},pc=(e,t)=>{if(Er)e.adoptedStyleSheets=t.map(o=>o instanceof CSSStyleSheet?o:o.styleSheet);else for(const o of t){const i=document.createElement("style"),s=Qi.litNonce;s!==void 0&&i.setAttribute("nonce",s),i.textContent=o.cssText,e.appendChild(i)}},xa=Er?e=>e:e=>e instanceof CSSStyleSheet?(t=>{let o="";for(const i of t.cssRules)o+=i.cssText;return uc(o)})(e):e;/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const{is:fc,defineProperty:mc,getOwnPropertyDescriptor:gc,getOwnPropertyNames:bc,getOwnPropertySymbols:vc,getPrototypeOf:yc}=Object,Le=globalThis,ka=Le.trustedTypes,wc=ka?ka.emptyScript:"",Vs=Le.reactiveElementPolyfillSupport,ei=(e,t)=>e,$o={toAttribute(e,t){switch(t){case Boolean:e=e?wc:null;break;case Object:case Array:e=e==null?e:JSON.stringify(e)}return e},fromAttribute(e,t){let o=e;switch(t){case Boolean:o=e!==null;break;case Number:o=e===null?null:Number(e);break;case Object:case Array:try{o=JSON.parse(e)}catch{o=null}}return o}},Pr=(e,t)=>!fc(e,t),_a={attribute:!0,type:String,converter:$o,reflect:!1,hasChanged:Pr};Symbol.metadata??(Symbol.metadata=Symbol("metadata")),Le.litPropertyMetadata??(Le.litPropertyMetadata=new WeakMap);let vo=class extends HTMLElement{static addInitializer(t){this._$Ei(),(this.l??(this.l=[])).push(t)}static get observedAttributes(){return this.finalize(),this._$Eh&&[...this._$Eh.keys()]}static createProperty(t,o=_a){if(o.state&&(o.attribute=!1),this._$Ei(),this.elementProperties.set(t,o),!o.noAccessor){const i=Symbol(),s=this.getPropertyDescriptor(t,i,o);s!==void 0&&mc(this.prototype,t,s)}}static getPropertyDescriptor(t,o,i){const{get:s,set:r}=gc(this.prototype,t)??{get(){return this[o]},set(a){this[o]=a}};return{get(){return s==null?void 0:s.call(this)},set(a){const n=s==null?void 0:s.call(this);r.call(this,a),this.requestUpdate(t,n,i)},configurable:!0,enumerable:!0}}static getPropertyOptions(t){return this.elementProperties.get(t)??_a}static _$Ei(){if(this.hasOwnProperty(ei("elementProperties")))return;const t=yc(this);t.finalize(),t.l!==void 0&&(this.l=[...t.l]),this.elementProperties=new Map(t.elementProperties)}static finalize(){if(this.hasOwnProperty(ei("finalized")))return;if(this.finalized=!0,this._$Ei(),this.hasOwnProperty(ei("properties"))){const o=this.properties,i=[...bc(o),...vc(o)];for(const s of i)this.createProperty(s,o[s])}const t=this[Symbol.metadata];if(t!==null){const o=litPropertyMetadata.get(t);if(o!==void 0)for(const[i,s]of o)this.elementProperties.set(i,s)}this._$Eh=new Map;for(const[o,i]of this.elementProperties){const s=this._$Eu(o,i);s!==void 0&&this._$Eh.set(s,o)}this.elementStyles=this.finalizeStyles(this.styles)}static finalizeStyles(t){const o=[];if(Array.isArray(t)){const i=new Set(t.flat(1/0).reverse());for(const s of i)o.unshift(xa(s))}else t!==void 0&&o.push(xa(t));return o}static _$Eu(t,o){const i=o.attribute;return i===!1?void 0:typeof i=="string"?i:typeof t=="string"?t.toLowerCase():void 0}constructor(){super(),this._$Ep=void 0,this.isUpdatePending=!1,this.hasUpdated=!1,this._$Em=null,this._$Ev()}_$Ev(){var t;this._$ES=new Promise(o=>this.enableUpdating=o),this._$AL=new Map,this._$E_(),this.requestUpdate(),(t=this.constructor.l)==null||t.forEach(o=>o(this))}addController(t){var o;(this._$EO??(this._$EO=new Set)).add(t),this.renderRoot!==void 0&&this.isConnected&&((o=t.hostConnected)==null||o.call(t))}removeController(t){var o;(o=this._$EO)==null||o.delete(t)}_$E_(){const t=new Map,o=this.constructor.elementProperties;for(const i of o.keys())this.hasOwnProperty(i)&&(t.set(i,this[i]),delete this[i]);t.size>0&&(this._$Ep=t)}createRenderRoot(){const t=this.shadowRoot??this.attachShadow(this.constructor.shadowRootOptions);return pc(t,this.constructor.elementStyles),t}connectedCallback(){var t;this.renderRoot??(this.renderRoot=this.createRenderRoot()),this.enableUpdating(!0),(t=this._$EO)==null||t.forEach(o=>{var i;return(i=o.hostConnected)==null?void 0:i.call(o)})}enableUpdating(t){}disconnectedCallback(){var t;(t=this._$EO)==null||t.forEach(o=>{var i;return(i=o.hostDisconnected)==null?void 0:i.call(o)})}attributeChangedCallback(t,o,i){this._$AK(t,i)}_$EC(t,o){var r;const i=this.constructor.elementProperties.get(t),s=this.constructor._$Eu(t,i);if(s!==void 0&&i.reflect===!0){const a=(((r=i.converter)==null?void 0:r.toAttribute)!==void 0?i.converter:$o).toAttribute(o,i.type);this._$Em=t,a==null?this.removeAttribute(s):this.setAttribute(s,a),this._$Em=null}}_$AK(t,o){var r;const i=this.constructor,s=i._$Eh.get(t);if(s!==void 0&&this._$Em!==s){const a=i.getPropertyOptions(s),n=typeof a.converter=="function"?{fromAttribute:a.converter}:((r=a.converter)==null?void 0:r.fromAttribute)!==void 0?a.converter:$o;this._$Em=s,this[s]=n.fromAttribute(o,a.type),this._$Em=null}}requestUpdate(t,o,i){if(t!==void 0){if(i??(i=this.constructor.getPropertyOptions(t)),!(i.hasChanged??Pr)(this[t],o))return;this.P(t,o,i)}this.isUpdatePending===!1&&(this._$ES=this._$ET())}P(t,o,i){this._$AL.has(t)||this._$AL.set(t,o),i.reflect===!0&&this._$Em!==t&&(this._$Ej??(this._$Ej=new Set)).add(t)}async _$ET(){this.isUpdatePending=!0;try{await this._$ES}catch(o){Promise.reject(o)}const t=this.scheduleUpdate();return t!=null&&await t,!this.isUpdatePending}scheduleUpdate(){return this.performUpdate()}performUpdate(){var i;if(!this.isUpdatePending)return;if(!this.hasUpdated){if(this.renderRoot??(this.renderRoot=this.createRenderRoot()),this._$Ep){for(const[r,a]of this._$Ep)this[r]=a;this._$Ep=void 0}const s=this.constructor.elementProperties;if(s.size>0)for(const[r,a]of s)a.wrapped!==!0||this._$AL.has(r)||this[r]===void 0||this.P(r,this[r],a)}let t=!1;const o=this._$AL;try{t=this.shouldUpdate(o),t?(this.willUpdate(o),(i=this._$EO)==null||i.forEach(s=>{var r;return(r=s.hostUpdate)==null?void 0:r.call(s)}),this.update(o)):this._$EU()}catch(s){throw t=!1,this._$EU(),s}t&&this._$AE(o)}willUpdate(t){}_$AE(t){var o;(o=this._$EO)==null||o.forEach(i=>{var s;return(s=i.hostUpdated)==null?void 0:s.call(i)}),this.hasUpdated||(this.hasUpdated=!0,this.firstUpdated(t)),this.updated(t)}_$EU(){this._$AL=new Map,this.isUpdatePending=!1}get updateComplete(){return this.getUpdateComplete()}getUpdateComplete(){return this._$ES}shouldUpdate(t){return!0}update(t){this._$Ej&&(this._$Ej=this._$Ej.forEach(o=>this._$EC(o,this[o]))),this._$EU()}updated(t){}firstUpdated(t){}};vo.elementStyles=[],vo.shadowRootOptions={mode:"open"},vo[ei("elementProperties")]=new Map,vo[ei("finalized")]=new Map,Vs==null||Vs({ReactiveElement:vo}),(Le.reactiveElementVersions??(Le.reactiveElementVersions=[])).push("2.0.4");/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const oi=globalThis,os=oi.trustedTypes,$a=os?os.createPolicy("lit-html",{createHTML:e=>e}):void 0,Dn="$lit$",Te=`lit$${Math.random().toFixed(9).slice(2)}$`,Mn="?"+Te,xc=`<${Mn}>`,Je=document,fi=()=>Je.createComment(""),mi=e=>e===null||typeof e!="object"&&typeof e!="function",Or=Array.isArray,kc=e=>Or(e)||typeof(e==null?void 0:e[Symbol.iterator])=="function",Us=`[ 	
\f\r]`,jo=/<(?:(!--|\/[^a-zA-Z])|(\/?[a-zA-Z][^>\s]*)|(\/?$))/g,Ca=/-->/g,Sa=/>/g,He=RegExp(`>|${Us}(?:([^\\s"'>=/]+)(${Us}*=${Us}*(?:[^ 	
\f\r"'\`<>=]|("|')|))|$)`,"g"),Aa=/'/g,za=/"/g,In=/^(?:script|style|textarea|title)$/i,_c=e=>(t,...o)=>({_$litType$:e,strings:t,values:o}),x=_c(1),Bt=Symbol.for("lit-noChange"),et=Symbol.for("lit-nothing"),Ea=new WeakMap,Ye=Je.createTreeWalker(Je,129);function Bn(e,t){if(!Or(e)||!e.hasOwnProperty("raw"))throw Error("invalid template strings array");return $a!==void 0?$a.createHTML(t):t}const $c=(e,t)=>{const o=e.length-1,i=[];let s,r=t===2?"<svg>":t===3?"<math>":"",a=jo;for(let n=0;n<o;n++){const c=e[n];let d,u,h=-1,f=0;for(;f<c.length&&(a.lastIndex=f,u=a.exec(c),u!==null);)f=a.lastIndex,a===jo?u[1]==="!--"?a=Ca:u[1]!==void 0?a=Sa:u[2]!==void 0?(In.test(u[2])&&(s=RegExp("</"+u[2],"g")),a=He):u[3]!==void 0&&(a=He):a===He?u[0]===">"?(a=s??jo,h=-1):u[1]===void 0?h=-2:(h=a.lastIndex-u[2].length,d=u[1],a=u[3]===void 0?He:u[3]==='"'?za:Aa):a===za||a===Aa?a=He:a===Ca||a===Sa?a=jo:(a=He,s=void 0);const m=a===He&&e[n+1].startsWith("/>")?" ":"";r+=a===jo?c+xc:h>=0?(i.push(d),c.slice(0,h)+Dn+c.slice(h)+Te+m):c+Te+(h===-2?n:m)}return[Bn(e,r+(e[o]||"<?>")+(t===2?"</svg>":t===3?"</math>":"")),i]};let ar=class Fn{constructor({strings:t,_$litType$:o},i){let s;this.parts=[];let r=0,a=0;const n=t.length-1,c=this.parts,[d,u]=$c(t,o);if(this.el=Fn.createElement(d,i),Ye.currentNode=this.el.content,o===2||o===3){const h=this.el.content.firstChild;h.replaceWith(...h.childNodes)}for(;(s=Ye.nextNode())!==null&&c.length<n;){if(s.nodeType===1){if(s.hasAttributes())for(const h of s.getAttributeNames())if(h.endsWith(Dn)){const f=u[a++],m=s.getAttribute(h).split(Te),g=/([.?@])?(.*)/.exec(f);c.push({type:1,index:r,name:g[2],strings:m,ctor:g[1]==="."?Sc:g[1]==="?"?Ac:g[1]==="@"?zc:$s}),s.removeAttribute(h)}else h.startsWith(Te)&&(c.push({type:6,index:r}),s.removeAttribute(h));if(In.test(s.tagName)){const h=s.textContent.split(Te),f=h.length-1;if(f>0){s.textContent=os?os.emptyScript:"";for(let m=0;m<f;m++)s.append(h[m],fi()),Ye.nextNode(),c.push({type:2,index:++r});s.append(h[f],fi())}}}else if(s.nodeType===8)if(s.data===Mn)c.push({type:2,index:r});else{let h=-1;for(;(h=s.data.indexOf(Te,h+1))!==-1;)c.push({type:7,index:r}),h+=Te.length-1}r++}}static createElement(t,o){const i=Je.createElement("template");return i.innerHTML=t,i}};function Co(e,t,o=e,i){var a,n;if(t===Bt)return t;let s=i!==void 0?(a=o._$Co)==null?void 0:a[i]:o._$Cl;const r=mi(t)?void 0:t._$litDirective$;return(s==null?void 0:s.constructor)!==r&&((n=s==null?void 0:s._$AO)==null||n.call(s,!1),r===void 0?s=void 0:(s=new r(e),s._$AT(e,o,i)),i!==void 0?(o._$Co??(o._$Co=[]))[i]=s:o._$Cl=s),s!==void 0&&(t=Co(e,s._$AS(e,t.values),s,i)),t}let Cc=class{constructor(t,o){this._$AV=[],this._$AN=void 0,this._$AD=t,this._$AM=o}get parentNode(){return this._$AM.parentNode}get _$AU(){return this._$AM._$AU}u(t){const{el:{content:o},parts:i}=this._$AD,s=((t==null?void 0:t.creationScope)??Je).importNode(o,!0);Ye.currentNode=s;let r=Ye.nextNode(),a=0,n=0,c=i[0];for(;c!==void 0;){if(a===c.index){let d;c.type===2?d=new Lr(r,r.nextSibling,this,t):c.type===1?d=new c.ctor(r,c.name,c.strings,this,t):c.type===6&&(d=new Ec(r,this,t)),this._$AV.push(d),c=i[++n]}a!==(c==null?void 0:c.index)&&(r=Ye.nextNode(),a++)}return Ye.currentNode=Je,s}p(t){let o=0;for(const i of this._$AV)i!==void 0&&(i.strings!==void 0?(i._$AI(t,i,o),o+=i.strings.length-2):i._$AI(t[o])),o++}},Lr=class Vn{get _$AU(){var t;return((t=this._$AM)==null?void 0:t._$AU)??this._$Cv}constructor(t,o,i,s){this.type=2,this._$AH=et,this._$AN=void 0,this._$AA=t,this._$AB=o,this._$AM=i,this.options=s,this._$Cv=(s==null?void 0:s.isConnected)??!0}get parentNode(){let t=this._$AA.parentNode;const o=this._$AM;return o!==void 0&&(t==null?void 0:t.nodeType)===11&&(t=o.parentNode),t}get startNode(){return this._$AA}get endNode(){return this._$AB}_$AI(t,o=this){t=Co(this,t,o),mi(t)?t===et||t==null||t===""?(this._$AH!==et&&this._$AR(),this._$AH=et):t!==this._$AH&&t!==Bt&&this._(t):t._$litType$!==void 0?this.$(t):t.nodeType!==void 0?this.T(t):kc(t)?this.k(t):this._(t)}O(t){return this._$AA.parentNode.insertBefore(t,this._$AB)}T(t){this._$AH!==t&&(this._$AR(),this._$AH=this.O(t))}_(t){this._$AH!==et&&mi(this._$AH)?this._$AA.nextSibling.data=t:this.T(Je.createTextNode(t)),this._$AH=t}$(t){var r;const{values:o,_$litType$:i}=t,s=typeof i=="number"?this._$AC(t):(i.el===void 0&&(i.el=ar.createElement(Bn(i.h,i.h[0]),this.options)),i);if(((r=this._$AH)==null?void 0:r._$AD)===s)this._$AH.p(o);else{const a=new Cc(s,this),n=a.u(this.options);a.p(o),this.T(n),this._$AH=a}}_$AC(t){let o=Ea.get(t.strings);return o===void 0&&Ea.set(t.strings,o=new ar(t)),o}k(t){Or(this._$AH)||(this._$AH=[],this._$AR());const o=this._$AH;let i,s=0;for(const r of t)s===o.length?o.push(i=new Vn(this.O(fi()),this.O(fi()),this,this.options)):i=o[s],i._$AI(r),s++;s<o.length&&(this._$AR(i&&i._$AB.nextSibling,s),o.length=s)}_$AR(t=this._$AA.nextSibling,o){var i;for((i=this._$AP)==null?void 0:i.call(this,!1,!0,o);t&&t!==this._$AB;){const s=t.nextSibling;t.remove(),t=s}}setConnected(t){var o;this._$AM===void 0&&(this._$Cv=t,(o=this._$AP)==null||o.call(this,t))}},$s=class{get tagName(){return this.element.tagName}get _$AU(){return this._$AM._$AU}constructor(t,o,i,s,r){this.type=1,this._$AH=et,this._$AN=void 0,this.element=t,this.name=o,this._$AM=s,this.options=r,i.length>2||i[0]!==""||i[1]!==""?(this._$AH=Array(i.length-1).fill(new String),this.strings=i):this._$AH=et}_$AI(t,o=this,i,s){const r=this.strings;let a=!1;if(r===void 0)t=Co(this,t,o,0),a=!mi(t)||t!==this._$AH&&t!==Bt,a&&(this._$AH=t);else{const n=t;let c,d;for(t=r[0],c=0;c<r.length-1;c++)d=Co(this,n[i+c],o,c),d===Bt&&(d=this._$AH[c]),a||(a=!mi(d)||d!==this._$AH[c]),d===et?t=et:t!==et&&(t+=(d??"")+r[c+1]),this._$AH[c]=d}a&&!s&&this.j(t)}j(t){t===et?this.element.removeAttribute(this.name):this.element.setAttribute(this.name,t??"")}},Sc=class extends $s{constructor(){super(...arguments),this.type=3}j(t){this.element[this.name]=t===et?void 0:t}},Ac=class extends $s{constructor(){super(...arguments),this.type=4}j(t){this.element.toggleAttribute(this.name,!!t&&t!==et)}},zc=class extends $s{constructor(t,o,i,s,r){super(t,o,i,s,r),this.type=5}_$AI(t,o=this){if((t=Co(this,t,o,0)??et)===Bt)return;const i=this._$AH,s=t===et&&i!==et||t.capture!==i.capture||t.once!==i.once||t.passive!==i.passive,r=t!==et&&(i===et||s);s&&this.element.removeEventListener(this.name,this,i),r&&this.element.addEventListener(this.name,this,t),this._$AH=t}handleEvent(t){var o;typeof this._$AH=="function"?this._$AH.call(((o=this.options)==null?void 0:o.host)??this.element,t):this._$AH.handleEvent(t)}},Ec=class{constructor(t,o,i){this.element=t,this.type=6,this._$AN=void 0,this._$AM=o,this.options=i}get _$AU(){return this._$AM._$AU}_$AI(t){Co(this,t)}};const Ns=oi.litHtmlPolyfillSupport;Ns==null||Ns(ar,Lr),(oi.litHtmlVersions??(oi.litHtmlVersions=[])).push("3.2.1");const Tc=(e,t,o)=>{const i=(o==null?void 0:o.renderBefore)??t;let s=i._$litPart$;if(s===void 0){const r=(o==null?void 0:o.renderBefore)??null;i._$litPart$=s=new Lr(t.insertBefore(fi(),r),r,void 0,o??{})}return s._$AI(e),s};/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */let de=class extends vo{constructor(){super(...arguments),this.renderOptions={host:this},this._$Do=void 0}createRenderRoot(){var o;const t=super.createRenderRoot();return(o=this.renderOptions).renderBefore??(o.renderBefore=t.firstChild),t}update(t){const o=this.render();this.hasUpdated||(this.renderOptions.isConnected=this.isConnected),super.update(t),this._$Do=Tc(o,this.renderRoot,this.renderOptions)}connectedCallback(){var t;super.connectedCallback(),(t=this._$Do)==null||t.setConnected(!0)}disconnectedCallback(){var t;super.disconnectedCallback(),(t=this._$Do)==null||t.setConnected(!1)}render(){return Bt}};var On;de._$litElement$=!0,de.finalized=!0,(On=globalThis.litElementHydrateSupport)==null||On.call(globalThis,{LitElement:de});const Hs=globalThis.litElementPolyfillSupport;Hs==null||Hs({LitElement:de});(globalThis.litElementVersions??(globalThis.litElementVersions=[])).push("4.1.1");/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Si=e=>(t,o)=>{o!==void 0?o.addInitializer(()=>{customElements.define(e,t)}):customElements.define(e,t)};/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Pc={attribute:!0,type:String,converter:$o,reflect:!1,hasChanged:Pr},Oc=(e=Pc,t,o)=>{const{kind:i,metadata:s}=o;let r=globalThis.litPropertyMetadata.get(s);if(r===void 0&&globalThis.litPropertyMetadata.set(s,r=new Map),r.set(o.name,e),i==="accessor"){const{name:a}=o;return{set(n){const c=t.get.call(this);t.set.call(this,n),this.requestUpdate(a,c,e)},init(n){return n!==void 0&&this.P(a,void 0,e),n}}}if(i==="setter"){const{name:a}=o;return function(n){const c=this[a];t.call(this,n),this.requestUpdate(a,c,e)}}throw Error("Unsupported decorator location: "+i)};function p(e){return(t,o)=>typeof o=="object"?Oc(e,t,o):((i,s,r)=>{const a=s.hasOwnProperty(r);return s.constructor.createProperty(r,a?{...i,wrapped:!0}:i),a?Object.getOwnPropertyDescriptor(s,r):void 0})(e,t,o)}/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function z(e){return p({...e,state:!0,attribute:!1})}/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function Ai(e){return(t,o)=>{const i=typeof t=="function"?t:t[o];Object.assign(i,e)}}/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Un=(e,t,o)=>(o.configurable=!0,o.enumerable=!0,Reflect.decorate&&typeof t!="object"&&Object.defineProperty(e,t,o),o);/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function S(e,t){return(o,i,s)=>{const r=a=>{var n;return((n=a.renderRoot)==null?void 0:n.querySelector(e))??null};return Un(o,i,{get(){return r(this)}})}}/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function Lc(e){return(t,o)=>Un(t,o,{async get(){var i;return await this.updateComplete,((i=this.renderRoot)==null?void 0:i.querySelector(e))??null}})}/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Ta=new WeakMap,Pa=e=>{if((o=>o.pattern!==void 0)(e))return e.pattern;let t=Ta.get(e);return t===void 0&&Ta.set(e,t=new URLPattern({pathname:e.path})),t};let Rc=class{constructor(t,o,i){this.routes=[],this.o=[],this.t={},this.i=s=>{if(s.routes===this)return;const r=s.routes;this.o.push(r),r.h=this,s.stopImmediatePropagation(),s.onDisconnect=()=>{var n;(n=this.o)==null||n.splice(this.o.indexOf(r)>>>0,1)};const a=Oa(this.t);a!==void 0&&r.goto(a)},(this.l=t).addController(this),this.routes=[...o],this.fallback=i==null?void 0:i.fallback}link(t){var o;if(t!=null&&t.startsWith("/"))return t;if(t!=null&&t.startsWith("."))throw Error("Not implemented");return t??(t=this.u),(((o=this.h)==null?void 0:o.link())??"")+t}async goto(t){let o;if(this.routes.length===0&&this.fallback===void 0)o=t,this.u="",this.t={0:o};else{const i=this.p(t);if(i===void 0)throw Error("No route found for "+t);const s=Pa(i).exec({pathname:t}),r=(s==null?void 0:s.pathname.groups)??{};if(o=Oa(r),typeof i.enter=="function"&&await i.enter(r)===!1)return;this.v=i,this.t=r,this.u=o===void 0?t:t.substring(0,t.length-o.length)}if(o!==void 0)for(const i of this.o)i.goto(o);this.l.requestUpdate()}outlet(){var t,o;return(o=(t=this.v)==null?void 0:t.render)==null?void 0:o.call(t,this.t)}get params(){return this.t}p(t){const o=this.routes.find(i=>Pa(i).test({pathname:t}));return o||this.fallback===void 0?o:this.fallback?{...this.fallback,path:"/*"}:void 0}hostConnected(){this.l.addEventListener(nr.eventName,this.i);const t=new nr(this);this.l.dispatchEvent(t),this._=t.onDisconnect}hostDisconnected(){var t;(t=this._)==null||t.call(this),this.h=void 0}};const Oa=e=>{let t;for(const o of Object.keys(e))/\d+/.test(o)&&(t===void 0||o>t)&&(t=o);return t&&e[t]};let nr=class Nn extends Event{constructor(t){super(Nn.eventName,{bubbles:!0,composed:!0,cancelable:!1}),this.routes=t}};nr.eventName="lit-routes-connected";/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Dc=location.origin||location.protocol+"//"+location.host;let Mc=class extends Rc{constructor(){super(...arguments),this.m=t=>{const o=t.button!==0||t.metaKey||t.ctrlKey||t.shiftKey;if(t.defaultPrevented||o)return;const i=t.composedPath().find(a=>a.tagName==="A");if(i===void 0||i.target!==""||i.hasAttribute("download")||i.getAttribute("rel")==="external")return;const s=i.href;if(s===""||s.startsWith("mailto:"))return;const r=window.location;i.origin===Dc&&(t.preventDefault(),s!==r.href&&(window.history.pushState({},"",s),this.goto(i.pathname)))},this.R=t=>{this.goto(window.location.pathname)}}hostConnected(){super.hostConnected(),window.addEventListener("click",this.m),window.addEventListener("popstate",this.R),this.goto(window.location.pathname)}hostDisconnected(){super.hostDisconnected(),window.removeEventListener("click",this.m),window.removeEventListener("popstate",this.R)}};/**
 * @license
 * Copyright 2019 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Zi=globalThis,Rr=Zi.ShadowRoot&&(Zi.ShadyCSS===void 0||Zi.ShadyCSS.nativeShadow)&&"adoptedStyleSheets"in Document.prototype&&"replace"in CSSStyleSheet.prototype,Dr=Symbol(),La=new WeakMap;let Hn=class{constructor(t,o,i){if(this._$cssResult$=!0,i!==Dr)throw Error("CSSResult is not constructable. Use `unsafeCSS` or `css` instead.");this.cssText=t,this.t=o}get styleSheet(){let t=this.o;const o=this.t;if(Rr&&t===void 0){const i=o!==void 0&&o.length===1;i&&(t=La.get(o)),t===void 0&&((this.o=t=new CSSStyleSheet).replaceSync(this.cssText),i&&La.set(o,t))}return t}toString(){return this.cssText}};const Ic=e=>new Hn(typeof e=="string"?e:e+"",void 0,Dr),bt=(e,...t)=>{const o=e.length===1?e[0]:t.reduce((i,s,r)=>i+(a=>{if(a._$cssResult$===!0)return a.cssText;if(typeof a=="number")return a;throw Error("Value passed to 'css' function must be a 'css' function result: "+a+". Use 'unsafeCSS' to pass non-literal values, but take care to ensure page security.")})(s)+e[r+1],e[0]);return new Hn(o,e,Dr)},Bc=(e,t)=>{if(Rr)e.adoptedStyleSheets=t.map(o=>o instanceof CSSStyleSheet?o:o.styleSheet);else for(const o of t){const i=document.createElement("style"),s=Zi.litNonce;s!==void 0&&i.setAttribute("nonce",s),i.textContent=o.cssText,e.appendChild(i)}},Ra=Rr?e=>e:e=>e instanceof CSSStyleSheet?(t=>{let o="";for(const i of t.cssRules)o+=i.cssText;return Ic(o)})(e):e;/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const{is:Fc,defineProperty:Vc,getOwnPropertyDescriptor:Uc,getOwnPropertyNames:Nc,getOwnPropertySymbols:Hc,getPrototypeOf:jc}=Object,Re=globalThis,Da=Re.trustedTypes,qc=Da?Da.emptyScript:"",js=Re.reactiveElementPolyfillSupport,ii=(e,t)=>e,is={toAttribute(e,t){switch(t){case Boolean:e=e?qc:null;break;case Object:case Array:e=e==null?e:JSON.stringify(e)}return e},fromAttribute(e,t){let o=e;switch(t){case Boolean:o=e!==null;break;case Number:o=e===null?null:Number(e);break;case Object:case Array:try{o=JSON.parse(e)}catch{o=null}}return o}},Mr=(e,t)=>!Fc(e,t),Ma={attribute:!0,type:String,converter:is,reflect:!1,useDefault:!1,hasChanged:Mr};Symbol.metadata??(Symbol.metadata=Symbol("metadata")),Re.litPropertyMetadata??(Re.litPropertyMetadata=new WeakMap);let yo=class extends HTMLElement{static addInitializer(t){this._$Ei(),(this.l??(this.l=[])).push(t)}static get observedAttributes(){return this.finalize(),this._$Eh&&[...this._$Eh.keys()]}static createProperty(t,o=Ma){if(o.state&&(o.attribute=!1),this._$Ei(),this.prototype.hasOwnProperty(t)&&((o=Object.create(o)).wrapped=!0),this.elementProperties.set(t,o),!o.noAccessor){const i=Symbol(),s=this.getPropertyDescriptor(t,i,o);s!==void 0&&Vc(this.prototype,t,s)}}static getPropertyDescriptor(t,o,i){const{get:s,set:r}=Uc(this.prototype,t)??{get(){return this[o]},set(a){this[o]=a}};return{get:s,set(a){const n=s==null?void 0:s.call(this);r==null||r.call(this,a),this.requestUpdate(t,n,i)},configurable:!0,enumerable:!0}}static getPropertyOptions(t){return this.elementProperties.get(t)??Ma}static _$Ei(){if(this.hasOwnProperty(ii("elementProperties")))return;const t=jc(this);t.finalize(),t.l!==void 0&&(this.l=[...t.l]),this.elementProperties=new Map(t.elementProperties)}static finalize(){if(this.hasOwnProperty(ii("finalized")))return;if(this.finalized=!0,this._$Ei(),this.hasOwnProperty(ii("properties"))){const o=this.properties,i=[...Nc(o),...Hc(o)];for(const s of i)this.createProperty(s,o[s])}const t=this[Symbol.metadata];if(t!==null){const o=litPropertyMetadata.get(t);if(o!==void 0)for(const[i,s]of o)this.elementProperties.set(i,s)}this._$Eh=new Map;for(const[o,i]of this.elementProperties){const s=this._$Eu(o,i);s!==void 0&&this._$Eh.set(s,o)}this.elementStyles=this.finalizeStyles(this.styles)}static finalizeStyles(t){const o=[];if(Array.isArray(t)){const i=new Set(t.flat(1/0).reverse());for(const s of i)o.unshift(Ra(s))}else t!==void 0&&o.push(Ra(t));return o}static _$Eu(t,o){const i=o.attribute;return i===!1?void 0:typeof i=="string"?i:typeof t=="string"?t.toLowerCase():void 0}constructor(){super(),this._$Ep=void 0,this.isUpdatePending=!1,this.hasUpdated=!1,this._$Em=null,this._$Ev()}_$Ev(){var t;this._$ES=new Promise(o=>this.enableUpdating=o),this._$AL=new Map,this._$E_(),this.requestUpdate(),(t=this.constructor.l)==null||t.forEach(o=>o(this))}addController(t){var o;(this._$EO??(this._$EO=new Set)).add(t),this.renderRoot!==void 0&&this.isConnected&&((o=t.hostConnected)==null||o.call(t))}removeController(t){var o;(o=this._$EO)==null||o.delete(t)}_$E_(){const t=new Map,o=this.constructor.elementProperties;for(const i of o.keys())this.hasOwnProperty(i)&&(t.set(i,this[i]),delete this[i]);t.size>0&&(this._$Ep=t)}createRenderRoot(){const t=this.shadowRoot??this.attachShadow(this.constructor.shadowRootOptions);return Bc(t,this.constructor.elementStyles),t}connectedCallback(){var t;this.renderRoot??(this.renderRoot=this.createRenderRoot()),this.enableUpdating(!0),(t=this._$EO)==null||t.forEach(o=>{var i;return(i=o.hostConnected)==null?void 0:i.call(o)})}enableUpdating(t){}disconnectedCallback(){var t;(t=this._$EO)==null||t.forEach(o=>{var i;return(i=o.hostDisconnected)==null?void 0:i.call(o)})}attributeChangedCallback(t,o,i){this._$AK(t,i)}_$ET(t,o){var r;const i=this.constructor.elementProperties.get(t),s=this.constructor._$Eu(t,i);if(s!==void 0&&i.reflect===!0){const a=(((r=i.converter)==null?void 0:r.toAttribute)!==void 0?i.converter:is).toAttribute(o,i.type);this._$Em=t,a==null?this.removeAttribute(s):this.setAttribute(s,a),this._$Em=null}}_$AK(t,o){var r,a;const i=this.constructor,s=i._$Eh.get(t);if(s!==void 0&&this._$Em!==s){const n=i.getPropertyOptions(s),c=typeof n.converter=="function"?{fromAttribute:n.converter}:((r=n.converter)==null?void 0:r.fromAttribute)!==void 0?n.converter:is;this._$Em=s,this[s]=c.fromAttribute(o,n.type)??((a=this._$Ej)==null?void 0:a.get(s))??null,this._$Em=null}}requestUpdate(t,o,i){var s;if(t!==void 0){const r=this.constructor,a=this[t];if(i??(i=r.getPropertyOptions(t)),!((i.hasChanged??Mr)(a,o)||i.useDefault&&i.reflect&&a===((s=this._$Ej)==null?void 0:s.get(t))&&!this.hasAttribute(r._$Eu(t,i))))return;this.C(t,o,i)}this.isUpdatePending===!1&&(this._$ES=this._$EP())}C(t,o,{useDefault:i,reflect:s,wrapped:r},a){i&&!(this._$Ej??(this._$Ej=new Map)).has(t)&&(this._$Ej.set(t,a??o??this[t]),r!==!0||a!==void 0)||(this._$AL.has(t)||(this.hasUpdated||i||(o=void 0),this._$AL.set(t,o)),s===!0&&this._$Em!==t&&(this._$Eq??(this._$Eq=new Set)).add(t))}async _$EP(){this.isUpdatePending=!0;try{await this._$ES}catch(o){Promise.reject(o)}const t=this.scheduleUpdate();return t!=null&&await t,!this.isUpdatePending}scheduleUpdate(){return this.performUpdate()}performUpdate(){var i;if(!this.isUpdatePending)return;if(!this.hasUpdated){if(this.renderRoot??(this.renderRoot=this.createRenderRoot()),this._$Ep){for(const[r,a]of this._$Ep)this[r]=a;this._$Ep=void 0}const s=this.constructor.elementProperties;if(s.size>0)for(const[r,a]of s){const{wrapped:n}=a,c=this[r];n!==!0||this._$AL.has(r)||c===void 0||this.C(r,void 0,a,c)}}let t=!1;const o=this._$AL;try{t=this.shouldUpdate(o),t?(this.willUpdate(o),(i=this._$EO)==null||i.forEach(s=>{var r;return(r=s.hostUpdate)==null?void 0:r.call(s)}),this.update(o)):this._$EM()}catch(s){throw t=!1,this._$EM(),s}t&&this._$AE(o)}willUpdate(t){}_$AE(t){var o;(o=this._$EO)==null||o.forEach(i=>{var s;return(s=i.hostUpdated)==null?void 0:s.call(i)}),this.hasUpdated||(this.hasUpdated=!0,this.firstUpdated(t)),this.updated(t)}_$EM(){this._$AL=new Map,this.isUpdatePending=!1}get updateComplete(){return this.getUpdateComplete()}getUpdateComplete(){return this._$ES}shouldUpdate(t){return!0}update(t){this._$Eq&&(this._$Eq=this._$Eq.forEach(o=>this._$ET(o,this[o]))),this._$EM()}updated(t){}firstUpdated(t){}};yo.elementStyles=[],yo.shadowRootOptions={mode:"open"},yo[ii("elementProperties")]=new Map,yo[ii("finalized")]=new Map,js==null||js({ReactiveElement:yo}),(Re.reactiveElementVersions??(Re.reactiveElementVersions=[])).push("2.1.0");/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const si=globalThis,ss=si.trustedTypes,Ia=ss?ss.createPolicy("lit-html",{createHTML:e=>e}):void 0,jn="$lit$",Pe=`lit$${Math.random().toFixed(9).slice(2)}$`,qn="?"+Pe,Wc=`<${qn}>`,to=document,gi=()=>to.createComment(""),bi=e=>e===null||typeof e!="object"&&typeof e!="function",Ir=Array.isArray,Kc=e=>Ir(e)||typeof(e==null?void 0:e[Symbol.iterator])=="function",qs=`[ 	
\f\r]`,qo=/<(?:(!--|\/[^a-zA-Z])|(\/?[a-zA-Z][^>\s]*)|(\/?$))/g,Ba=/-->/g,Fa=/>/g,je=RegExp(`>|${qs}(?:([^\\s"'>=/]+)(${qs}*=${qs}*(?:[^ 	
\f\r"'\`<>=]|("|')|))|$)`,"g"),Va=/'/g,Ua=/"/g,Wn=/^(?:script|style|textarea|title)$/i,Yc=e=>(t,...o)=>({_$litType$:e,strings:t,values:o}),B=Yc(1),pe=Symbol.for("lit-noChange"),U=Symbol.for("lit-nothing"),Na=new WeakMap,Xe=to.createTreeWalker(to,129);function Kn(e,t){if(!Ir(e)||!e.hasOwnProperty("raw"))throw Error("invalid template strings array");return Ia!==void 0?Ia.createHTML(t):t}const Xc=(e,t)=>{const o=e.length-1,i=[];let s,r=t===2?"<svg>":t===3?"<math>":"",a=qo;for(let n=0;n<o;n++){const c=e[n];let d,u,h=-1,f=0;for(;f<c.length&&(a.lastIndex=f,u=a.exec(c),u!==null);)f=a.lastIndex,a===qo?u[1]==="!--"?a=Ba:u[1]!==void 0?a=Fa:u[2]!==void 0?(Wn.test(u[2])&&(s=RegExp("</"+u[2],"g")),a=je):u[3]!==void 0&&(a=je):a===je?u[0]===">"?(a=s??qo,h=-1):u[1]===void 0?h=-2:(h=a.lastIndex-u[2].length,d=u[1],a=u[3]===void 0?je:u[3]==='"'?Ua:Va):a===Ua||a===Va?a=je:a===Ba||a===Fa?a=qo:(a=je,s=void 0);const m=a===je&&e[n+1].startsWith("/>")?" ":"";r+=a===qo?c+Wc:h>=0?(i.push(d),c.slice(0,h)+jn+c.slice(h)+Pe+m):c+Pe+(h===-2?n:m)}return[Kn(e,r+(e[o]||"<?>")+(t===2?"</svg>":t===3?"</math>":"")),i]};class vi{constructor({strings:t,_$litType$:o},i){let s;this.parts=[];let r=0,a=0;const n=t.length-1,c=this.parts,[d,u]=Xc(t,o);if(this.el=vi.createElement(d,i),Xe.currentNode=this.el.content,o===2||o===3){const h=this.el.content.firstChild;h.replaceWith(...h.childNodes)}for(;(s=Xe.nextNode())!==null&&c.length<n;){if(s.nodeType===1){if(s.hasAttributes())for(const h of s.getAttributeNames())if(h.endsWith(jn)){const f=u[a++],m=s.getAttribute(h).split(Pe),g=/([.?@])?(.*)/.exec(f);c.push({type:1,index:r,name:g[2],strings:m,ctor:g[1]==="."?Zc:g[1]==="?"?Gc:g[1]==="@"?Jc:Cs}),s.removeAttribute(h)}else h.startsWith(Pe)&&(c.push({type:6,index:r}),s.removeAttribute(h));if(Wn.test(s.tagName)){const h=s.textContent.split(Pe),f=h.length-1;if(f>0){s.textContent=ss?ss.emptyScript:"";for(let m=0;m<f;m++)s.append(h[m],gi()),Xe.nextNode(),c.push({type:2,index:++r});s.append(h[f],gi())}}}else if(s.nodeType===8)if(s.data===qn)c.push({type:2,index:r});else{let h=-1;for(;(h=s.data.indexOf(Pe,h+1))!==-1;)c.push({type:7,index:r}),h+=Pe.length-1}r++}}static createElement(t,o){const i=to.createElement("template");return i.innerHTML=t,i}}function So(e,t,o=e,i){var a,n;if(t===pe)return t;let s=i!==void 0?(a=o._$Co)==null?void 0:a[i]:o._$Cl;const r=bi(t)?void 0:t._$litDirective$;return(s==null?void 0:s.constructor)!==r&&((n=s==null?void 0:s._$AO)==null||n.call(s,!1),r===void 0?s=void 0:(s=new r(e),s._$AT(e,o,i)),i!==void 0?(o._$Co??(o._$Co=[]))[i]=s:o._$Cl=s),s!==void 0&&(t=So(e,s._$AS(e,t.values),s,i)),t}let Qc=class{constructor(t,o){this._$AV=[],this._$AN=void 0,this._$AD=t,this._$AM=o}get parentNode(){return this._$AM.parentNode}get _$AU(){return this._$AM._$AU}u(t){const{el:{content:o},parts:i}=this._$AD,s=((t==null?void 0:t.creationScope)??to).importNode(o,!0);Xe.currentNode=s;let r=Xe.nextNode(),a=0,n=0,c=i[0];for(;c!==void 0;){if(a===c.index){let d;c.type===2?d=new Lo(r,r.nextSibling,this,t):c.type===1?d=new c.ctor(r,c.name,c.strings,this,t):c.type===6&&(d=new td(r,this,t)),this._$AV.push(d),c=i[++n]}a!==(c==null?void 0:c.index)&&(r=Xe.nextNode(),a++)}return Xe.currentNode=to,s}p(t){let o=0;for(const i of this._$AV)i!==void 0&&(i.strings!==void 0?(i._$AI(t,i,o),o+=i.strings.length-2):i._$AI(t[o])),o++}};class Lo{get _$AU(){var t;return((t=this._$AM)==null?void 0:t._$AU)??this._$Cv}constructor(t,o,i,s){this.type=2,this._$AH=U,this._$AN=void 0,this._$AA=t,this._$AB=o,this._$AM=i,this.options=s,this._$Cv=(s==null?void 0:s.isConnected)??!0}get parentNode(){let t=this._$AA.parentNode;const o=this._$AM;return o!==void 0&&(t==null?void 0:t.nodeType)===11&&(t=o.parentNode),t}get startNode(){return this._$AA}get endNode(){return this._$AB}_$AI(t,o=this){t=So(this,t,o),bi(t)?t===U||t==null||t===""?(this._$AH!==U&&this._$AR(),this._$AH=U):t!==this._$AH&&t!==pe&&this._(t):t._$litType$!==void 0?this.$(t):t.nodeType!==void 0?this.T(t):Kc(t)?this.k(t):this._(t)}O(t){return this._$AA.parentNode.insertBefore(t,this._$AB)}T(t){this._$AH!==t&&(this._$AR(),this._$AH=this.O(t))}_(t){this._$AH!==U&&bi(this._$AH)?this._$AA.nextSibling.data=t:this.T(to.createTextNode(t)),this._$AH=t}$(t){var r;const{values:o,_$litType$:i}=t,s=typeof i=="number"?this._$AC(t):(i.el===void 0&&(i.el=vi.createElement(Kn(i.h,i.h[0]),this.options)),i);if(((r=this._$AH)==null?void 0:r._$AD)===s)this._$AH.p(o);else{const a=new Qc(s,this),n=a.u(this.options);a.p(o),this.T(n),this._$AH=a}}_$AC(t){let o=Na.get(t.strings);return o===void 0&&Na.set(t.strings,o=new vi(t)),o}k(t){Ir(this._$AH)||(this._$AH=[],this._$AR());const o=this._$AH;let i,s=0;for(const r of t)s===o.length?o.push(i=new Lo(this.O(gi()),this.O(gi()),this,this.options)):i=o[s],i._$AI(r),s++;s<o.length&&(this._$AR(i&&i._$AB.nextSibling,s),o.length=s)}_$AR(t=this._$AA.nextSibling,o){var i;for((i=this._$AP)==null?void 0:i.call(this,!1,!0,o);t&&t!==this._$AB;){const s=t.nextSibling;t.remove(),t=s}}setConnected(t){var o;this._$AM===void 0&&(this._$Cv=t,(o=this._$AP)==null||o.call(this,t))}}class Cs{get tagName(){return this.element.tagName}get _$AU(){return this._$AM._$AU}constructor(t,o,i,s,r){this.type=1,this._$AH=U,this._$AN=void 0,this.element=t,this.name=o,this._$AM=s,this.options=r,i.length>2||i[0]!==""||i[1]!==""?(this._$AH=Array(i.length-1).fill(new String),this.strings=i):this._$AH=U}_$AI(t,o=this,i,s){const r=this.strings;let a=!1;if(r===void 0)t=So(this,t,o,0),a=!bi(t)||t!==this._$AH&&t!==pe,a&&(this._$AH=t);else{const n=t;let c,d;for(t=r[0],c=0;c<r.length-1;c++)d=So(this,n[i+c],o,c),d===pe&&(d=this._$AH[c]),a||(a=!bi(d)||d!==this._$AH[c]),d===U?t=U:t!==U&&(t+=(d??"")+r[c+1]),this._$AH[c]=d}a&&!s&&this.j(t)}j(t){t===U?this.element.removeAttribute(this.name):this.element.setAttribute(this.name,t??"")}}let Zc=class extends Cs{constructor(){super(...arguments),this.type=3}j(t){this.element[this.name]=t===U?void 0:t}};class Gc extends Cs{constructor(){super(...arguments),this.type=4}j(t){this.element.toggleAttribute(this.name,!!t&&t!==U)}}class Jc extends Cs{constructor(t,o,i,s,r){super(t,o,i,s,r),this.type=5}_$AI(t,o=this){if((t=So(this,t,o,0)??U)===pe)return;const i=this._$AH,s=t===U&&i!==U||t.capture!==i.capture||t.once!==i.once||t.passive!==i.passive,r=t!==U&&(i===U||s);s&&this.element.removeEventListener(this.name,this,i),r&&this.element.addEventListener(this.name,this,t),this._$AH=t}handleEvent(t){var o;typeof this._$AH=="function"?this._$AH.call(((o=this.options)==null?void 0:o.host)??this.element,t):this._$AH.handleEvent(t)}}class td{constructor(t,o,i){this.element=t,this.type=6,this._$AN=void 0,this._$AM=o,this.options=i}get _$AU(){return this._$AM._$AU}_$AI(t){So(this,t)}}const ed={I:Lo},Ws=si.litHtmlPolyfillSupport;Ws==null||Ws(vi,Lo),(si.litHtmlVersions??(si.litHtmlVersions=[])).push("3.3.0");const od=(e,t,o)=>{const i=(o==null?void 0:o.renderBefore)??t;let s=i._$litPart$;if(s===void 0){const r=(o==null?void 0:o.renderBefore)??null;i._$litPart$=s=new Lo(t.insertBefore(gi(),r),r,void 0,o??{})}return s._$AI(e),s};/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Ze=globalThis;let nt=class extends yo{constructor(){super(...arguments),this.renderOptions={host:this},this._$Do=void 0}createRenderRoot(){var o;const t=super.createRenderRoot();return(o=this.renderOptions).renderBefore??(o.renderBefore=t.firstChild),t}update(t){const o=this.render();this.hasUpdated||(this.renderOptions.isConnected=this.isConnected),super.update(t),this._$Do=od(o,this.renderRoot,this.renderOptions)}connectedCallback(){var t;super.connectedCallback(),(t=this._$Do)==null||t.setConnected(!0)}disconnectedCallback(){var t;super.disconnectedCallback(),(t=this._$Do)==null||t.setConnected(!1)}render(){return pe}};var Ln;nt._$litElement$=!0,nt.finalized=!0,(Ln=Ze.litElementHydrateSupport)==null||Ln.call(Ze,{LitElement:nt});const Ks=Ze.litElementPolyfillSupport;Ks==null||Ks({LitElement:nt});(Ze.litElementVersions??(Ze.litElementVersions=[])).push("4.2.0");/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const vt=e=>(t,o)=>{o!==void 0?o.addInitializer(()=>{customElements.define(e,t)}):customElements.define(e,t)};/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const id={attribute:!0,type:String,converter:is,reflect:!1,hasChanged:Mr},sd=(e=id,t,o)=>{const{kind:i,metadata:s}=o;let r=globalThis.litPropertyMetadata.get(s);if(r===void 0&&globalThis.litPropertyMetadata.set(s,r=new Map),i==="setter"&&((e=Object.create(e)).wrapped=!0),r.set(o.name,e),i==="accessor"){const{name:a}=o;return{set(n){const c=t.get.call(this);t.set.call(this,n),this.requestUpdate(a,c,e)},init(n){return n!==void 0&&this.C(a,void 0,e,n),n}}}if(i==="setter"){const{name:a}=o;return function(n){const c=this[a];t.call(this,n),this.requestUpdate(a,c,e)}}throw Error("Unsupported decorator location: "+i)};function F(e){return(t,o)=>typeof o=="object"?sd(e,t,o):((i,s,r)=>{const a=s.hasOwnProperty(r);return s.constructor.createProperty(r,i),a?Object.getOwnPropertyDescriptor(s,r):void 0})(e,t,o)}/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function Yn(e){return F({...e,state:!0,attribute:!1})}/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const rd=(e,t,o)=>(o.configurable=!0,o.enumerable=!0,Reflect.decorate&&typeof t!="object"&&Object.defineProperty(e,t,o),o);/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function no(e,t){return(o,i,s)=>{const r=a=>{var n;return((n=a.renderRoot)==null?void 0:n.querySelector(e))??null};return rd(o,i,{get(){return r(this)}})}}var ad=Object.defineProperty,nd=Object.getOwnPropertyDescriptor,Xn=(e,t,o,i)=>{for(var s=i>1?void 0:i?nd(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&ad(t,o,s),s};let rs=class extends nt{render(){return this.value?B`${this.value}`:U}};rs.styles=bt`
    :host {
      display: block;
      width: 100%;
      
      /* Font styles */
      font-family: auto;
      font-size: 14px;
      font-weight: 400;
      font-style: normal;
      font-variant: normal;

      /* Color styles */
      color: var(--uc-text-color-high);
      background-color: transparent;
      text-shadow: none;

      /* Alignment styles */
      text-align: left;
      text-decoration: none;
      line-height: 1.5;
      white-space: pre-wrap;
      word-wrap: break-word;
      letter-spacing: normal;
      word-spacing: normal;

      /* Overflow styles */
      overflow: unset; /* 숨김 처리 */
      overflow-wrap: anywhere; /* 텍스트 오버 플로우 처리 */
      text-overflow: unset; /* 텍스트 오버플로우 스타일 처리 */

      white-space: pre-wrap; /* 텍스트의 공백 문자 처리 */

      word-break: break-all; /* 단어 줄 바꿈 처리 */
      word-spacing: normal; /* 단어 간격 */
    }
  `;Xn([F({type:String})],rs.prototype,"value",2);rs=Xn([vt("text-block")],rs);var ld=Object.defineProperty,cd=Object.getOwnPropertyDescriptor,zi=(e,t,o,i)=>{for(var s=i>1?void 0:i?cd(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&ld(t,o,s),s};let eo=class extends nt{constructor(){super(...arguments),this.loading=!1,this.collapsed=!0,this.toggle=()=>{this.loading||(this.collapsed=!this.collapsed)}}updated(e){super.updated(e),e.has("value")&&this.value&&this.loading&&this.bodyEl&&requestAnimationFrame(()=>{this.bodyEl.scrollTo({top:this.bodyEl.scrollHeight,behavior:"smooth"})})}render(){return this.value?B`
      <div class="container">
        <div class="header" @click=${this.toggle}>
          ${this.loading?B`<div class="title">🤔 Thinking <span class="dots"></span></div>`:B`<div class="title">💡 Thought</div>
              ${this.collapsed?B`<uc-icon name="plus"></uc-icon>`:B`<uc-icon name="minus"></uc-icon>`}`}
        </div>
        <div class="body">
          ${this.value}
        </div>
      </div>
    `:U}};eo.styles=bt`
    :host {
      display: block;
      width: 100%;
      height: auto;

      --font-size: 14px;
      --line-height: 21px;
      --max-height: 260px;
    }
    :host([loading]) .header {
      cursor: wait;
    }
    :host([loading]) .body {
      height: calc(var(--line-height) * 3 + 14px);
      max-height: calc(var(--line-height) * 3 + 14px);
      overflow: hidden !important;
      animation: pulse 1.5s infinite;
      cursor: wait;
    }
    :host(:not([loading])[collapsed]) .body {
      max-height: 0;
      padding: 0;
      overflow: hidden;
    }

    .container {
      display: block;
      border-radius: 8px;
      border: 1px solid var(--uc-border-color-mid);
      box-sizing: border-box;
    }

    .header {
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      padding: 5px 10px;
      box-sizing: border-box;
      cursor: pointer;
    }
    .header .title {
      font-size: 16px;
      font-weight: 600;
      line-height: 24px;
    }
    .header .dots::after {
      content: '';
      animation: dots 1.5s infinite;
    }

    .body {
      height: auto;
      max-height: var(--max-height);
      font-weight: 300;
      font-size: var(--font-size);
      line-height: var(--line-height);
      padding: 14px;
      box-sizing: border-box;
      overflow: auto;
      overflow-wrap: anywhere;
      transition: max-height 0.3s ease, padding 0.3s ease;
    }

    @keyframes dots {
      0%, 20% { content: ''; }
      40% { content: '.'; }
      60% { content: '..'; }
      80%, 100% { content: '...'; }
    }

    @keyframes pulse {
      0% {
        box-shadow: inset 0px -20px 20px -15px var(--uc-shadow-color-low);
      }
      50% {
        box-shadow: inset 0px -20px 20px -15px var(--uc-shadow-color-high);
      }
      100% {
        box-shadow: inset 0px -20px 20px -15px var(--uc-shadow-color-low);
      }
    }
  `;zi([no(".body")],eo.prototype,"bodyEl",2);zi([F({type:String})],eo.prototype,"value",2);zi([F({type:Boolean,reflect:!0})],eo.prototype,"loading",2);zi([F({type:Boolean,reflect:!0})],eo.prototype,"collapsed",2);eo=zi([vt("thinking-block")],eo);/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Br={CHILD:2},Fr=e=>(...t)=>({_$litDirective$:e,values:t});let Vr=class{constructor(t){}get _$AU(){return this._$AM._$AU}_$AT(t,o,i){this._$Ct=t,this._$AM=o,this._$Ci=i}_$AS(t,o){return this.update(t,o)}update(t,o){return this.render(...o)}};/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */let lr=class extends Vr{constructor(t){if(super(t),this.it=U,t.type!==Br.CHILD)throw Error(this.constructor.directiveName+"() can only be used in child bindings")}render(t){if(t===U||t==null)return this._t=void 0,this.it=t;if(t===pe)return t;if(typeof t!="string")throw Error(this.constructor.directiveName+"() called with a non-string value");if(t===this.it)return this._t;this.it=t;const o=[t];return o.raw=o,this._t={_$litType$:this.constructor.resultType,strings:o,values:[]}}};lr.directiveName="unsafeHTML",lr.resultType=1;const Qn=Fr(lr);/**
 * @license
 * Copyright 2020 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const{I:dd}=ed,hd=e=>e===null||typeof e!="object"&&typeof e!="function",ud=e=>e.strings===void 0,Ha=()=>document.createComment(""),Wo=(e,t,o)=>{var r;const i=e._$AA.parentNode,s=t===void 0?e._$AB:t._$AA;if(o===void 0){const a=i.insertBefore(Ha(),s),n=i.insertBefore(Ha(),s);o=new dd(a,n,e,e.options)}else{const a=o._$AB.nextSibling,n=o._$AM,c=n!==e;if(c){let d;(r=o._$AQ)==null||r.call(o,e),o._$AM=e,o._$AP!==void 0&&(d=e._$AU)!==n._$AU&&o._$AP(d)}if(a!==s||c){let d=o._$AA;for(;d!==a;){const u=d.nextSibling;i.insertBefore(d,s),d=u}}}return o},qe=(e,t,o=e)=>(e._$AI(t,o),e),pd={},fd=(e,t=pd)=>e._$AH=t,md=e=>e._$AH,Ys=e=>{var i;(i=e._$AP)==null||i.call(e,!1,!0);let t=e._$AA;const o=e._$AB.nextSibling;for(;t!==o;){const s=t.nextSibling;t.remove(),t=s}};/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const ri=(e,t)=>{var i;const o=e._$AN;if(o===void 0)return!1;for(const s of o)(i=s._$AO)==null||i.call(s,t,!1),ri(s,t);return!0},as=e=>{let t,o;do{if((t=e._$AM)===void 0)break;o=t._$AN,o.delete(e),e=t}while((o==null?void 0:o.size)===0)},Zn=e=>{for(let t;t=e._$AM;e=t){let o=t._$AN;if(o===void 0)t._$AN=o=new Set;else if(o.has(e))break;o.add(e),vd(t)}};function gd(e){this._$AN!==void 0?(as(this),this._$AM=e,Zn(this)):this._$AM=e}function bd(e,t=!1,o=0){const i=this._$AH,s=this._$AN;if(s!==void 0&&s.size!==0)if(t)if(Array.isArray(i))for(let r=o;r<i.length;r++)ri(i[r],!1),as(i[r]);else i!=null&&(ri(i,!1),as(i));else ri(this,e)}const vd=e=>{e.type==Br.CHILD&&(e._$AP??(e._$AP=bd),e._$AQ??(e._$AQ=gd))};let yd=class extends Vr{constructor(){super(...arguments),this._$AN=void 0}_$AT(t,o,i){super._$AT(t,o,i),Zn(this),this.isConnected=t._$AU}_$AO(t,o=!0){var i,s;t!==this.isConnected&&(this.isConnected=t,t?(i=this.reconnected)==null||i.call(this):(s=this.disconnected)==null||s.call(this)),o&&(ri(this,t),as(this))}setValue(t){if(ud(this._$Ct))this._$Ct._$AI(t,this);else{const o=[...this._$Ct._$AH];o[this._$Ci]=t,this._$Ct._$AI(o,this,0)}}disconnected(){}reconnected(){}};/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */let wd=class{constructor(t){this.G=t}disconnect(){this.G=void 0}reconnect(t){this.G=t}deref(){return this.G}},xd=class{constructor(){this.Y=void 0,this.Z=void 0}get(){return this.Y}pause(){this.Y??(this.Y=new Promise(t=>this.Z=t))}resume(){var t;(t=this.Z)==null||t.call(this),this.Y=this.Z=void 0}};/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const ja=e=>!hd(e)&&typeof e.then=="function",qa=1073741823;let kd=class extends yd{constructor(){super(...arguments),this._$Cwt=qa,this._$Cbt=[],this._$CK=new wd(this),this._$CX=new xd}render(...t){return t.find(o=>!ja(o))??pe}update(t,o){const i=this._$Cbt;let s=i.length;this._$Cbt=o;const r=this._$CK,a=this._$CX;this.isConnected||this.disconnected();for(let n=0;n<o.length&&!(n>this._$Cwt);n++){const c=o[n];if(!ja(c))return this._$Cwt=n,c;n<s&&c===i[n]||(this._$Cwt=qa,s=0,Promise.resolve(c).then(async d=>{for(;a.get();)await a.get();const u=r.deref();if(u!==void 0){const h=u._$Cbt.indexOf(c);h>-1&&h<u._$Cwt&&(u._$Cwt=h,u.setValue(d))}}))}return pe}disconnected(){this._$CK.disconnect(),this._$CX.pause()}reconnected(){this._$CK.reconnect(this),this._$CX.resume()}};const _d=Fr(kd);function Ur(){return{async:!1,breaks:!1,extensions:null,gfm:!0,hooks:null,pedantic:!1,renderer:null,silent:!1,tokenizer:null,walkTokens:null}}let lo=Ur();function Gn(e){lo=e}const ai={exec:()=>null};function W(e,t=""){let o=typeof e=="string"?e:e.source;const i={replace:(s,r)=>{let a=typeof r=="string"?r:r.source;return a=a.replace(Tt.caret,"$1"),o=o.replace(s,a),i},getRegex:()=>new RegExp(o,t)};return i}const Tt={codeRemoveIndent:/^(?: {1,4}| {0,3}\t)/gm,outputLinkReplace:/\\([\[\]])/g,indentCodeCompensation:/^(\s+)(?:```)/,beginningSpace:/^\s+/,endingHash:/#$/,startingSpaceChar:/^ /,endingSpaceChar:/ $/,nonSpaceChar:/[^ ]/,newLineCharGlobal:/\n/g,tabCharGlobal:/\t/g,multipleSpaceGlobal:/\s+/g,blankLine:/^[ \t]*$/,doubleBlankLine:/\n[ \t]*\n[ \t]*$/,blockquoteStart:/^ {0,3}>/,blockquoteSetextReplace:/\n {0,3}((?:=+|-+) *)(?=\n|$)/g,blockquoteSetextReplace2:/^ {0,3}>[ \t]?/gm,listReplaceTabs:/^\t+/,listReplaceNesting:/^ {1,4}(?=( {4})*[^ ])/g,listIsTask:/^\[[ xX]\] /,listReplaceTask:/^\[[ xX]\] +/,anyLine:/\n.*\n/,hrefBrackets:/^<(.*)>$/,tableDelimiter:/[:|]/,tableAlignChars:/^\||\| *$/g,tableRowBlankLine:/\n[ \t]*$/,tableAlignRight:/^ *-+: *$/,tableAlignCenter:/^ *:-+: *$/,tableAlignLeft:/^ *:-+ *$/,startATag:/^<a /i,endATag:/^<\/a>/i,startPreScriptTag:/^<(pre|code|kbd|script)(\s|>)/i,endPreScriptTag:/^<\/(pre|code|kbd|script)(\s|>)/i,startAngleBracket:/^</,endAngleBracket:/>$/,pedanticHrefTitle:/^([^'"]*[^\s])\s+(['"])(.*)\2/,unicodeAlphaNumeric:/[\p{L}\p{N}]/u,escapeTest:/[&<>"']/,escapeReplace:/[&<>"']/g,escapeTestNoEncode:/[<>"']|&(?!(#\d{1,7}|#[Xx][a-fA-F0-9]{1,6}|\w+);)/,escapeReplaceNoEncode:/[<>"']|&(?!(#\d{1,7}|#[Xx][a-fA-F0-9]{1,6}|\w+);)/g,unescapeTest:/&(#(?:\d+)|(?:#x[0-9A-Fa-f]+)|(?:\w+));?/ig,caret:/(^|[^\[])\^/g,percentDecode:/%25/g,findPipe:/\|/g,splitPipe:/ \|/,slashPipe:/\\\|/g,carriageReturn:/\r\n|\r/g,spaceLine:/^ +$/gm,notSpaceStart:/^\S*/,endingNewline:/\n$/,listItemRegex:e=>new RegExp(`^( {0,3}${e})((?:[	 ][^\\n]*)?(?:\\n|$))`),nextBulletRegex:e=>new RegExp(`^ {0,${Math.min(3,e-1)}}(?:[*+-]|\\d{1,9}[.)])((?:[ 	][^\\n]*)?(?:\\n|$))`),hrRegex:e=>new RegExp(`^ {0,${Math.min(3,e-1)}}((?:- *){3,}|(?:_ *){3,}|(?:\\* *){3,})(?:\\n+|$)`),fencesBeginRegex:e=>new RegExp(`^ {0,${Math.min(3,e-1)}}(?:\`\`\`|~~~)`),headingBeginRegex:e=>new RegExp(`^ {0,${Math.min(3,e-1)}}#`),htmlBeginRegex:e=>new RegExp(`^ {0,${Math.min(3,e-1)}}<(?:[a-z].*>|!--)`,"i")},$d=/^(?:[ \t]*(?:\n|$))+/,Cd=/^((?: {4}| {0,3}\t)[^\n]+(?:\n(?:[ \t]*(?:\n|$))*)?)+/,Sd=/^ {0,3}(`{3,}(?=[^`\n]*(?:\n|$))|~{3,})([^\n]*)(?:\n|$)(?:|([\s\S]*?)(?:\n|$))(?: {0,3}\1[~`]* *(?=\n|$)|$)/,Ei=/^ {0,3}((?:-[\t ]*){3,}|(?:_[ \t]*){3,}|(?:\*[ \t]*){3,})(?:\n+|$)/,Ad=/^ {0,3}(#{1,6})(?=\s|$)(.*)(?:\n+|$)/,Nr=/(?:[*+-]|\d{1,9}[.)])/,Jn=/^(?!bull |blockCode|fences|blockquote|heading|html|table)((?:.|\n(?!\s*?\n|bull |blockCode|fences|blockquote|heading|html|table))+?)\n {0,3}(=+|-+) *(?:\n+|$)/,tl=W(Jn).replace(/bull/g,Nr).replace(/blockCode/g,/(?: {4}| {0,3}\t)/).replace(/fences/g,/ {0,3}(?:`{3,}|~{3,})/).replace(/blockquote/g,/ {0,3}>/).replace(/heading/g,/ {0,3}#{1,6}/).replace(/html/g,/ {0,3}<[^\n>]+>\n/).replace(/\|table/g,"").getRegex(),zd=W(Jn).replace(/bull/g,Nr).replace(/blockCode/g,/(?: {4}| {0,3}\t)/).replace(/fences/g,/ {0,3}(?:`{3,}|~{3,})/).replace(/blockquote/g,/ {0,3}>/).replace(/heading/g,/ {0,3}#{1,6}/).replace(/html/g,/ {0,3}<[^\n>]+>\n/).replace(/table/g,/ {0,3}\|?(?:[:\- ]*\|)+[\:\- ]*\n/).getRegex(),Hr=/^([^\n]+(?:\n(?!hr|heading|lheading|blockquote|fences|list|html|table| +\n)[^\n]+)*)/,Ed=/^[^\n]+/,jr=/(?!\s*\])(?:\\.|[^\[\]\\])+/,Td=W(/^ {0,3}\[(label)\]: *(?:\n[ \t]*)?([^<\s][^\s]*|<.*?>)(?:(?: +(?:\n[ \t]*)?| *\n[ \t]*)(title))? *(?:\n+|$)/).replace("label",jr).replace("title",/(?:"(?:\\"?|[^"\\])*"|'[^'\n]*(?:\n[^'\n]+)*\n?'|\([^()]*\))/).getRegex(),Pd=W(/^( {0,3}bull)([ \t][^\n]+?)?(?:\n|$)/).replace(/bull/g,Nr).getRegex(),Ss="address|article|aside|base|basefont|blockquote|body|caption|center|col|colgroup|dd|details|dialog|dir|div|dl|dt|fieldset|figcaption|figure|footer|form|frame|frameset|h[1-6]|head|header|hr|html|iframe|legend|li|link|main|menu|menuitem|meta|nav|noframes|ol|optgroup|option|p|param|search|section|summary|table|tbody|td|tfoot|th|thead|title|tr|track|ul",qr=/<!--(?:-?>|[\s\S]*?(?:-->|$))/,Od=W("^ {0,3}(?:<(script|pre|style|textarea)[\\s>][\\s\\S]*?(?:</\\1>[^\\n]*\\n+|$)|comment[^\\n]*(\\n+|$)|<\\?[\\s\\S]*?(?:\\?>\\n*|$)|<![A-Z][\\s\\S]*?(?:>\\n*|$)|<!\\[CDATA\\[[\\s\\S]*?(?:\\]\\]>\\n*|$)|</?(tag)(?: +|\\n|/?>)[\\s\\S]*?(?:(?:\\n[ 	]*)+\\n|$)|<(?!script|pre|style|textarea)([a-z][\\w-]*)(?:attribute)*? */?>(?=[ \\t]*(?:\\n|$))[\\s\\S]*?(?:(?:\\n[ 	]*)+\\n|$)|</(?!script|pre|style|textarea)[a-z][\\w-]*\\s*>(?=[ \\t]*(?:\\n|$))[\\s\\S]*?(?:(?:\\n[ 	]*)+\\n|$))","i").replace("comment",qr).replace("tag",Ss).replace("attribute",/ +[a-zA-Z:_][\w.:-]*(?: *= *"[^"\n]*"| *= *'[^'\n]*'| *= *[^\s"'=<>`]+)?/).getRegex(),el=W(Hr).replace("hr",Ei).replace("heading"," {0,3}#{1,6}(?:\\s|$)").replace("|lheading","").replace("|table","").replace("blockquote"," {0,3}>").replace("fences"," {0,3}(?:`{3,}(?=[^`\\n]*\\n)|~{3,})[^\\n]*\\n").replace("list"," {0,3}(?:[*+-]|1[.)]) ").replace("html","</?(?:tag)(?: +|\\n|/?>)|<(?:script|pre|style|textarea|!--)").replace("tag",Ss).getRegex(),Ld=W(/^( {0,3}> ?(paragraph|[^\n]*)(?:\n|$))+/).replace("paragraph",el).getRegex(),Wr={blockquote:Ld,code:Cd,def:Td,fences:Sd,heading:Ad,hr:Ei,html:Od,lheading:tl,list:Pd,newline:$d,paragraph:el,table:ai,text:Ed},Wa=W("^ *([^\\n ].*)\\n {0,3}((?:\\| *)?:?-+:? *(?:\\| *:?-+:? *)*(?:\\| *)?)(?:\\n((?:(?! *\\n|hr|heading|blockquote|code|fences|list|html).*(?:\\n|$))*)\\n*|$)").replace("hr",Ei).replace("heading"," {0,3}#{1,6}(?:\\s|$)").replace("blockquote"," {0,3}>").replace("code","(?: {4}| {0,3}	)[^\\n]").replace("fences"," {0,3}(?:`{3,}(?=[^`\\n]*\\n)|~{3,})[^\\n]*\\n").replace("list"," {0,3}(?:[*+-]|1[.)]) ").replace("html","</?(?:tag)(?: +|\\n|/?>)|<(?:script|pre|style|textarea|!--)").replace("tag",Ss).getRegex(),Rd={...Wr,lheading:zd,table:Wa,paragraph:W(Hr).replace("hr",Ei).replace("heading"," {0,3}#{1,6}(?:\\s|$)").replace("|lheading","").replace("table",Wa).replace("blockquote"," {0,3}>").replace("fences"," {0,3}(?:`{3,}(?=[^`\\n]*\\n)|~{3,})[^\\n]*\\n").replace("list"," {0,3}(?:[*+-]|1[.)]) ").replace("html","</?(?:tag)(?: +|\\n|/?>)|<(?:script|pre|style|textarea|!--)").replace("tag",Ss).getRegex()},Dd={...Wr,html:W(`^ *(?:comment *(?:\\n|\\s*$)|<(tag)[\\s\\S]+?</\\1> *(?:\\n{2,}|\\s*$)|<tag(?:"[^"]*"|'[^']*'|\\s[^'"/>\\s]*)*?/?> *(?:\\n{2,}|\\s*$))`).replace("comment",qr).replace(/tag/g,"(?!(?:a|em|strong|small|s|cite|q|dfn|abbr|data|time|code|var|samp|kbd|sub|sup|i|b|u|mark|ruby|rt|rp|bdi|bdo|span|br|wbr|ins|del|img)\\b)\\w+(?!:|[^\\w\\s@]*@)\\b").getRegex(),def:/^ *\[([^\]]+)\]: *<?([^\s>]+)>?(?: +(["(][^\n]+[")]))? *(?:\n+|$)/,heading:/^(#{1,6})(.*)(?:\n+|$)/,fences:ai,lheading:/^(.+?)\n {0,3}(=+|-+) *(?:\n+|$)/,paragraph:W(Hr).replace("hr",Ei).replace("heading",` *#{1,6} *[^
]`).replace("lheading",tl).replace("|table","").replace("blockquote"," {0,3}>").replace("|fences","").replace("|list","").replace("|html","").replace("|tag","").getRegex()},Md=/^\\([!"#$%&'()*+,\-./:;<=>?@\[\]\\^_`{|}~])/,Id=/^(`+)([^`]|[^`][\s\S]*?[^`])\1(?!`)/,ol=/^( {2,}|\\)\n(?!\s*$)/,Bd=/^(`+|[^`])(?:(?= {2,}\n)|[\s\S]*?(?:(?=[\\<!\[`*_]|\b_|$)|[^ ](?= {2,}\n)))/,As=/[\p{P}\p{S}]/u,Kr=/[\s\p{P}\p{S}]/u,il=/[^\s\p{P}\p{S}]/u,Fd=W(/^((?![*_])punctSpace)/,"u").replace(/punctSpace/g,Kr).getRegex(),sl=/(?!~)[\p{P}\p{S}]/u,Vd=/(?!~)[\s\p{P}\p{S}]/u,Ud=/(?:[^\s\p{P}\p{S}]|~)/u,Nd=/\[[^[\]]*?\]\((?:\\.|[^\\\(\)]|\((?:\\.|[^\\\(\)])*\))*\)|`[^`]*?`|<[^<>]*?>/g,rl=/^(?:\*+(?:((?!\*)punct)|[^\s*]))|^_+(?:((?!_)punct)|([^\s_]))/,Hd=W(rl,"u").replace(/punct/g,As).getRegex(),jd=W(rl,"u").replace(/punct/g,sl).getRegex(),al="^[^_*]*?__[^_*]*?\\*[^_*]*?(?=__)|[^*]+(?=[^*])|(?!\\*)punct(\\*+)(?=[\\s]|$)|notPunctSpace(\\*+)(?!\\*)(?=punctSpace|$)|(?!\\*)punctSpace(\\*+)(?=notPunctSpace)|[\\s](\\*+)(?!\\*)(?=punct)|(?!\\*)punct(\\*+)(?!\\*)(?=punct)|notPunctSpace(\\*+)(?=notPunctSpace)",qd=W(al,"gu").replace(/notPunctSpace/g,il).replace(/punctSpace/g,Kr).replace(/punct/g,As).getRegex(),Wd=W(al,"gu").replace(/notPunctSpace/g,Ud).replace(/punctSpace/g,Vd).replace(/punct/g,sl).getRegex(),Kd=W("^[^_*]*?\\*\\*[^_*]*?_[^_*]*?(?=\\*\\*)|[^_]+(?=[^_])|(?!_)punct(_+)(?=[\\s]|$)|notPunctSpace(_+)(?!_)(?=punctSpace|$)|(?!_)punctSpace(_+)(?=notPunctSpace)|[\\s](_+)(?!_)(?=punct)|(?!_)punct(_+)(?!_)(?=punct)","gu").replace(/notPunctSpace/g,il).replace(/punctSpace/g,Kr).replace(/punct/g,As).getRegex(),Yd=W(/\\(punct)/,"gu").replace(/punct/g,As).getRegex(),Xd=W(/^<(scheme:[^\s\x00-\x1f<>]*|email)>/).replace("scheme",/[a-zA-Z][a-zA-Z0-9+.-]{1,31}/).replace("email",/[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+(@)[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)+(?![-_])/).getRegex(),Qd=W(qr).replace("(?:-->|$)","-->").getRegex(),Zd=W("^comment|^</[a-zA-Z][\\w:-]*\\s*>|^<[a-zA-Z][\\w-]*(?:attribute)*?\\s*/?>|^<\\?[\\s\\S]*?\\?>|^<![a-zA-Z]+\\s[\\s\\S]*?>|^<!\\[CDATA\\[[\\s\\S]*?\\]\\]>").replace("comment",Qd).replace("attribute",/\s+[a-zA-Z:_][\w.:-]*(?:\s*=\s*"[^"]*"|\s*=\s*'[^']*'|\s*=\s*[^\s"'=<>`]+)?/).getRegex(),ns=/(?:\[(?:\\.|[^\[\]\\])*\]|\\.|`[^`]*`|[^\[\]\\`])*?/,Gd=W(/^!?\[(label)\]\(\s*(href)(?:(?:[ \t]*(?:\n[ \t]*)?)(title))?\s*\)/).replace("label",ns).replace("href",/<(?:\\.|[^\n<>\\])+>|[^ \t\n\x00-\x1f]*/).replace("title",/"(?:\\"?|[^"\\])*"|'(?:\\'?|[^'\\])*'|\((?:\\\)?|[^)\\])*\)/).getRegex(),nl=W(/^!?\[(label)\]\[(ref)\]/).replace("label",ns).replace("ref",jr).getRegex(),ll=W(/^!?\[(ref)\](?:\[\])?/).replace("ref",jr).getRegex(),Jd=W("reflink|nolink(?!\\()","g").replace("reflink",nl).replace("nolink",ll).getRegex(),Yr={_backpedal:ai,anyPunctuation:Yd,autolink:Xd,blockSkip:Nd,br:ol,code:Id,del:ai,emStrongLDelim:Hd,emStrongRDelimAst:qd,emStrongRDelimUnd:Kd,escape:Md,link:Gd,nolink:ll,punctuation:Fd,reflink:nl,reflinkSearch:Jd,tag:Zd,text:Bd,url:ai},th={...Yr,link:W(/^!?\[(label)\]\((.*?)\)/).replace("label",ns).getRegex(),reflink:W(/^!?\[(label)\]\s*\[([^\]]*)\]/).replace("label",ns).getRegex()},cr={...Yr,emStrongRDelimAst:Wd,emStrongLDelim:jd,url:W(/^((?:ftp|https?):\/\/|www\.)(?:[a-zA-Z0-9\-]+\.?)+[^\s<]*|^email/,"i").replace("email",/[A-Za-z0-9._+-]+(@)[a-zA-Z0-9-_]+(?:\.[a-zA-Z0-9-_]*[a-zA-Z0-9])+(?![-_])/).getRegex(),_backpedal:/(?:[^?!.,:;*_'"~()&]+|\([^)]*\)|&(?![a-zA-Z0-9]+;$)|[?!.,:;*_'"~)]+(?!$))+/,del:/^(~~?)(?=[^\s~])((?:\\.|[^\\])*?(?:\\.|[^\s~\\]))\1(?=[^~]|$)/,text:/^([`~]+|[^`~])(?:(?= {2,}\n)|(?=[a-zA-Z0-9.!#$%&'*+\/=?_`{\|}~-]+@)|[\s\S]*?(?:(?=[\\<!\[`*~_]|\b_|https?:\/\/|ftp:\/\/|www\.|$)|[^ ](?= {2,}\n)|[^a-zA-Z0-9.!#$%&'*+\/=?_`{\|}~-](?=[a-zA-Z0-9.!#$%&'*+\/=?_`{\|}~-]+@)))/},eh={...cr,br:W(ol).replace("{2,}","*").getRegex(),text:W(cr.text).replace("\\b_","\\b_| {2,}\\n").replace(/\{2,\}/g,"*").getRegex()},Ni={normal:Wr,gfm:Rd,pedantic:Dd},Ko={normal:Yr,gfm:cr,breaks:eh,pedantic:th},oh={"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#39;"},Ka=e=>oh[e];function le(e,t){if(t){if(Tt.escapeTest.test(e))return e.replace(Tt.escapeReplace,Ka)}else if(Tt.escapeTestNoEncode.test(e))return e.replace(Tt.escapeReplaceNoEncode,Ka);return e}function Ya(e){try{e=encodeURI(e).replace(Tt.percentDecode,"%")}catch{return null}return e}function Xa(e,t){var r;const o=e.replace(Tt.findPipe,(a,n,c)=>{let d=!1,u=n;for(;--u>=0&&c[u]==="\\";)d=!d;return d?"|":" |"}),i=o.split(Tt.splitPipe);let s=0;if(i[0].trim()||i.shift(),i.length>0&&!((r=i.at(-1))!=null&&r.trim())&&i.pop(),t)if(i.length>t)i.splice(t);else for(;i.length<t;)i.push("");for(;s<i.length;s++)i[s]=i[s].trim().replace(Tt.slashPipe,"|");return i}function Yo(e,t,o){const i=e.length;if(i===0)return"";let s=0;for(;s<i&&e.charAt(i-s-1)===t;)s++;return e.slice(0,i-s)}function ih(e,t){if(e.indexOf(t[1])===-1)return-1;let o=0;for(let i=0;i<e.length;i++)if(e[i]==="\\")i++;else if(e[i]===t[0])o++;else if(e[i]===t[1]&&(o--,o<0))return i;return o>0?-2:-1}function Qa(e,t,o,i,s){const r=t.href,a=t.title||null,n=e[1].replace(s.other.outputLinkReplace,"$1");i.state.inLink=!0;const c={type:e[0].charAt(0)==="!"?"image":"link",raw:o,href:r,title:a,text:n,tokens:i.inlineTokens(n)};return i.state.inLink=!1,c}function sh(e,t,o){const i=e.match(o.other.indentCodeCompensation);if(i===null)return t;const s=i[1];return t.split(`
`).map(r=>{const a=r.match(o.other.beginningSpace);if(a===null)return r;const[n]=a;return n.length>=s.length?r.slice(s.length):r}).join(`
`)}class ls{constructor(t){G(this,"options");G(this,"rules");G(this,"lexer");this.options=t||lo}space(t){const o=this.rules.block.newline.exec(t);if(o&&o[0].length>0)return{type:"space",raw:o[0]}}code(t){const o=this.rules.block.code.exec(t);if(o){const i=o[0].replace(this.rules.other.codeRemoveIndent,"");return{type:"code",raw:o[0],codeBlockStyle:"indented",text:this.options.pedantic?i:Yo(i,`
`)}}}fences(t){const o=this.rules.block.fences.exec(t);if(o){const i=o[0],s=sh(i,o[3]||"",this.rules);return{type:"code",raw:i,lang:o[2]?o[2].trim().replace(this.rules.inline.anyPunctuation,"$1"):o[2],text:s}}}heading(t){const o=this.rules.block.heading.exec(t);if(o){let i=o[2].trim();if(this.rules.other.endingHash.test(i)){const s=Yo(i,"#");(this.options.pedantic||!s||this.rules.other.endingSpaceChar.test(s))&&(i=s.trim())}return{type:"heading",raw:o[0],depth:o[1].length,text:i,tokens:this.lexer.inline(i)}}}hr(t){const o=this.rules.block.hr.exec(t);if(o)return{type:"hr",raw:Yo(o[0],`
`)}}blockquote(t){const o=this.rules.block.blockquote.exec(t);if(o){let i=Yo(o[0],`
`).split(`
`),s="",r="";const a=[];for(;i.length>0;){let n=!1;const c=[];let d;for(d=0;d<i.length;d++)if(this.rules.other.blockquoteStart.test(i[d]))c.push(i[d]),n=!0;else if(!n)c.push(i[d]);else break;i=i.slice(d);const u=c.join(`
`),h=u.replace(this.rules.other.blockquoteSetextReplace,`
    $1`).replace(this.rules.other.blockquoteSetextReplace2,"");s=s?`${s}
${u}`:u,r=r?`${r}
${h}`:h;const f=this.lexer.state.top;if(this.lexer.state.top=!0,this.lexer.blockTokens(h,a,!0),this.lexer.state.top=f,i.length===0)break;const m=a.at(-1);if((m==null?void 0:m.type)==="code")break;if((m==null?void 0:m.type)==="blockquote"){const g=m,b=g.raw+`
`+i.join(`
`),k=this.blockquote(b);a[a.length-1]=k,s=s.substring(0,s.length-g.raw.length)+k.raw,r=r.substring(0,r.length-g.text.length)+k.text;break}else if((m==null?void 0:m.type)==="list"){const g=m,b=g.raw+`
`+i.join(`
`),k=this.list(b);a[a.length-1]=k,s=s.substring(0,s.length-m.raw.length)+k.raw,r=r.substring(0,r.length-g.raw.length)+k.raw,i=b.substring(a.at(-1).raw.length).split(`
`);continue}}return{type:"blockquote",raw:s,tokens:a,text:r}}}list(t){let o=this.rules.block.list.exec(t);if(o){let i=o[1].trim();const s=i.length>1,r={type:"list",raw:"",ordered:s,start:s?+i.slice(0,-1):"",loose:!1,items:[]};i=s?`\\d{1,9}\\${i.slice(-1)}`:`\\${i}`,this.options.pedantic&&(i=s?i:"[*+-]");const a=this.rules.other.listItemRegex(i);let n=!1;for(;t;){let d=!1,u="",h="";if(!(o=a.exec(t))||this.rules.block.hr.test(t))break;u=o[0],t=t.substring(u.length);let f=o[2].split(`
`,1)[0].replace(this.rules.other.listReplaceTabs,w=>" ".repeat(3*w.length)),m=t.split(`
`,1)[0],g=!f.trim(),b=0;if(this.options.pedantic?(b=2,h=f.trimStart()):g?b=o[1].length+1:(b=o[2].search(this.rules.other.nonSpaceChar),b=b>4?1:b,h=f.slice(b),b+=o[1].length),g&&this.rules.other.blankLine.test(m)&&(u+=m+`
`,t=t.substring(m.length+1),d=!0),!d){const w=this.rules.other.nextBulletRegex(b),_=this.rules.other.hrRegex(b),v=this.rules.other.fencesBeginRegex(b),y=this.rules.other.headingBeginRegex(b),P=this.rules.other.htmlBeginRegex(b);for(;t;){const M=t.split(`
`,1)[0];let I;if(m=M,this.options.pedantic?(m=m.replace(this.rules.other.listReplaceNesting,"  "),I=m):I=m.replace(this.rules.other.tabCharGlobal,"    "),v.test(m)||y.test(m)||P.test(m)||w.test(m)||_.test(m))break;if(I.search(this.rules.other.nonSpaceChar)>=b||!m.trim())h+=`
`+I.slice(b);else{if(g||f.replace(this.rules.other.tabCharGlobal,"    ").search(this.rules.other.nonSpaceChar)>=4||v.test(f)||y.test(f)||_.test(f))break;h+=`
`+m}!g&&!m.trim()&&(g=!0),u+=M+`
`,t=t.substring(M.length+1),f=I.slice(b)}}r.loose||(n?r.loose=!0:this.rules.other.doubleBlankLine.test(u)&&(n=!0));let k=null,$;this.options.gfm&&(k=this.rules.other.listIsTask.exec(h),k&&($=k[0]!=="[ ] ",h=h.replace(this.rules.other.listReplaceTask,""))),r.items.push({type:"list_item",raw:u,task:!!k,checked:$,loose:!1,text:h,tokens:[]}),r.raw+=u}const c=r.items.at(-1);if(c)c.raw=c.raw.trimEnd(),c.text=c.text.trimEnd();else return;r.raw=r.raw.trimEnd();for(let d=0;d<r.items.length;d++)if(this.lexer.state.top=!1,r.items[d].tokens=this.lexer.blockTokens(r.items[d].text,[]),!r.loose){const u=r.items[d].tokens.filter(f=>f.type==="space"),h=u.length>0&&u.some(f=>this.rules.other.anyLine.test(f.raw));r.loose=h}if(r.loose)for(let d=0;d<r.items.length;d++)r.items[d].loose=!0;return r}}html(t){const o=this.rules.block.html.exec(t);if(o)return{type:"html",block:!0,raw:o[0],pre:o[1]==="pre"||o[1]==="script"||o[1]==="style",text:o[0]}}def(t){const o=this.rules.block.def.exec(t);if(o){const i=o[1].toLowerCase().replace(this.rules.other.multipleSpaceGlobal," "),s=o[2]?o[2].replace(this.rules.other.hrefBrackets,"$1").replace(this.rules.inline.anyPunctuation,"$1"):"",r=o[3]?o[3].substring(1,o[3].length-1).replace(this.rules.inline.anyPunctuation,"$1"):o[3];return{type:"def",tag:i,raw:o[0],href:s,title:r}}}table(t){var n;const o=this.rules.block.table.exec(t);if(!o||!this.rules.other.tableDelimiter.test(o[2]))return;const i=Xa(o[1]),s=o[2].replace(this.rules.other.tableAlignChars,"").split("|"),r=(n=o[3])!=null&&n.trim()?o[3].replace(this.rules.other.tableRowBlankLine,"").split(`
`):[],a={type:"table",raw:o[0],header:[],align:[],rows:[]};if(i.length===s.length){for(const c of s)this.rules.other.tableAlignRight.test(c)?a.align.push("right"):this.rules.other.tableAlignCenter.test(c)?a.align.push("center"):this.rules.other.tableAlignLeft.test(c)?a.align.push("left"):a.align.push(null);for(let c=0;c<i.length;c++)a.header.push({text:i[c],tokens:this.lexer.inline(i[c]),header:!0,align:a.align[c]});for(const c of r)a.rows.push(Xa(c,a.header.length).map((d,u)=>({text:d,tokens:this.lexer.inline(d),header:!1,align:a.align[u]})));return a}}lheading(t){const o=this.rules.block.lheading.exec(t);if(o)return{type:"heading",raw:o[0],depth:o[2].charAt(0)==="="?1:2,text:o[1],tokens:this.lexer.inline(o[1])}}paragraph(t){const o=this.rules.block.paragraph.exec(t);if(o){const i=o[1].charAt(o[1].length-1)===`
`?o[1].slice(0,-1):o[1];return{type:"paragraph",raw:o[0],text:i,tokens:this.lexer.inline(i)}}}text(t){const o=this.rules.block.text.exec(t);if(o)return{type:"text",raw:o[0],text:o[0],tokens:this.lexer.inline(o[0])}}escape(t){const o=this.rules.inline.escape.exec(t);if(o)return{type:"escape",raw:o[0],text:o[1]}}tag(t){const o=this.rules.inline.tag.exec(t);if(o)return!this.lexer.state.inLink&&this.rules.other.startATag.test(o[0])?this.lexer.state.inLink=!0:this.lexer.state.inLink&&this.rules.other.endATag.test(o[0])&&(this.lexer.state.inLink=!1),!this.lexer.state.inRawBlock&&this.rules.other.startPreScriptTag.test(o[0])?this.lexer.state.inRawBlock=!0:this.lexer.state.inRawBlock&&this.rules.other.endPreScriptTag.test(o[0])&&(this.lexer.state.inRawBlock=!1),{type:"html",raw:o[0],inLink:this.lexer.state.inLink,inRawBlock:this.lexer.state.inRawBlock,block:!1,text:o[0]}}link(t){const o=this.rules.inline.link.exec(t);if(o){const i=o[2].trim();if(!this.options.pedantic&&this.rules.other.startAngleBracket.test(i)){if(!this.rules.other.endAngleBracket.test(i))return;const a=Yo(i.slice(0,-1),"\\");if((i.length-a.length)%2===0)return}else{const a=ih(o[2],"()");if(a===-2)return;if(a>-1){const c=(o[0].indexOf("!")===0?5:4)+o[1].length+a;o[2]=o[2].substring(0,a),o[0]=o[0].substring(0,c).trim(),o[3]=""}}let s=o[2],r="";if(this.options.pedantic){const a=this.rules.other.pedanticHrefTitle.exec(s);a&&(s=a[1],r=a[3])}else r=o[3]?o[3].slice(1,-1):"";return s=s.trim(),this.rules.other.startAngleBracket.test(s)&&(this.options.pedantic&&!this.rules.other.endAngleBracket.test(i)?s=s.slice(1):s=s.slice(1,-1)),Qa(o,{href:s&&s.replace(this.rules.inline.anyPunctuation,"$1"),title:r&&r.replace(this.rules.inline.anyPunctuation,"$1")},o[0],this.lexer,this.rules)}}reflink(t,o){let i;if((i=this.rules.inline.reflink.exec(t))||(i=this.rules.inline.nolink.exec(t))){const s=(i[2]||i[1]).replace(this.rules.other.multipleSpaceGlobal," "),r=o[s.toLowerCase()];if(!r){const a=i[0].charAt(0);return{type:"text",raw:a,text:a}}return Qa(i,r,i[0],this.lexer,this.rules)}}emStrong(t,o,i=""){let s=this.rules.inline.emStrongLDelim.exec(t);if(!s||s[3]&&i.match(this.rules.other.unicodeAlphaNumeric))return;if(!(s[1]||s[2]||"")||!i||this.rules.inline.punctuation.exec(i)){const a=[...s[0]].length-1;let n,c,d=a,u=0;const h=s[0][0]==="*"?this.rules.inline.emStrongRDelimAst:this.rules.inline.emStrongRDelimUnd;for(h.lastIndex=0,o=o.slice(-1*t.length+a);(s=h.exec(o))!=null;){if(n=s[1]||s[2]||s[3]||s[4]||s[5]||s[6],!n)continue;if(c=[...n].length,s[3]||s[4]){d+=c;continue}else if((s[5]||s[6])&&a%3&&!((a+c)%3)){u+=c;continue}if(d-=c,d>0)continue;c=Math.min(c,c+d+u);const f=[...s[0]][0].length,m=t.slice(0,a+s.index+f+c);if(Math.min(a,c)%2){const b=m.slice(1,-1);return{type:"em",raw:m,text:b,tokens:this.lexer.inlineTokens(b)}}const g=m.slice(2,-2);return{type:"strong",raw:m,text:g,tokens:this.lexer.inlineTokens(g)}}}}codespan(t){const o=this.rules.inline.code.exec(t);if(o){let i=o[2].replace(this.rules.other.newLineCharGlobal," ");const s=this.rules.other.nonSpaceChar.test(i),r=this.rules.other.startingSpaceChar.test(i)&&this.rules.other.endingSpaceChar.test(i);return s&&r&&(i=i.substring(1,i.length-1)),{type:"codespan",raw:o[0],text:i}}}br(t){const o=this.rules.inline.br.exec(t);if(o)return{type:"br",raw:o[0]}}del(t){const o=this.rules.inline.del.exec(t);if(o)return{type:"del",raw:o[0],text:o[2],tokens:this.lexer.inlineTokens(o[2])}}autolink(t){const o=this.rules.inline.autolink.exec(t);if(o){let i,s;return o[2]==="@"?(i=o[1],s="mailto:"+i):(i=o[1],s=i),{type:"link",raw:o[0],text:i,href:s,tokens:[{type:"text",raw:i,text:i}]}}}url(t){var i;let o;if(o=this.rules.inline.url.exec(t)){let s,r;if(o[2]==="@")s=o[0],r="mailto:"+s;else{let a;do a=o[0],o[0]=((i=this.rules.inline._backpedal.exec(o[0]))==null?void 0:i[0])??"";while(a!==o[0]);s=o[0],o[1]==="www."?r="http://"+o[0]:r=o[0]}return{type:"link",raw:o[0],text:s,href:r,tokens:[{type:"text",raw:s,text:s}]}}}inlineText(t){const o=this.rules.inline.text.exec(t);if(o){const i=this.lexer.state.inRawBlock;return{type:"text",raw:o[0],text:o[0],escaped:i}}}}class Kt{constructor(t){G(this,"tokens");G(this,"options");G(this,"state");G(this,"tokenizer");G(this,"inlineQueue");this.tokens=[],this.tokens.links=Object.create(null),this.options=t||lo,this.options.tokenizer=this.options.tokenizer||new ls,this.tokenizer=this.options.tokenizer,this.tokenizer.options=this.options,this.tokenizer.lexer=this,this.inlineQueue=[],this.state={inLink:!1,inRawBlock:!1,top:!0};const o={other:Tt,block:Ni.normal,inline:Ko.normal};this.options.pedantic?(o.block=Ni.pedantic,o.inline=Ko.pedantic):this.options.gfm&&(o.block=Ni.gfm,this.options.breaks?o.inline=Ko.breaks:o.inline=Ko.gfm),this.tokenizer.rules=o}static get rules(){return{block:Ni,inline:Ko}}static lex(t,o){return new Kt(o).lex(t)}static lexInline(t,o){return new Kt(o).inlineTokens(t)}lex(t){t=t.replace(Tt.carriageReturn,`
`),this.blockTokens(t,this.tokens);for(let o=0;o<this.inlineQueue.length;o++){const i=this.inlineQueue[o];this.inlineTokens(i.src,i.tokens)}return this.inlineQueue=[],this.tokens}blockTokens(t,o=[],i=!1){var s,r,a;for(this.options.pedantic&&(t=t.replace(Tt.tabCharGlobal,"    ").replace(Tt.spaceLine,""));t;){let n;if((r=(s=this.options.extensions)==null?void 0:s.block)!=null&&r.some(d=>(n=d.call({lexer:this},t,o))?(t=t.substring(n.raw.length),o.push(n),!0):!1))continue;if(n=this.tokenizer.space(t)){t=t.substring(n.raw.length);const d=o.at(-1);n.raw.length===1&&d!==void 0?d.raw+=`
`:o.push(n);continue}if(n=this.tokenizer.code(t)){t=t.substring(n.raw.length);const d=o.at(-1);(d==null?void 0:d.type)==="paragraph"||(d==null?void 0:d.type)==="text"?(d.raw+=`
`+n.raw,d.text+=`
`+n.text,this.inlineQueue.at(-1).src=d.text):o.push(n);continue}if(n=this.tokenizer.fences(t)){t=t.substring(n.raw.length),o.push(n);continue}if(n=this.tokenizer.heading(t)){t=t.substring(n.raw.length),o.push(n);continue}if(n=this.tokenizer.hr(t)){t=t.substring(n.raw.length),o.push(n);continue}if(n=this.tokenizer.blockquote(t)){t=t.substring(n.raw.length),o.push(n);continue}if(n=this.tokenizer.list(t)){t=t.substring(n.raw.length),o.push(n);continue}if(n=this.tokenizer.html(t)){t=t.substring(n.raw.length),o.push(n);continue}if(n=this.tokenizer.def(t)){t=t.substring(n.raw.length);const d=o.at(-1);(d==null?void 0:d.type)==="paragraph"||(d==null?void 0:d.type)==="text"?(d.raw+=`
`+n.raw,d.text+=`
`+n.raw,this.inlineQueue.at(-1).src=d.text):this.tokens.links[n.tag]||(this.tokens.links[n.tag]={href:n.href,title:n.title});continue}if(n=this.tokenizer.table(t)){t=t.substring(n.raw.length),o.push(n);continue}if(n=this.tokenizer.lheading(t)){t=t.substring(n.raw.length),o.push(n);continue}let c=t;if((a=this.options.extensions)!=null&&a.startBlock){let d=1/0;const u=t.slice(1);let h;this.options.extensions.startBlock.forEach(f=>{h=f.call({lexer:this},u),typeof h=="number"&&h>=0&&(d=Math.min(d,h))}),d<1/0&&d>=0&&(c=t.substring(0,d+1))}if(this.state.top&&(n=this.tokenizer.paragraph(c))){const d=o.at(-1);i&&(d==null?void 0:d.type)==="paragraph"?(d.raw+=`
`+n.raw,d.text+=`
`+n.text,this.inlineQueue.pop(),this.inlineQueue.at(-1).src=d.text):o.push(n),i=c.length!==t.length,t=t.substring(n.raw.length);continue}if(n=this.tokenizer.text(t)){t=t.substring(n.raw.length);const d=o.at(-1);(d==null?void 0:d.type)==="text"?(d.raw+=`
`+n.raw,d.text+=`
`+n.text,this.inlineQueue.pop(),this.inlineQueue.at(-1).src=d.text):o.push(n);continue}if(t){const d="Infinite loop on byte: "+t.charCodeAt(0);if(this.options.silent){console.error(d);break}else throw new Error(d)}}return this.state.top=!0,o}inline(t,o=[]){return this.inlineQueue.push({src:t,tokens:o}),o}inlineTokens(t,o=[]){var n,c,d;let i=t,s=null;if(this.tokens.links){const u=Object.keys(this.tokens.links);if(u.length>0)for(;(s=this.tokenizer.rules.inline.reflinkSearch.exec(i))!=null;)u.includes(s[0].slice(s[0].lastIndexOf("[")+1,-1))&&(i=i.slice(0,s.index)+"["+"a".repeat(s[0].length-2)+"]"+i.slice(this.tokenizer.rules.inline.reflinkSearch.lastIndex))}for(;(s=this.tokenizer.rules.inline.anyPunctuation.exec(i))!=null;)i=i.slice(0,s.index)+"++"+i.slice(this.tokenizer.rules.inline.anyPunctuation.lastIndex);for(;(s=this.tokenizer.rules.inline.blockSkip.exec(i))!=null;)i=i.slice(0,s.index)+"["+"a".repeat(s[0].length-2)+"]"+i.slice(this.tokenizer.rules.inline.blockSkip.lastIndex);let r=!1,a="";for(;t;){r||(a=""),r=!1;let u;if((c=(n=this.options.extensions)==null?void 0:n.inline)!=null&&c.some(f=>(u=f.call({lexer:this},t,o))?(t=t.substring(u.raw.length),o.push(u),!0):!1))continue;if(u=this.tokenizer.escape(t)){t=t.substring(u.raw.length),o.push(u);continue}if(u=this.tokenizer.tag(t)){t=t.substring(u.raw.length),o.push(u);continue}if(u=this.tokenizer.link(t)){t=t.substring(u.raw.length),o.push(u);continue}if(u=this.tokenizer.reflink(t,this.tokens.links)){t=t.substring(u.raw.length);const f=o.at(-1);u.type==="text"&&(f==null?void 0:f.type)==="text"?(f.raw+=u.raw,f.text+=u.text):o.push(u);continue}if(u=this.tokenizer.emStrong(t,i,a)){t=t.substring(u.raw.length),o.push(u);continue}if(u=this.tokenizer.codespan(t)){t=t.substring(u.raw.length),o.push(u);continue}if(u=this.tokenizer.br(t)){t=t.substring(u.raw.length),o.push(u);continue}if(u=this.tokenizer.del(t)){t=t.substring(u.raw.length),o.push(u);continue}if(u=this.tokenizer.autolink(t)){t=t.substring(u.raw.length),o.push(u);continue}if(!this.state.inLink&&(u=this.tokenizer.url(t))){t=t.substring(u.raw.length),o.push(u);continue}let h=t;if((d=this.options.extensions)!=null&&d.startInline){let f=1/0;const m=t.slice(1);let g;this.options.extensions.startInline.forEach(b=>{g=b.call({lexer:this},m),typeof g=="number"&&g>=0&&(f=Math.min(f,g))}),f<1/0&&f>=0&&(h=t.substring(0,f+1))}if(u=this.tokenizer.inlineText(h)){t=t.substring(u.raw.length),u.raw.slice(-1)!=="_"&&(a=u.raw.slice(-1)),r=!0;const f=o.at(-1);(f==null?void 0:f.type)==="text"?(f.raw+=u.raw,f.text+=u.text):o.push(u);continue}if(t){const f="Infinite loop on byte: "+t.charCodeAt(0);if(this.options.silent){console.error(f);break}else throw new Error(f)}}return o}}class cs{constructor(t){G(this,"options");G(this,"parser");this.options=t||lo}space(t){return""}code({text:t,lang:o,escaped:i}){var a;const s=(a=(o||"").match(Tt.notSpaceStart))==null?void 0:a[0],r=t.replace(Tt.endingNewline,"")+`
`;return s?'<pre><code class="language-'+le(s)+'">'+(i?r:le(r,!0))+`</code></pre>
`:"<pre><code>"+(i?r:le(r,!0))+`</code></pre>
`}blockquote({tokens:t}){return`<blockquote>
${this.parser.parse(t)}</blockquote>
`}html({text:t}){return t}heading({tokens:t,depth:o}){return`<h${o}>${this.parser.parseInline(t)}</h${o}>
`}hr(t){return`<hr>
`}list(t){const o=t.ordered,i=t.start;let s="";for(let n=0;n<t.items.length;n++){const c=t.items[n];s+=this.listitem(c)}const r=o?"ol":"ul",a=o&&i!==1?' start="'+i+'"':"";return"<"+r+a+`>
`+s+"</"+r+`>
`}listitem(t){var i;let o="";if(t.task){const s=this.checkbox({checked:!!t.checked});t.loose?((i=t.tokens[0])==null?void 0:i.type)==="paragraph"?(t.tokens[0].text=s+" "+t.tokens[0].text,t.tokens[0].tokens&&t.tokens[0].tokens.length>0&&t.tokens[0].tokens[0].type==="text"&&(t.tokens[0].tokens[0].text=s+" "+le(t.tokens[0].tokens[0].text),t.tokens[0].tokens[0].escaped=!0)):t.tokens.unshift({type:"text",raw:s+" ",text:s+" ",escaped:!0}):o+=s+" "}return o+=this.parser.parse(t.tokens,!!t.loose),`<li>${o}</li>
`}checkbox({checked:t}){return"<input "+(t?'checked="" ':"")+'disabled="" type="checkbox">'}paragraph({tokens:t}){return`<p>${this.parser.parseInline(t)}</p>
`}table(t){let o="",i="";for(let r=0;r<t.header.length;r++)i+=this.tablecell(t.header[r]);o+=this.tablerow({text:i});let s="";for(let r=0;r<t.rows.length;r++){const a=t.rows[r];i="";for(let n=0;n<a.length;n++)i+=this.tablecell(a[n]);s+=this.tablerow({text:i})}return s&&(s=`<tbody>${s}</tbody>`),`<table>
<thead>
`+o+`</thead>
`+s+`</table>
`}tablerow({text:t}){return`<tr>
${t}</tr>
`}tablecell(t){const o=this.parser.parseInline(t.tokens),i=t.header?"th":"td";return(t.align?`<${i} align="${t.align}">`:`<${i}>`)+o+`</${i}>
`}strong({tokens:t}){return`<strong>${this.parser.parseInline(t)}</strong>`}em({tokens:t}){return`<em>${this.parser.parseInline(t)}</em>`}codespan({text:t}){return`<code>${le(t,!0)}</code>`}br(t){return"<br>"}del({tokens:t}){return`<del>${this.parser.parseInline(t)}</del>`}link({href:t,title:o,tokens:i}){const s=this.parser.parseInline(i),r=Ya(t);if(r===null)return s;t=r;let a='<a href="'+t+'"';return o&&(a+=' title="'+le(o)+'"'),a+=">"+s+"</a>",a}image({href:t,title:o,text:i,tokens:s}){s&&(i=this.parser.parseInline(s,this.parser.textRenderer));const r=Ya(t);if(r===null)return le(i);t=r;let a=`<img src="${t}" alt="${i}"`;return o&&(a+=` title="${le(o)}"`),a+=">",a}text(t){return"tokens"in t&&t.tokens?this.parser.parseInline(t.tokens):"escaped"in t&&t.escaped?t.text:le(t.text)}}class Xr{strong({text:t}){return t}em({text:t}){return t}codespan({text:t}){return t}del({text:t}){return t}html({text:t}){return t}text({text:t}){return t}link({text:t}){return""+t}image({text:t}){return""+t}br(){return""}}class Yt{constructor(t){G(this,"options");G(this,"renderer");G(this,"textRenderer");this.options=t||lo,this.options.renderer=this.options.renderer||new cs,this.renderer=this.options.renderer,this.renderer.options=this.options,this.renderer.parser=this,this.textRenderer=new Xr}static parse(t,o){return new Yt(o).parse(t)}static parseInline(t,o){return new Yt(o).parseInline(t)}parse(t,o=!0){var s,r;let i="";for(let a=0;a<t.length;a++){const n=t[a];if((r=(s=this.options.extensions)==null?void 0:s.renderers)!=null&&r[n.type]){const d=n,u=this.options.extensions.renderers[d.type].call({parser:this},d);if(u!==!1||!["space","hr","heading","code","table","blockquote","list","html","paragraph","text"].includes(d.type)){i+=u||"";continue}}const c=n;switch(c.type){case"space":{i+=this.renderer.space(c);continue}case"hr":{i+=this.renderer.hr(c);continue}case"heading":{i+=this.renderer.heading(c);continue}case"code":{i+=this.renderer.code(c);continue}case"table":{i+=this.renderer.table(c);continue}case"blockquote":{i+=this.renderer.blockquote(c);continue}case"list":{i+=this.renderer.list(c);continue}case"html":{i+=this.renderer.html(c);continue}case"paragraph":{i+=this.renderer.paragraph(c);continue}case"text":{let d=c,u=this.renderer.text(d);for(;a+1<t.length&&t[a+1].type==="text";)d=t[++a],u+=`
`+this.renderer.text(d);o?i+=this.renderer.paragraph({type:"paragraph",raw:u,text:u,tokens:[{type:"text",raw:u,text:u,escaped:!0}]}):i+=u;continue}default:{const d='Token with "'+c.type+'" type was not found.';if(this.options.silent)return console.error(d),"";throw new Error(d)}}}return i}parseInline(t,o=this.renderer){var s,r;let i="";for(let a=0;a<t.length;a++){const n=t[a];if((r=(s=this.options.extensions)==null?void 0:s.renderers)!=null&&r[n.type]){const d=this.options.extensions.renderers[n.type].call({parser:this},n);if(d!==!1||!["escape","html","link","image","strong","em","codespan","br","del","text"].includes(n.type)){i+=d||"";continue}}const c=n;switch(c.type){case"escape":{i+=o.text(c);break}case"html":{i+=o.html(c);break}case"link":{i+=o.link(c);break}case"image":{i+=o.image(c);break}case"strong":{i+=o.strong(c);break}case"em":{i+=o.em(c);break}case"codespan":{i+=o.codespan(c);break}case"br":{i+=o.br(c);break}case"del":{i+=o.del(c);break}case"text":{i+=o.text(c);break}default:{const d='Token with "'+c.type+'" type was not found.';if(this.options.silent)return console.error(d),"";throw new Error(d)}}}return i}}class ni{constructor(t){G(this,"options");G(this,"block");this.options=t||lo}preprocess(t){return t}postprocess(t){return t}processAllTokens(t){return t}provideLexer(){return this.block?Kt.lex:Kt.lexInline}provideParser(){return this.block?Yt.parse:Yt.parseInline}}G(ni,"passThroughHooks",new Set(["preprocess","postprocess","processAllTokens"]));class rh{constructor(...t){G(this,"defaults",Ur());G(this,"options",this.setOptions);G(this,"parse",this.parseMarkdown(!0));G(this,"parseInline",this.parseMarkdown(!1));G(this,"Parser",Yt);G(this,"Renderer",cs);G(this,"TextRenderer",Xr);G(this,"Lexer",Kt);G(this,"Tokenizer",ls);G(this,"Hooks",ni);this.use(...t)}walkTokens(t,o){var s,r;let i=[];for(const a of t)switch(i=i.concat(o.call(this,a)),a.type){case"table":{const n=a;for(const c of n.header)i=i.concat(this.walkTokens(c.tokens,o));for(const c of n.rows)for(const d of c)i=i.concat(this.walkTokens(d.tokens,o));break}case"list":{const n=a;i=i.concat(this.walkTokens(n.items,o));break}default:{const n=a;(r=(s=this.defaults.extensions)==null?void 0:s.childTokens)!=null&&r[n.type]?this.defaults.extensions.childTokens[n.type].forEach(c=>{const d=n[c].flat(1/0);i=i.concat(this.walkTokens(d,o))}):n.tokens&&(i=i.concat(this.walkTokens(n.tokens,o)))}}return i}use(...t){const o=this.defaults.extensions||{renderers:{},childTokens:{}};return t.forEach(i=>{const s={...i};if(s.async=this.defaults.async||s.async||!1,i.extensions&&(i.extensions.forEach(r=>{if(!r.name)throw new Error("extension name required");if("renderer"in r){const a=o.renderers[r.name];a?o.renderers[r.name]=function(...n){let c=r.renderer.apply(this,n);return c===!1&&(c=a.apply(this,n)),c}:o.renderers[r.name]=r.renderer}if("tokenizer"in r){if(!r.level||r.level!=="block"&&r.level!=="inline")throw new Error("extension level must be 'block' or 'inline'");const a=o[r.level];a?a.unshift(r.tokenizer):o[r.level]=[r.tokenizer],r.start&&(r.level==="block"?o.startBlock?o.startBlock.push(r.start):o.startBlock=[r.start]:r.level==="inline"&&(o.startInline?o.startInline.push(r.start):o.startInline=[r.start]))}"childTokens"in r&&r.childTokens&&(o.childTokens[r.name]=r.childTokens)}),s.extensions=o),i.renderer){const r=this.defaults.renderer||new cs(this.defaults);for(const a in i.renderer){if(!(a in r))throw new Error(`renderer '${a}' does not exist`);if(["options","parser"].includes(a))continue;const n=a,c=i.renderer[n],d=r[n];r[n]=(...u)=>{let h=c.apply(r,u);return h===!1&&(h=d.apply(r,u)),h||""}}s.renderer=r}if(i.tokenizer){const r=this.defaults.tokenizer||new ls(this.defaults);for(const a in i.tokenizer){if(!(a in r))throw new Error(`tokenizer '${a}' does not exist`);if(["options","rules","lexer"].includes(a))continue;const n=a,c=i.tokenizer[n],d=r[n];r[n]=(...u)=>{let h=c.apply(r,u);return h===!1&&(h=d.apply(r,u)),h}}s.tokenizer=r}if(i.hooks){const r=this.defaults.hooks||new ni;for(const a in i.hooks){if(!(a in r))throw new Error(`hook '${a}' does not exist`);if(["options","block"].includes(a))continue;const n=a,c=i.hooks[n],d=r[n];ni.passThroughHooks.has(a)?r[n]=u=>{if(this.defaults.async)return Promise.resolve(c.call(r,u)).then(f=>d.call(r,f));const h=c.call(r,u);return d.call(r,h)}:r[n]=(...u)=>{let h=c.apply(r,u);return h===!1&&(h=d.apply(r,u)),h}}s.hooks=r}if(i.walkTokens){const r=this.defaults.walkTokens,a=i.walkTokens;s.walkTokens=function(n){let c=[];return c.push(a.call(this,n)),r&&(c=c.concat(r.call(this,n))),c}}this.defaults={...this.defaults,...s}}),this}setOptions(t){return this.defaults={...this.defaults,...t},this}lexer(t,o){return Kt.lex(t,o??this.defaults)}parser(t,o){return Yt.parse(t,o??this.defaults)}parseMarkdown(t){return(i,s)=>{const r={...s},a={...this.defaults,...r},n=this.onError(!!a.silent,!!a.async);if(this.defaults.async===!0&&r.async===!1)return n(new Error("marked(): The async option was set to true by an extension. Remove async: false from the parse options object to return a Promise."));if(typeof i>"u"||i===null)return n(new Error("marked(): input parameter is undefined or null"));if(typeof i!="string")return n(new Error("marked(): input parameter is of type "+Object.prototype.toString.call(i)+", string expected"));a.hooks&&(a.hooks.options=a,a.hooks.block=t);const c=a.hooks?a.hooks.provideLexer():t?Kt.lex:Kt.lexInline,d=a.hooks?a.hooks.provideParser():t?Yt.parse:Yt.parseInline;if(a.async)return Promise.resolve(a.hooks?a.hooks.preprocess(i):i).then(u=>c(u,a)).then(u=>a.hooks?a.hooks.processAllTokens(u):u).then(u=>a.walkTokens?Promise.all(this.walkTokens(u,a.walkTokens)).then(()=>u):u).then(u=>d(u,a)).then(u=>a.hooks?a.hooks.postprocess(u):u).catch(n);try{a.hooks&&(i=a.hooks.preprocess(i));let u=c(i,a);a.hooks&&(u=a.hooks.processAllTokens(u)),a.walkTokens&&this.walkTokens(u,a.walkTokens);let h=d(u,a);return a.hooks&&(h=a.hooks.postprocess(h)),h}catch(u){return n(u)}}}onError(t,o){return i=>{if(i.message+=`
Please report this to https://github.com/markedjs/marked.`,t){const s="<p>An error occurred:</p><pre>"+le(i.message+"",!0)+"</pre>";return o?Promise.resolve(s):s}if(o)return Promise.reject(i);throw i}}}const oo=new rh;function Q(e,t){return oo.parse(e,t)}Q.options=Q.setOptions=function(e){return oo.setOptions(e),Q.defaults=oo.defaults,Gn(Q.defaults),Q};Q.getDefaults=Ur;Q.defaults=lo;Q.use=function(...e){return oo.use(...e),Q.defaults=oo.defaults,Gn(Q.defaults),Q};Q.walkTokens=function(e,t){return oo.walkTokens(e,t)};Q.parseInline=oo.parseInline;Q.Parser=Yt;Q.parser=Yt.parse;Q.Renderer=cs;Q.TextRenderer=Xr;Q.Lexer=Kt;Q.lexer=Kt.lex;Q.Tokenizer=ls;Q.Hooks=ni;Q.parse=Q;Q.options;Q.setOptions;Q.use;Q.walkTokens;Q.parseInline;Yt.parse;Kt.lex;var ah=Object.defineProperty,nh=Object.getOwnPropertyDescriptor,cl=(e,t,o,i)=>{for(var s=i>1?void 0:i?nh(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&ah(t,o,s),s};let ds=class extends nt{constructor(){super(...arguments),this.parse=async e=>(e=e.replace(/^[\u200B\u200C\u200D\u200E\u200F\uFEFF]/,""),e=await Q.parse(e,{async:!0,gfm:!0}),B`
      <div class="markdown-body">
        ${Qn(e)}
      </div>
    `)}render(){return this.value?_d(this.parse(this.value),U):U}};ds.styles=bt`
    :host {
      display: block;
      width: 100%;
      height: auto;
    }

    * {
      overflow-wrap: anywhere;
    }

    /* github-markdown styles */
    .markdown-body {
      -ms-text-size-adjust: 100%;
      -webkit-text-size-adjust: 100%;
      margin: 0;
      color: var(--fgColor-default);
      background-color: transparent;
      font-family: -apple-system,BlinkMacSystemFont,"Segoe UI","Noto Sans",Helvetica,Arial,sans-serif,"Apple Color Emoji","Segoe UI Emoji";
      font-size: 16px;
      line-height: 1.5;
      word-wrap: break-word;

      --base-size-4: 0.25rem;
      --base-size-8: 0.5rem;
      --base-size-16: 1rem;
      --base-size-24: 1.5rem;
      --base-size-40: 2.5rem;
      --base-text-weight-normal: 400;
      --base-text-weight-medium: 500;
      --base-text-weight-semibold: 600;
      --fontStack-monospace: ui-monospace, SFMono-Regular, SF Mono, Menlo, Consolas, Liberation Mono, monospace;
      --fgColor-accent: Highlight;
    }

    .markdown-body .octicon {
      display: inline-block;
      fill: currentColor;
      vertical-align: text-bottom;
    }

    .markdown-body h1:hover .anchor .octicon-link:before,
    .markdown-body h2:hover .anchor .octicon-link:before,
    .markdown-body h3:hover .anchor .octicon-link:before,
    .markdown-body h4:hover .anchor .octicon-link:before,
    .markdown-body h5:hover .anchor .octicon-link:before,
    .markdown-body h6:hover .anchor .octicon-link:before {
      width: 16px;
      height: 16px;
      content: ' ';
      display: inline-block;
      background-color: currentColor;
      -webkit-mask-image: url("data:image/svg+xml,<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16' version='1.1' aria-hidden='true'><path fill-rule='evenodd' d='M7.775 3.275a.75.75 0 001.06 1.06l1.25-1.25a2 2 0 112.83 2.83l-2.5 2.5a2 2 0 01-2.83 0 .75.75 0 00-1.06 1.06 3.5 3.5 0 004.95 0l2.5-2.5a3.5 3.5 0 00-4.95-4.95l-1.25 1.25zm-4.69 9.64a2 2 0 010-2.83l2.5-2.5a2 2 0 012.83 0 .75.75 0 001.06-1.06 3.5 3.5 0 00-4.95 0l-2.5 2.5a3.5 3.5 0 004.95 4.95l1.25-1.25a.75.75 0 00-1.06-1.06l-1.25 1.25a2 2 0 01-2.83 0z'></path></svg>");
      mask-image: url("data:image/svg+xml,<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16' version='1.1' aria-hidden='true'><path fill-rule='evenodd' d='M7.775 3.275a.75.75 0 001.06 1.06l1.25-1.25a2 2 0 112.83 2.83l-2.5 2.5a2 2 0 01-2.83 0 .75.75 0 00-1.06 1.06 3.5 3.5 0 004.95 0l2.5-2.5a3.5 3.5 0 00-4.95-4.95l-1.25 1.25zm-4.69 9.64a2 2 0 010-2.83l2.5-2.5a2 2 0 012.83 0 .75.75 0 001.06-1.06 3.5 3.5 0 00-4.95 0l-2.5 2.5a3.5 3.5 0 004.95 4.95l1.25-1.25a.75.75 0 00-1.06-1.06l-1.25 1.25a2 2 0 01-2.83 0z'></path></svg>");
    }

    .markdown-body details,
    .markdown-body figcaption,
    .markdown-body figure {
      display: block;
    }

    .markdown-body summary {
      display: list-item;
    }

    .markdown-body [hidden] {
      display: none !important;
    }

    .markdown-body a {
      background-color: transparent;
      color: var(--fgColor-accent);
      text-decoration: none;
    }

    .markdown-body abbr[title] {
      border-bottom: none;
      -webkit-text-decoration: underline dotted;
      text-decoration: underline dotted;
    }

    .markdown-body b,
    .markdown-body strong {
      font-weight: var(--base-text-weight-semibold, 600);
    }

    .markdown-body dfn {
      font-style: italic;
    }

    .markdown-body h1 {
      margin: .67em 0;
      font-weight: var(--base-text-weight-semibold, 600);
      padding-bottom: .3em;
      font-size: 2em;
      border-bottom: 1px solid var(--borderColor-muted);
    }

    .markdown-body mark {
      background-color: var(--bgColor-attention-muted);
      color: var(--fgColor-default);
    }

    .markdown-body small {
      font-size: 90%;
    }

    .markdown-body sub,
    .markdown-body sup {
      font-size: 75%;
      line-height: 0;
      position: relative;
      vertical-align: baseline;
    }

    .markdown-body sub {
      bottom: -0.25em;
    }

    .markdown-body sup {
      top: -0.5em;
    }

    .markdown-body img {
      border-style: none;
      max-width: 100%;
      box-sizing: content-box;
    }

    .markdown-body code,
    .markdown-body kbd,
    .markdown-body pre,
    .markdown-body samp {
      font-family: monospace;
      font-size: 1em;
    }

    .markdown-body figure {
      margin: 1em var(--base-size-40);
    }

    .markdown-body hr {
      box-sizing: content-box;
      overflow: hidden;
      background: transparent;
      border-bottom: 1px solid var(--borderColor-muted);
      height: .25em;
      padding: 0;
      margin: var(--base-size-24) 0;
      background-color: transparent;
      border: 0;
    }

    .markdown-body input {
      font: inherit;
      margin: 0;
      overflow: visible;
      font-family: inherit;
      font-size: inherit;
      line-height: inherit;
    }

    .markdown-body [type=button],
    .markdown-body [type=reset],
    .markdown-body [type=submit] {
      -webkit-appearance: button;
      appearance: button;
    }

    .markdown-body [type=checkbox],
    .markdown-body [type=radio] {
      box-sizing: border-box;
      padding: 0;
    }

    .markdown-body [type=number]::-webkit-inner-spin-button,
    .markdown-body [type=number]::-webkit-outer-spin-button {
      height: auto;
    }

    .markdown-body [type=search]::-webkit-search-cancel-button,
    .markdown-body [type=search]::-webkit-search-decoration {
      -webkit-appearance: none;
      appearance: none;
    }

    .markdown-body ::-webkit-input-placeholder {
      color: inherit;
      opacity: .54;
    }

    .markdown-body ::-webkit-file-upload-button {
      -webkit-appearance: button;
      appearance: button;
      font: inherit;
    }

    .markdown-body a:hover {
      text-decoration: underline;
    }

    .markdown-body ::placeholder {
      color: var(--fgColor-muted);
      opacity: 1;
    }

    .markdown-body hr::before {
      display: table;
      content: "";
    }

    .markdown-body hr::after {
      display: table;
      clear: both;
      content: "";
    }

    .markdown-body table {
      border-spacing: 0;
      border-collapse: collapse;
      display: block;
      width: max-content;
      max-width: 100%;
      overflow: auto;
      font-variant: tabular-nums;
    }

    .markdown-body td,
    .markdown-body th {
      padding: 0;
    }

    .markdown-body details summary {
      cursor: pointer;
    }

    .markdown-body a:focus,
    .markdown-body [role=button]:focus,
    .markdown-body input[type=radio]:focus,
    .markdown-body input[type=checkbox]:focus {
      outline: 2px solid var(--focus-outlineColor);
      outline-offset: -2px;
      box-shadow: none;
    }

    .markdown-body a:focus:not(:focus-visible),
    .markdown-body [role=button]:focus:not(:focus-visible),
    .markdown-body input[type=radio]:focus:not(:focus-visible),
    .markdown-body input[type=checkbox]:focus:not(:focus-visible) {
      outline: solid 1px transparent;
    }

    .markdown-body a:focus-visible,
    .markdown-body [role=button]:focus-visible,
    .markdown-body input[type=radio]:focus-visible,
    .markdown-body input[type=checkbox]:focus-visible {
      outline: 2px solid var(--focus-outlineColor);
      outline-offset: -2px;
      box-shadow: none;
    }

    .markdown-body a:not([class]):focus,
    .markdown-body a:not([class]):focus-visible,
    .markdown-body input[type=radio]:focus,
    .markdown-body input[type=radio]:focus-visible,
    .markdown-body input[type=checkbox]:focus,
    .markdown-body input[type=checkbox]:focus-visible {
      outline-offset: 0;
    }

    .markdown-body kbd {
      display: inline-block;
      padding: var(--base-size-4);
      font: 11px var(--fontStack-monospace, ui-monospace, SFMono-Regular, SF Mono, Menlo, Consolas, Liberation Mono, monospace);
      line-height: 10px;
      color: var(--fgColor-default);
      vertical-align: middle;
      background-color: var(--bgColor-muted);
      border: solid 1px var(--borderColor-neutral-muted);
      border-bottom-color: var(--borderColor-neutral-muted);
      border-radius: 6px;
      box-shadow: inset 0 -1px 0 var(--borderColor-neutral-muted);
    }

    .markdown-body h1,
    .markdown-body h2,
    .markdown-body h3,
    .markdown-body h4,
    .markdown-body h5,
    .markdown-body h6 {
      margin-top: var(--base-size-24);
      margin-bottom: var(--base-size-16);
      font-weight: var(--base-text-weight-semibold, 600);
      line-height: 1.25;
    }

    .markdown-body h2 {
      font-weight: var(--base-text-weight-semibold, 600);
      padding-bottom: .3em;
      font-size: 1.5em;
      border-bottom: 1px solid var(--borderColor-muted);
    }

    .markdown-body h3 {
      font-weight: var(--base-text-weight-semibold, 600);
      font-size: 1.25em;
    }

    .markdown-body h4 {
      font-weight: var(--base-text-weight-semibold, 600);
      font-size: 1em;
    }

    .markdown-body h5 {
      font-weight: var(--base-text-weight-semibold, 600);
      font-size: .875em;
    }

    .markdown-body h6 {
      font-weight: var(--base-text-weight-semibold, 600);
      font-size: .85em;
      color: var(--fgColor-muted);
    }

    .markdown-body p {
      margin-top: 0;
      margin-bottom: 10px;
    }

    .markdown-body blockquote {
      margin: 0;
      padding: 0 1em;
      color: var(--fgColor-muted);
      border-left: .25em solid var(--borderColor-default);
    }

    .markdown-body ul,
    .markdown-body ol {
      margin-top: 0;
      margin-bottom: 0;
      padding-left: 2em;
    }

    .markdown-body ol ol,
    .markdown-body ul ol {
      list-style-type: lower-roman;
    }

    .markdown-body ul ul ol,
    .markdown-body ul ol ol,
    .markdown-body ol ul ol,
    .markdown-body ol ol ol {
      list-style-type: lower-alpha;
    }

    .markdown-body dd {
      margin-left: 0;
    }

    .markdown-body tt,
    .markdown-body code,
    .markdown-body samp {
      font-family: var(--fontStack-monospace, ui-monospace, SFMono-Regular, SF Mono, Menlo, Consolas, Liberation Mono, monospace);
      font-size: 12px;
    }

    .markdown-body pre {
      margin-top: 0;
      margin-bottom: 0;
      font-family: var(--fontStack-monospace, ui-monospace, SFMono-Regular, SF Mono, Menlo, Consolas, Liberation Mono, monospace);
      font-size: 12px;
      word-wrap: normal;
    }

    .markdown-body .octicon {
      display: inline-block;
      overflow: visible !important;
      vertical-align: text-bottom;
      fill: currentColor;
    }

    .markdown-body input::-webkit-outer-spin-button,
    .markdown-body input::-webkit-inner-spin-button {
      margin: 0;
      appearance: none;
    }

    .markdown-body .mr-2 {
      margin-right: var(--base-size-8, 8px) !important;
    }

    .markdown-body::before {
      display: table;
      content: "";
    }

    .markdown-body::after {
      display: table;
      clear: both;
      content: "";
    }

    .markdown-body>*:first-child {
      margin-top: 0 !important;
    }

    .markdown-body>*:last-child {
      margin-bottom: 0 !important;
    }

    .markdown-body a:not([href]) {
      color: inherit;
      text-decoration: none;
    }

    .markdown-body .absent {
      color: var(--fgColor-danger);
    }

    .markdown-body .anchor {
      float: left;
      padding-right: var(--base-size-4);
      margin-left: -20px;
      line-height: 1;
    }

    .markdown-body .anchor:focus {
      outline: none;
    }

    .markdown-body p,
    .markdown-body blockquote,
    .markdown-body ul,
    .markdown-body ol,
    .markdown-body dl,
    .markdown-body table,
    .markdown-body pre,
    .markdown-body details {
      margin-top: 0;
      margin-bottom: var(--base-size-16);
    }

    .markdown-body blockquote>:first-child {
      margin-top: 0;
    }

    .markdown-body blockquote>:last-child {
      margin-bottom: 0;
    }

    .markdown-body h1 .octicon-link,
    .markdown-body h2 .octicon-link,
    .markdown-body h3 .octicon-link,
    .markdown-body h4 .octicon-link,
    .markdown-body h5 .octicon-link,
    .markdown-body h6 .octicon-link {
      color: var(--fgColor-default);
      vertical-align: middle;
      visibility: hidden;
    }

    .markdown-body h1:hover .anchor,
    .markdown-body h2:hover .anchor,
    .markdown-body h3:hover .anchor,
    .markdown-body h4:hover .anchor,
    .markdown-body h5:hover .anchor,
    .markdown-body h6:hover .anchor {
      text-decoration: none;
    }

    .markdown-body h1:hover .anchor .octicon-link,
    .markdown-body h2:hover .anchor .octicon-link,
    .markdown-body h3:hover .anchor .octicon-link,
    .markdown-body h4:hover .anchor .octicon-link,
    .markdown-body h5:hover .anchor .octicon-link,
    .markdown-body h6:hover .anchor .octicon-link {
      visibility: visible;
    }

    .markdown-body h1 tt,
    .markdown-body h1 code,
    .markdown-body h2 tt,
    .markdown-body h2 code,
    .markdown-body h3 tt,
    .markdown-body h3 code,
    .markdown-body h4 tt,
    .markdown-body h4 code,
    .markdown-body h5 tt,
    .markdown-body h5 code,
    .markdown-body h6 tt,
    .markdown-body h6 code {
      padding: 0 .2em;
      font-size: inherit;
    }

    .markdown-body summary h1,
    .markdown-body summary h2,
    .markdown-body summary h3,
    .markdown-body summary h4,
    .markdown-body summary h5,
    .markdown-body summary h6 {
      display: inline-block;
    }

    .markdown-body summary h1 .anchor,
    .markdown-body summary h2 .anchor,
    .markdown-body summary h3 .anchor,
    .markdown-body summary h4 .anchor,
    .markdown-body summary h5 .anchor,
    .markdown-body summary h6 .anchor {
      margin-left: -40px;
    }

    .markdown-body summary h1,
    .markdown-body summary h2 {
      padding-bottom: 0;
      border-bottom: 0;
    }

    .markdown-body ul.no-list,
    .markdown-body ol.no-list {
      padding: 0;
      list-style-type: none;
    }

    .markdown-body ol[type="a s"] {
      list-style-type: lower-alpha;
    }

    .markdown-body ol[type="A s"] {
      list-style-type: upper-alpha;
    }

    .markdown-body ol[type="i s"] {
      list-style-type: lower-roman;
    }

    .markdown-body ol[type="I s"] {
      list-style-type: upper-roman;
    }

    .markdown-body ol[type="1"] {
      list-style-type: decimal;
    }

    .markdown-body div>ol:not([type]) {
      list-style-type: decimal;
    }

    .markdown-body ul ul,
    .markdown-body ul ol,
    .markdown-body ol ol,
    .markdown-body ol ul {
      margin-top: 0;
      margin-bottom: 0;
    }

    .markdown-body li>p {
      margin-top: var(--base-size-16);
    }

    .markdown-body li+li {
      margin-top: .25em;
    }

    .markdown-body dl {
      padding: 0;
    }

    .markdown-body dl dt {
      padding: 0;
      margin-top: var(--base-size-16);
      font-size: 1em;
      font-style: italic;
      font-weight: var(--base-text-weight-semibold, 600);
    }

    .markdown-body dl dd {
      padding: 0 var(--base-size-16);
      margin-bottom: var(--base-size-16);
    }

    .markdown-body table th {
      font-weight: var(--base-text-weight-semibold, 600);
    }

    .markdown-body table th,
    .markdown-body table td {
      padding: 6px 13px;
      border: 1px solid var(--borderColor-default);
    }

    .markdown-body table td>:last-child {
      margin-bottom: 0;
    }

    .markdown-body table tr {
      background-color: var(--bgColor-default);
      border-top: 1px solid var(--borderColor-muted);
    }

    .markdown-body table tr:nth-child(2n) {
      background-color: var(--bgColor-muted);
    }

    .markdown-body table img {
      background-color: transparent;
    }

    .markdown-body img[align=right] {
      padding-left: 20px;
    }

    .markdown-body img[align=left] {
      padding-right: 20px;
    }

    .markdown-body .emoji {
      max-width: none;
      vertical-align: text-top;
      background-color: transparent;
    }

    .markdown-body span.frame {
      display: block;
      overflow: hidden;
    }

    .markdown-body span.frame>span {
      display: block;
      float: left;
      width: auto;
      padding: 7px;
      margin: 13px 0 0;
      overflow: hidden;
      border: 1px solid var(--borderColor-default);
    }

    .markdown-body span.frame span img {
      display: block;
      float: left;
    }

    .markdown-body span.frame span span {
      display: block;
      padding: 5px 0 0;
      clear: both;
      color: var(--fgColor-default);
    }

    .markdown-body span.align-center {
      display: block;
      overflow: hidden;
      clear: both;
    }

    .markdown-body span.align-center>span {
      display: block;
      margin: 13px auto 0;
      overflow: hidden;
      text-align: center;
    }

    .markdown-body span.align-center span img {
      margin: 0 auto;
      text-align: center;
    }

    .markdown-body span.align-right {
      display: block;
      overflow: hidden;
      clear: both;
    }

    .markdown-body span.align-right>span {
      display: block;
      margin: 13px 0 0;
      overflow: hidden;
      text-align: right;
    }

    .markdown-body span.align-right span img {
      margin: 0;
      text-align: right;
    }

    .markdown-body span.float-left {
      display: block;
      float: left;
      margin-right: 13px;
      overflow: hidden;
    }

    .markdown-body span.float-left span {
      margin: 13px 0 0;
    }

    .markdown-body span.float-right {
      display: block;
      float: right;
      margin-left: 13px;
      overflow: hidden;
    }

    .markdown-body span.float-right>span {
      display: block;
      margin: 13px auto 0;
      overflow: hidden;
      text-align: right;
    }

    .markdown-body code,
    .markdown-body tt {
      padding: .2em .4em;
      margin: 0;
      font-size: 85%;
      white-space: break-spaces;
      background-color: var(--bgColor-neutral-muted);
      border-radius: 6px;
    }

    .markdown-body code br,
    .markdown-body tt br {
      display: none;
    }

    .markdown-body del code {
      text-decoration: inherit;
    }

    .markdown-body samp {
      font-size: 85%;
    }

    .markdown-body pre code {
      font-size: 100%;
    }

    .markdown-body pre>code {
      padding: 0;
      margin: 0;
      word-break: break-word;
      white-space: pre-wrap;
      background: transparent;
      border: 0;
    }

    .markdown-body .highlight {
      margin-bottom: var(--base-size-16);
    }

    .markdown-body .highlight pre {
      margin-bottom: 0;
      word-break: normal;
    }

    .markdown-body .highlight pre,
    .markdown-body pre {
      padding: var(--base-size-16);
      overflow: auto;
      font-size: 85%;
      line-height: 1.45;
      color: var(--fgColor-default);
      background-color: var(--bgColor-muted);
      border-radius: 6px;
    }

    .markdown-body pre code,
    .markdown-body pre tt {
      display: inline;
      max-width: auto;
      padding: 0;
      margin: 0;
      overflow: visible;
      line-height: inherit;
      word-wrap: normal;
      background-color: transparent;
      border: 0;
    }

    .markdown-body .csv-data td,
    .markdown-body .csv-data th {
      padding: 5px;
      overflow: hidden;
      font-size: 12px;
      line-height: 1;
      text-align: left;
      white-space: nowrap;
    }

    .markdown-body .csv-data .blob-num {
      padding: 10px var(--base-size-8) 9px;
      text-align: right;
      background: var(--bgColor-default);
      border: 0;
    }

    .markdown-body .csv-data tr {
      border-top: 0;
    }

    .markdown-body .csv-data th {
      font-weight: var(--base-text-weight-semibold, 600);
      background: var(--bgColor-muted);
      border-top: 0;
    }

    .markdown-body [data-footnote-ref]::before {
      content: "[";
    }

    .markdown-body [data-footnote-ref]::after {
      content: "]";
    }

    .markdown-body .footnotes {
      font-size: 12px;
      color: var(--fgColor-muted);
      border-top: 1px solid var(--borderColor-default);
    }

    .markdown-body .footnotes ol {
      padding-left: var(--base-size-16);
    }

    .markdown-body .footnotes ol ul {
      display: inline-block;
      padding-left: var(--base-size-16);
      margin-top: var(--base-size-16);
    }

    .markdown-body .footnotes li {
      position: relative;
    }

    .markdown-body .footnotes li:target::before {
      position: absolute;
      top: calc(var(--base-size-8)*-1);
      right: calc(var(--base-size-8)*-1);
      bottom: calc(var(--base-size-8)*-1);
      left: calc(var(--base-size-24)*-1);
      pointer-events: none;
      content: "";
      border: 2px solid var(--borderColor-accent-emphasis);
      border-radius: 6px;
    }

    .markdown-body .footnotes li:target {
      color: var(--fgColor-default);
    }

    .markdown-body .footnotes .data-footnote-backref g-emoji {
      font-family: monospace;
    }

    .markdown-body body:has(:modal) {
      padding-right: var(--dialog-scrollgutter) !important;
    }

    .markdown-body .pl-c {
      color: var(--color-prettylights-syntax-comment);
    }

    .markdown-body .pl-c1,
    .markdown-body .pl-s .pl-v {
      color: var(--color-prettylights-syntax-constant);
    }

    .markdown-body .pl-e,
    .markdown-body .pl-en {
      color: var(--color-prettylights-syntax-entity);
    }

    .markdown-body .pl-smi,
    .markdown-body .pl-s .pl-s1 {
      color: var(--color-prettylights-syntax-storage-modifier-import);
    }

    .markdown-body .pl-ent {
      color: var(--color-prettylights-syntax-entity-tag);
    }

    .markdown-body .pl-k {
      color: var(--color-prettylights-syntax-keyword);
    }

    .markdown-body .pl-s,
    .markdown-body .pl-pds,
    .markdown-body .pl-s .pl-pse .pl-s1,
    .markdown-body .pl-sr,
    .markdown-body .pl-sr .pl-cce,
    .markdown-body .pl-sr .pl-sre,
    .markdown-body .pl-sr .pl-sra {
      color: var(--color-prettylights-syntax-string);
    }

    .markdown-body .pl-v,
    .markdown-body .pl-smw {
      color: var(--color-prettylights-syntax-variable);
    }

    .markdown-body .pl-bu {
      color: var(--color-prettylights-syntax-brackethighlighter-unmatched);
    }

    .markdown-body .pl-ii {
      color: var(--color-prettylights-syntax-invalid-illegal-text);
      background-color: var(--color-prettylights-syntax-invalid-illegal-bg);
    }

    .markdown-body .pl-c2 {
      color: var(--color-prettylights-syntax-carriage-return-text);
      background-color: var(--color-prettylights-syntax-carriage-return-bg);
    }

    .markdown-body .pl-sr .pl-cce {
      font-weight: bold;
      color: var(--color-prettylights-syntax-string-regexp);
    }

    .markdown-body .pl-ml {
      color: var(--color-prettylights-syntax-markup-list);
    }

    .markdown-body .pl-mh,
    .markdown-body .pl-mh .pl-en,
    .markdown-body .pl-ms {
      font-weight: bold;
      color: var(--color-prettylights-syntax-markup-heading);
    }

    .markdown-body .pl-mi {
      font-style: italic;
      color: var(--color-prettylights-syntax-markup-italic);
    }

    .markdown-body .pl-mb {
      font-weight: bold;
      color: var(--color-prettylights-syntax-markup-bold);
    }

    .markdown-body .pl-md {
      color: var(--color-prettylights-syntax-markup-deleted-text);
      background-color: var(--color-prettylights-syntax-markup-deleted-bg);
    }

    .markdown-body .pl-mi1 {
      color: var(--color-prettylights-syntax-markup-inserted-text);
      background-color: var(--color-prettylights-syntax-markup-inserted-bg);
    }

    .markdown-body .pl-mc {
      color: var(--color-prettylights-syntax-markup-changed-text);
      background-color: var(--color-prettylights-syntax-markup-changed-bg);
    }

    .markdown-body .pl-mi2 {
      color: var(--color-prettylights-syntax-markup-ignored-text);
      background-color: var(--color-prettylights-syntax-markup-ignored-bg);
    }

    .markdown-body .pl-mdr {
      font-weight: bold;
      color: var(--color-prettylights-syntax-meta-diff-range);
    }

    .markdown-body .pl-ba {
      color: var(--color-prettylights-syntax-brackethighlighter-angle);
    }

    .markdown-body .pl-sg {
      color: var(--color-prettylights-syntax-sublimelinter-gutter-mark);
    }

    .markdown-body .pl-corl {
      text-decoration: underline;
      color: var(--color-prettylights-syntax-constant-other-reference-link);
    }

    .markdown-body [role=button]:focus:not(:focus-visible),
    .markdown-body [role=tabpanel][tabindex="0"]:focus:not(:focus-visible),
    .markdown-body button:focus:not(:focus-visible),
    .markdown-body summary:focus:not(:focus-visible),
    .markdown-body a:focus:not(:focus-visible) {
      outline: none;
      box-shadow: none;
    }

    .markdown-body [tabindex="0"]:focus:not(:focus-visible),
    .markdown-body details-dialog:focus:not(:focus-visible) {
      outline: none;
    }

    .markdown-body g-emoji {
      display: inline-block;
      min-width: 1ch;
      font-family: "Apple Color Emoji","Segoe UI Emoji","Segoe UI Symbol";
      font-size: 1em;
      font-style: normal !important;
      font-weight: var(--base-text-weight-normal, 400);
      line-height: 1;
      vertical-align: -0.075em;
    }

    .markdown-body g-emoji img {
      width: 1em;
      height: 1em;
    }

    .markdown-body .task-list-item {
      list-style-type: none;
    }

    .markdown-body .task-list-item label {
      font-weight: var(--base-text-weight-normal, 400);
    }

    .markdown-body .task-list-item.enabled label {
      cursor: pointer;
    }

    .markdown-body .task-list-item+.task-list-item {
      margin-top: var(--base-size-4);
    }

    .markdown-body .task-list-item .handle {
      display: none;
    }

    .markdown-body .task-list-item-checkbox {
      margin: 0 .2em .25em -1.4em;
      vertical-align: middle;
    }

    .markdown-body ul:dir(rtl) .task-list-item-checkbox {
      margin: 0 -1.6em .25em .2em;
    }

    .markdown-body ol:dir(rtl) .task-list-item-checkbox {
      margin: 0 -1.6em .25em .2em;
    }

    .markdown-body .contains-task-list:hover .task-list-item-convert-container,
    .markdown-body .contains-task-list:focus-within .task-list-item-convert-container {
      display: block;
      width: auto;
      height: 24px;
      overflow: visible;
      clip: auto;
    }

    .markdown-body ::-webkit-calendar-picker-indicator {
      filter: invert(50%);
    }

    .markdown-body .markdown-alert {
      padding: var(--base-size-8) var(--base-size-16);
      margin-bottom: var(--base-size-16);
      color: inherit;
      border-left: .25em solid var(--borderColor-default);
    }

    .markdown-body .markdown-alert>:first-child {
      margin-top: 0;
    }

    .markdown-body .markdown-alert>:last-child {
      margin-bottom: 0;
    }

    .markdown-body .markdown-alert .markdown-alert-title {
      display: flex;
      font-weight: var(--base-text-weight-medium, 500);
      align-items: center;
      line-height: 1;
    }

    .markdown-body .markdown-alert.markdown-alert-note {
      border-left-color: var(--borderColor-accent-emphasis);
    }

    .markdown-body .markdown-alert.markdown-alert-note .markdown-alert-title {
      color: var(--fgColor-accent);
    }

    .markdown-body .markdown-alert.markdown-alert-important {
      border-left-color: var(--borderColor-done-emphasis);
    }

    .markdown-body .markdown-alert.markdown-alert-important .markdown-alert-title {
      color: var(--fgColor-done);
    }

    .markdown-body .markdown-alert.markdown-alert-warning {
      border-left-color: var(--borderColor-attention-emphasis);
    }

    .markdown-body .markdown-alert.markdown-alert-warning .markdown-alert-title {
      color: var(--fgColor-attention);
    }

    .markdown-body .markdown-alert.markdown-alert-tip {
      border-left-color: var(--borderColor-success-emphasis);
    }

    .markdown-body .markdown-alert.markdown-alert-tip .markdown-alert-title {
      color: var(--fgColor-success);
    }

    .markdown-body .markdown-alert.markdown-alert-caution {
      border-left-color: var(--borderColor-danger-emphasis);
    }

    .markdown-body .markdown-alert.markdown-alert-caution .markdown-alert-title {
      color: var(--fgColor-danger);
    }

    .markdown-body>*:first-child>.heading-element:first-child {
      margin-top: 0 !important;
    }

    .markdown-body .highlight pre:has(+zeroclipboard-container) {
      min-height: 52px;
    }
  `;cl([F({type:String})],ds.prototype,"value",2);ds=cl([vt("marked-block")],ds);var lh=Object.defineProperty,ch=Object.getOwnPropertyDescriptor,Qr=(e,t,o,i)=>{for(var s=i>1?void 0:i?ch(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&lh(t,o,s),s};let Ao=class extends nt{constructor(){super(...arguments),this.collapsed=!0,this.toggle=()=>{this.value&&this.value.executionStatus!=="completed"||(this.collapsed=!this.collapsed)},this.denied=()=>{this.value&&(this.value.approvalStatus="rejected",this.dispatchEvent(new CustomEvent("tool-change",{detail:this.value})),this.requestUpdate())},this.confirmed=()=>{this.value&&(this.value.approvalStatus="approved",this.dispatchEvent(new CustomEvent("tool-change",{detail:this.value})),this.requestUpdate())}}render(){var e,t,o;return this.value?B`
      <div class="container">
        <div class="header" @click=${this.toggle}>
          ${this.value.executionStatus==="pending"?B`<div class="title">⏳ Tool Waiting: ${this.value.name}</div>`:this.value.executionStatus==="running"?B`<div class="title">🔄 Tool Used: ${this.value.name}</div>`:this.value.executionStatus==="completed"&&((e=this.value.result)!=null&&e.isSuccess)?B`<div class="title">✅ Tool Result: ${this.value.name}</div>`:this.value.executionStatus==="completed"&&!((t=this.value.result)!=null&&t.isSuccess)?B`<div class="title">❌ Tool Failed: ${this.value.name}</div>`:U}
          
          ${this.value.executionStatus==="completed"?this.collapsed?B`<uc-icon name="plus"></uc-icon>`:B`<uc-icon name="minus"></uc-icon>`:U}
        </div>
        <div class="body">
          <div class="content">
            <div class="label">Argument</div>
            <pre class="value">${this.value.arguments}</pre>
            <div class="label">Result</div>
            <pre class="value">${(o=this.value.result)==null?void 0:o.data}</pre>
          </div>
        </div>
        ${this.value.approvalStatus==="requires"?B`
            <div class="footer">
              <uc-button @click=${this.denied}>
                Deny
              </uc-button>
              <uc-button @click=${this.confirmed}>
                Confirm
              </uc-button>
            </div>`:U}
      </div>
    `:U}};Ao.styles=bt`
    :host {
      display: block;
      width: 100%;
      height: auto;
    }
    :host([collapsed]) .body {
      height: 0;
      padding: 0;
      overflow: hidden;
    }

    .container {
      display: block;
      border-radius: 8px;
      border: 1px solid var(--uc-border-color-mid);
      box-sizing: border-box;
    }

    .header {
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      padding: 5px 10px;
      box-sizing: border-box;
      cursor: pointer;
    }
    .header .title {
      font-size: 16px;
      font-weight: 600;
      line-height: 24px;
    }

    .body {
      height: auto;
      padding: 5px 10px;
      overflow-wrap: anywhere;
      box-sizing: border-box;
    }

    .footer {
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: flex-end;
      gap: 8px;
      padding: 5px 10px;
      box-sizing: border-box;
    }

    .footer uc-button {
      font-size: 12px;
    }

    .content {
      width: 100%;
      display: flex;
      flex-direction: column;
      border: 1px solid var(--uc-border-color-mid);
      border-radius: 8px;
      box-sizing: border-box;
      color: var(--uc-text-color-high);
      font-size: 12px;
      line-height: 1.5;
    }

    .content .label {
      width: 100%;
      font-weight: 600;
      padding: 4px 8px;
      border-bottom: 1px solid var(--uc-border-color-mid);
      box-sizing: border-box;
      background-color: var(--uc-background-color-500);
    }

    .content .value {
      width: 100%;
      white-space: pre-wrap;
      overflow-wrap: anywhere;
      font-weight: 300;
      overflow: auto;
      max-height: 200px;
      padding: 8px;
      margin: 0;
      box-sizing: border-box;
      background-color: var(--uc-background-color-200);
    }

  `;Qr([F({type:Object})],Ao.prototype,"value",2);Qr([F({type:Boolean,reflect:!0})],Ao.prototype,"collapsed",2);Ao=Qr([vt("tool-block")],Ao);const dh=["top","right","bottom","left"],Za=["start","end"],Ga=dh.reduce((e,t)=>e.concat(t,t+"-"+Za[0],t+"-"+Za[1]),[]),dr=Math.min,xo=Math.max,hs=Math.round,he=e=>({x:e,y:e}),hh={left:"right",right:"left",bottom:"top",top:"bottom"},uh={start:"end",end:"start"};function Ja(e,t,o){return xo(e,dr(t,o))}function Ti(e,t){return typeof e=="function"?e(t):e}function ke(e){return e.split("-")[0]}function xe(e){return e.split("-")[1]}function dl(e){return e==="x"?"y":"x"}function hl(e){return e==="y"?"height":"width"}function Ge(e){return["top","bottom"].includes(ke(e))?"y":"x"}function ul(e){return dl(Ge(e))}function pl(e,t,o){o===void 0&&(o=!1);const i=xe(e),s=ul(e),r=hl(s);let a=s==="x"?i===(o?"end":"start")?"right":"left":i==="start"?"bottom":"top";return t.reference[r]>t.floating[r]&&(a=ps(a)),[a,ps(a)]}function ph(e){const t=ps(e);return[us(e),t,us(t)]}function us(e){return e.replace(/start|end/g,t=>uh[t])}function fh(e,t,o){const i=["left","right"],s=["right","left"],r=["top","bottom"],a=["bottom","top"];switch(e){case"top":case"bottom":return o?t?s:i:t?i:s;case"left":case"right":return t?r:a;default:return[]}}function mh(e,t,o,i){const s=xe(e);let r=fh(ke(e),o==="start",i);return s&&(r=r.map(a=>a+"-"+s),t&&(r=r.concat(r.map(us)))),r}function ps(e){return e.replace(/left|right|bottom|top/g,t=>hh[t])}function gh(e){return{top:0,right:0,bottom:0,left:0,...e}}function bh(e){return typeof e!="number"?gh(e):{top:e,right:e,bottom:e,left:e}}function fs(e){const{x:t,y:o,width:i,height:s}=e;return{width:i,height:s,top:o,left:t,right:t+i,bottom:o+s,x:t,y:o}}function tn(e,t,o){let{reference:i,floating:s}=e;const r=Ge(t),a=ul(t),n=hl(a),c=ke(t),d=r==="y",u=i.x+i.width/2-s.width/2,h=i.y+i.height/2-s.height/2,f=i[n]/2-s[n]/2;let m;switch(c){case"top":m={x:u,y:i.y-s.height};break;case"bottom":m={x:u,y:i.y+i.height};break;case"right":m={x:i.x+i.width,y:h};break;case"left":m={x:i.x-s.width,y:h};break;default:m={x:i.x,y:i.y}}switch(xe(t)){case"start":m[a]-=f*(o&&d?-1:1);break;case"end":m[a]+=f*(o&&d?-1:1);break}return m}const vh=async(e,t,o)=>{const{placement:i="bottom",strategy:s="absolute",middleware:r=[],platform:a}=o,n=r.filter(Boolean),c=await(a.isRTL==null?void 0:a.isRTL(t));let d=await a.getElementRects({reference:e,floating:t,strategy:s}),{x:u,y:h}=tn(d,i,c),f=i,m={},g=0;for(let b=0;b<n.length;b++){const{name:k,fn:$}=n[b],{x:w,y:_,data:v,reset:y}=await $({x:u,y:h,initialPlacement:i,placement:f,strategy:s,middlewareData:m,rects:d,platform:a,elements:{reference:e,floating:t}});u=w??u,h=_??h,m={...m,[k]:{...m[k],...v}},y&&g<=50&&(g++,typeof y=="object"&&(y.placement&&(f=y.placement),y.rects&&(d=y.rects===!0?await a.getElementRects({reference:e,floating:t,strategy:s}):y.rects),{x:u,y:h}=tn(d,f,c)),b=-1)}return{x:u,y:h,placement:f,strategy:s,middlewareData:m}};async function Zr(e,t){var o;t===void 0&&(t={});const{x:i,y:s,platform:r,rects:a,elements:n,strategy:c}=e,{boundary:d="clippingAncestors",rootBoundary:u="viewport",elementContext:h="floating",altBoundary:f=!1,padding:m=0}=Ti(t,e),g=bh(m),k=n[f?h==="floating"?"reference":"floating":h],$=fs(await r.getClippingRect({element:(o=await(r.isElement==null?void 0:r.isElement(k)))==null||o?k:k.contextElement||await(r.getDocumentElement==null?void 0:r.getDocumentElement(n.floating)),boundary:d,rootBoundary:u,strategy:c})),w=h==="floating"?{x:i,y:s,width:a.floating.width,height:a.floating.height}:a.reference,_=await(r.getOffsetParent==null?void 0:r.getOffsetParent(n.floating)),v=await(r.isElement==null?void 0:r.isElement(_))?await(r.getScale==null?void 0:r.getScale(_))||{x:1,y:1}:{x:1,y:1},y=fs(r.convertOffsetParentRelativeRectToViewportRelativeRect?await r.convertOffsetParentRelativeRectToViewportRelativeRect({elements:n,rect:w,offsetParent:_,strategy:c}):w);return{top:($.top-y.top+g.top)/v.y,bottom:(y.bottom-$.bottom+g.bottom)/v.y,left:($.left-y.left+g.left)/v.x,right:(y.right-$.right+g.right)/v.x}}function yh(e,t,o){return(e?[...o.filter(s=>xe(s)===e),...o.filter(s=>xe(s)!==e)]:o.filter(s=>ke(s)===s)).filter(s=>e?xe(s)===e||(t?us(s)!==s:!1):!0)}const wh=function(e){return e===void 0&&(e={}),{name:"autoPlacement",options:e,async fn(t){var o,i,s;const{rects:r,middlewareData:a,placement:n,platform:c,elements:d}=t,{crossAxis:u=!1,alignment:h,allowedPlacements:f=Ga,autoAlignment:m=!0,...g}=Ti(e,t),b=h!==void 0||f===Ga?yh(h||null,m,f):f,k=await Zr(t,g),$=((o=a.autoPlacement)==null?void 0:o.index)||0,w=b[$];if(w==null)return{};const _=pl(w,r,await(c.isRTL==null?void 0:c.isRTL(d.floating)));if(n!==w)return{reset:{placement:b[0]}};const v=[k[ke(w)],k[_[0]],k[_[1]]],y=[...((i=a.autoPlacement)==null?void 0:i.overflows)||[],{placement:w,overflows:v}],P=b[$+1];if(P)return{data:{index:$+1,overflows:y},reset:{placement:P}};const M=y.map(A=>{const Z=xe(A.placement);return[A.placement,Z&&u?A.overflows.slice(0,2).reduce((tt,dt)=>tt+dt,0):A.overflows[0],A.overflows]}).sort((A,Z)=>A[1]-Z[1]),L=((s=M.filter(A=>A[2].slice(0,xe(A[0])?2:3).every(Z=>Z<=0))[0])==null?void 0:s[0])||M[0][0];return L!==n?{data:{index:$+1,overflows:y},reset:{placement:L}}:{}}}},xh=function(e){return e===void 0&&(e={}),{name:"flip",options:e,async fn(t){var o,i;const{placement:s,middlewareData:r,rects:a,initialPlacement:n,platform:c,elements:d}=t,{mainAxis:u=!0,crossAxis:h=!0,fallbackPlacements:f,fallbackStrategy:m="bestFit",fallbackAxisSideDirection:g="none",flipAlignment:b=!0,...k}=Ti(e,t);if((o=r.arrow)!=null&&o.alignmentOffset)return{};const $=ke(s),w=Ge(n),_=ke(n)===n,v=await(c.isRTL==null?void 0:c.isRTL(d.floating)),y=f||(_||!b?[ps(n)]:ph(n)),P=g!=="none";!f&&P&&y.push(...mh(n,b,g,v));const M=[n,...y],I=await Zr(t,k),L=[];let A=((i=r.flip)==null?void 0:i.overflows)||[];if(u&&L.push(I[$]),h){const pt=pl(s,a,v);L.push(I[pt[0]],I[pt[1]])}if(A=[...A,{placement:s,overflows:L}],!L.every(pt=>pt<=0)){var Z,tt;const pt=(((Z=r.flip)==null?void 0:Z.index)||0)+1,Lt=M[pt];if(Lt){var dt;const kt=h==="alignment"?w!==Ge(Lt):!1,ft=((dt=A[0])==null?void 0:dt.overflows[0])>0;if(!kt||ft)return{data:{index:pt,overflows:A},reset:{placement:Lt}}}let At=(tt=A.filter(kt=>kt.overflows[0]<=0).sort((kt,ft)=>kt.overflows[1]-ft.overflows[1])[0])==null?void 0:tt.placement;if(!At)switch(m){case"bestFit":{var it;const kt=(it=A.filter(ft=>{if(P){const ne=Ge(ft.placement);return ne===w||ne==="y"}return!0}).map(ft=>[ft.placement,ft.overflows.filter(ne=>ne>0).reduce((ne,cc)=>ne+cc,0)]).sort((ft,ne)=>ft[1]-ne[1])[0])==null?void 0:it[0];kt&&(At=kt);break}case"initialPlacement":At=n;break}if(s!==At)return{reset:{placement:At}}}return{}}}};async function kh(e,t){const{placement:o,platform:i,elements:s}=e,r=await(i.isRTL==null?void 0:i.isRTL(s.floating)),a=ke(o),n=xe(o),c=Ge(o)==="y",d=["left","top"].includes(a)?-1:1,u=r&&c?-1:1,h=Ti(t,e);let{mainAxis:f,crossAxis:m,alignmentAxis:g}=typeof h=="number"?{mainAxis:h,crossAxis:0,alignmentAxis:null}:{mainAxis:h.mainAxis||0,crossAxis:h.crossAxis||0,alignmentAxis:h.alignmentAxis};return n&&typeof g=="number"&&(m=n==="end"?g*-1:g),c?{x:m*u,y:f*d}:{x:f*d,y:m*u}}const _h=function(e){return e===void 0&&(e=0),{name:"offset",options:e,async fn(t){var o,i;const{x:s,y:r,placement:a,middlewareData:n}=t,c=await kh(t,e);return a===((o=n.offset)==null?void 0:o.placement)&&(i=n.arrow)!=null&&i.alignmentOffset?{}:{x:s+c.x,y:r+c.y,data:{...c,placement:a}}}}},$h=function(e){return e===void 0&&(e={}),{name:"shift",options:e,async fn(t){const{x:o,y:i,placement:s}=t,{mainAxis:r=!0,crossAxis:a=!1,limiter:n={fn:k=>{let{x:$,y:w}=k;return{x:$,y:w}}},...c}=Ti(e,t),d={x:o,y:i},u=await Zr(t,c),h=Ge(ke(s)),f=dl(h);let m=d[f],g=d[h];if(r){const k=f==="y"?"top":"left",$=f==="y"?"bottom":"right",w=m+u[k],_=m-u[$];m=Ja(w,m,_)}if(a){const k=h==="y"?"top":"left",$=h==="y"?"bottom":"right",w=g+u[k],_=g-u[$];g=Ja(w,g,_)}const b=n.fn({...t,[f]:m,[h]:g});return{...b,data:{x:b.x-o,y:b.y-i,enabled:{[f]:r,[h]:a}}}}}};function zs(){return typeof window<"u"}function Ro(e){return fl(e)?(e.nodeName||"").toLowerCase():"#document"}function Ft(e){var t;return(e==null||(t=e.ownerDocument)==null?void 0:t.defaultView)||window}function Se(e){var t;return(t=(fl(e)?e.ownerDocument:e.document)||window.document)==null?void 0:t.documentElement}function fl(e){return zs()?e instanceof Node||e instanceof Ft(e).Node:!1}function Qt(e){return zs()?e instanceof Element||e instanceof Ft(e).Element:!1}function fe(e){return zs()?e instanceof HTMLElement||e instanceof Ft(e).HTMLElement:!1}function en(e){return!zs()||typeof ShadowRoot>"u"?!1:e instanceof ShadowRoot||e instanceof Ft(e).ShadowRoot}function Pi(e){const{overflow:t,overflowX:o,overflowY:i,display:s}=Zt(e);return/auto|scroll|overlay|hidden|clip/.test(t+i+o)&&!["inline","contents"].includes(s)}function Ch(e){return["table","td","th"].includes(Ro(e))}function Es(e){return[":popover-open",":modal"].some(t=>{try{return e.matches(t)}catch{return!1}})}function Gr(e){const t=Jr(),o=Qt(e)?Zt(e):e;return["transform","translate","scale","rotate","perspective"].some(i=>o[i]?o[i]!=="none":!1)||(o.containerType?o.containerType!=="normal":!1)||!t&&(o.backdropFilter?o.backdropFilter!=="none":!1)||!t&&(o.filter?o.filter!=="none":!1)||["transform","translate","scale","rotate","perspective","filter"].some(i=>(o.willChange||"").includes(i))||["paint","layout","strict","content"].some(i=>(o.contain||"").includes(i))}function Sh(e){let t=De(e);for(;fe(t)&&!zo(t);){if(Gr(t))return t;if(Es(t))return null;t=De(t)}return null}function Jr(){return typeof CSS>"u"||!CSS.supports?!1:CSS.supports("-webkit-backdrop-filter","none")}function zo(e){return["html","body","#document"].includes(Ro(e))}function Zt(e){return Ft(e).getComputedStyle(e)}function Ts(e){return Qt(e)?{scrollLeft:e.scrollLeft,scrollTop:e.scrollTop}:{scrollLeft:e.scrollX,scrollTop:e.scrollY}}function De(e){if(Ro(e)==="html")return e;const t=e.assignedSlot||e.parentNode||en(e)&&e.host||Se(e);return en(t)?t.host:t}function ml(e){const t=De(e);return zo(t)?e.ownerDocument?e.ownerDocument.body:e.body:fe(t)&&Pi(t)?t:ml(t)}function gl(e,t,o){var i;t===void 0&&(t=[]);const s=ml(e),r=s===((i=e.ownerDocument)==null?void 0:i.body),a=Ft(s);return r?(hr(a),t.concat(a,a.visualViewport||[],Pi(s)?s:[],[])):t.concat(s,gl(s,[]))}function hr(e){return e.parent&&Object.getPrototypeOf(e.parent)?e.frameElement:null}function bl(e){const t=Zt(e);let o=parseFloat(t.width)||0,i=parseFloat(t.height)||0;const s=fe(e),r=s?e.offsetWidth:o,a=s?e.offsetHeight:i,n=hs(o)!==r||hs(i)!==a;return n&&(o=r,i=a),{width:o,height:i,$:n}}function vl(e){return Qt(e)?e:e.contextElement}function ko(e){const t=vl(e);if(!fe(t))return he(1);const o=t.getBoundingClientRect(),{width:i,height:s,$:r}=bl(t);let a=(r?hs(o.width):o.width)/i,n=(r?hs(o.height):o.height)/s;return(!a||!Number.isFinite(a))&&(a=1),(!n||!Number.isFinite(n))&&(n=1),{x:a,y:n}}const Ah=he(0);function yl(e){const t=Ft(e);return!Jr()||!t.visualViewport?Ah:{x:t.visualViewport.offsetLeft,y:t.visualViewport.offsetTop}}function zh(e,t,o){return t===void 0&&(t=!1),!o||t&&o!==Ft(e)?!1:t}function yi(e,t,o,i){t===void 0&&(t=!1),o===void 0&&(o=!1);const s=e.getBoundingClientRect(),r=vl(e);let a=he(1);t&&(i?Qt(i)&&(a=ko(i)):a=ko(e));const n=zh(r,o,i)?yl(r):he(0);let c=(s.left+n.x)/a.x,d=(s.top+n.y)/a.y,u=s.width/a.x,h=s.height/a.y;if(r){const f=Ft(r),m=i&&Qt(i)?Ft(i):i;let g=f,b=hr(g);for(;b&&i&&m!==g;){const k=ko(b),$=b.getBoundingClientRect(),w=Zt(b),_=$.left+(b.clientLeft+parseFloat(w.paddingLeft))*k.x,v=$.top+(b.clientTop+parseFloat(w.paddingTop))*k.y;c*=k.x,d*=k.y,u*=k.x,h*=k.y,c+=_,d+=v,g=Ft(b),b=hr(g)}}return fs({width:u,height:h,x:c,y:d})}function ta(e,t){const o=Ts(e).scrollLeft;return t?t.left+o:yi(Se(e)).left+o}function wl(e,t,o){o===void 0&&(o=!1);const i=e.getBoundingClientRect(),s=i.left+t.scrollLeft-(o?0:ta(e,i)),r=i.top+t.scrollTop;return{x:s,y:r}}function Eh(e){let{elements:t,rect:o,offsetParent:i,strategy:s}=e;const r=s==="fixed",a=Se(i),n=t?Es(t.floating):!1;if(i===a||n&&r)return o;let c={scrollLeft:0,scrollTop:0},d=he(1);const u=he(0),h=fe(i);if((h||!h&&!r)&&((Ro(i)!=="body"||Pi(a))&&(c=Ts(i)),fe(i))){const m=yi(i);d=ko(i),u.x=m.x+i.clientLeft,u.y=m.y+i.clientTop}const f=a&&!h&&!r?wl(a,c,!0):he(0);return{width:o.width*d.x,height:o.height*d.y,x:o.x*d.x-c.scrollLeft*d.x+u.x+f.x,y:o.y*d.y-c.scrollTop*d.y+u.y+f.y}}function Th(e){return Array.from(e.getClientRects())}function Ph(e){const t=Se(e),o=Ts(e),i=e.ownerDocument.body,s=xo(t.scrollWidth,t.clientWidth,i.scrollWidth,i.clientWidth),r=xo(t.scrollHeight,t.clientHeight,i.scrollHeight,i.clientHeight);let a=-o.scrollLeft+ta(e);const n=-o.scrollTop;return Zt(i).direction==="rtl"&&(a+=xo(t.clientWidth,i.clientWidth)-s),{width:s,height:r,x:a,y:n}}function Oh(e,t){const o=Ft(e),i=Se(e),s=o.visualViewport;let r=i.clientWidth,a=i.clientHeight,n=0,c=0;if(s){r=s.width,a=s.height;const d=Jr();(!d||d&&t==="fixed")&&(n=s.offsetLeft,c=s.offsetTop)}return{width:r,height:a,x:n,y:c}}function Lh(e,t){const o=yi(e,!0,t==="fixed"),i=o.top+e.clientTop,s=o.left+e.clientLeft,r=fe(e)?ko(e):he(1),a=e.clientWidth*r.x,n=e.clientHeight*r.y,c=s*r.x,d=i*r.y;return{width:a,height:n,x:c,y:d}}function on(e,t,o){let i;if(t==="viewport")i=Oh(e,o);else if(t==="document")i=Ph(Se(e));else if(Qt(t))i=Lh(t,o);else{const s=yl(e);i={x:t.x-s.x,y:t.y-s.y,width:t.width,height:t.height}}return fs(i)}function xl(e,t){const o=De(e);return o===t||!Qt(o)||zo(o)?!1:Zt(o).position==="fixed"||xl(o,t)}function Rh(e,t){const o=t.get(e);if(o)return o;let i=gl(e,[]).filter(n=>Qt(n)&&Ro(n)!=="body"),s=null;const r=Zt(e).position==="fixed";let a=r?De(e):e;for(;Qt(a)&&!zo(a);){const n=Zt(a),c=Gr(a);!c&&n.position==="fixed"&&(s=null),(r?!c&&!s:!c&&n.position==="static"&&!!s&&["absolute","fixed"].includes(s.position)||Pi(a)&&!c&&xl(e,a))?i=i.filter(u=>u!==a):s=n,a=De(a)}return t.set(e,i),i}function Dh(e){let{element:t,boundary:o,rootBoundary:i,strategy:s}=e;const a=[...o==="clippingAncestors"?Es(t)?[]:Rh(t,this._c):[].concat(o),i],n=a[0],c=a.reduce((d,u)=>{const h=on(t,u,s);return d.top=xo(h.top,d.top),d.right=dr(h.right,d.right),d.bottom=dr(h.bottom,d.bottom),d.left=xo(h.left,d.left),d},on(t,n,s));return{width:c.right-c.left,height:c.bottom-c.top,x:c.left,y:c.top}}function Mh(e){const{width:t,height:o}=bl(e);return{width:t,height:o}}function Ih(e,t,o){const i=fe(t),s=Se(t),r=o==="fixed",a=yi(e,!0,r,t);let n={scrollLeft:0,scrollTop:0};const c=he(0);function d(){c.x=ta(s)}if(i||!i&&!r)if((Ro(t)!=="body"||Pi(s))&&(n=Ts(t)),i){const m=yi(t,!0,r,t);c.x=m.x+t.clientLeft,c.y=m.y+t.clientTop}else s&&d();r&&!i&&s&&d();const u=s&&!i&&!r?wl(s,n):he(0),h=a.left+n.scrollLeft-c.x-u.x,f=a.top+n.scrollTop-c.y-u.y;return{x:h,y:f,width:a.width,height:a.height}}function Xs(e){return Zt(e).position==="static"}function sn(e,t){if(!fe(e)||Zt(e).position==="fixed")return null;if(t)return t(e);let o=e.offsetParent;return Se(e)===o&&(o=o.ownerDocument.body),o}function kl(e,t){const o=Ft(e);if(Es(e))return o;if(!fe(e)){let s=De(e);for(;s&&!zo(s);){if(Qt(s)&&!Xs(s))return s;s=De(s)}return o}let i=sn(e,t);for(;i&&Ch(i)&&Xs(i);)i=sn(i,t);return i&&zo(i)&&Xs(i)&&!Gr(i)?o:i||Sh(e)||o}const Bh=async function(e){const t=this.getOffsetParent||kl,o=this.getDimensions,i=await o(e.floating);return{reference:Ih(e.reference,await t(e.floating),e.strategy),floating:{x:0,y:0,width:i.width,height:i.height}}};function Fh(e){return Zt(e).direction==="rtl"}const Vh={convertOffsetParentRelativeRectToViewportRelativeRect:Eh,getDocumentElement:Se,getClippingRect:Dh,getOffsetParent:kl,getElementRects:Bh,getClientRects:Th,getDimensions:Mh,getScale:ko,isElement:Qt,isRTL:Fh},_l=_h,Uh=wh,$l=$h,Cl=xh,Sl=(e,t,o)=>{const i=new Map,s={platform:Vh,...o},r={...s.platform,_c:i};return vh(e,t,{...s,platform:r})};var Nh=Object.defineProperty,Hh=Object.getOwnPropertyDescriptor,Do=(e,t,o,i)=>{for(var s=i>1?void 0:i?Hh(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&Nh(t,o,s),s};let Me=class extends nt{constructor(){super(...arguments),this.disabled=!1,this.loading=!1,this.showTooltip=async()=>{const{x:e,y:t}=await Sl(this,this.tooltipEl,{placement:this.tooltipPosition||"top",middleware:[Cl(),$l(),_l(4)]});Object.assign(this.tooltipEl.style,{left:`${e}px`,top:`${t}px`}),this.tooltipEl.classList.add("visible")},this.hideTooltip=()=>{this.tooltipEl.classList.remove("visible")}}connectedCallback(){super.connectedCallback(),this.tabIndex=0,this.tooltip&&(this.addEventListener("mouseenter",this.showTooltip),this.addEventListener("mouseleave",this.hideTooltip),this.addEventListener("focusin",this.showTooltip),this.addEventListener("focusout",this.hideTooltip))}disconnectedCallback(){this.tooltip&&(this.removeEventListener("mouseenter",this.showTooltip),this.removeEventListener("mouseleave",this.hideTooltip),this.removeEventListener("focusin",this.showTooltip),this.removeEventListener("focusout",this.hideTooltip)),super.disconnectedCallback()}render(){return B`
      <slot></slot>
      <div class="overlay">
        <uc-spinner></uc-spinner>
      </div>
      <div class="tooltip">
        ${this.tooltip}
      </div>
    `}};Me.styles=bt`
    :host {
      position: relative;
      display: flex;
      align-items: center;
      justify-content: center;

      border: 1px solid var(--uc-border-color-mid);
      border-radius: 8px;
      padding: 8px;
      font-size: 16px;
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      background-color: var(--uc-background-color-0);
      
      transition: opacity 0.3s ease;
      box-sizing: border-box;
      user-select: none;
      cursor: pointer;
    }
    :host([disabled]) {
      opacity: 0.5;
      pointer-events: none;
    }
    :host([loading]) .overlay {
      display: flex;
      pointer-events: none;
    }
    :host(:hover) {
      background-color: var(--uc-background-color-100);
    }

    .overlay {
      display: none;
      z-index: 100;
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      align-items: center;
      justify-content: center;

      padding: inherit;
      font-size: inherit;
      border-radius: inherit;
      background-color: inherit;
    }

    .tooltip {
      display: block;
      z-index: 1000;
      position: absolute;
      width: max-content;
      top: 0;
      left: 0;
      
      padding: 6px 12px;
      border-radius: 4px;
      font-size: 14px;

      background-color: var(--uc-background-color-1000);
      
      backdrop-filter: blur(5px);
      color: white;

      transition: opacity 0.3s ease, transform 0.3s ease;
      opacity: 0;
      pointer-events: none;
    }
    .tooltip.visible {
      opacity: 1;
      pointer-events: auto;
      transform: translateY(-4px);
    }
  `;Do([no(".tooltip")],Me.prototype,"tooltipEl",2);Do([F({type:Boolean,reflect:!0})],Me.prototype,"disabled",2);Do([F({type:Boolean,reflect:!0})],Me.prototype,"loading",2);Do([F({type:String})],Me.prototype,"tooltip",2);Do([F({type:String})],Me.prototype,"tooltipPosition",2);Me=Do([vt("uc-button")],Me);var jh=Object.defineProperty,qh=Object.getOwnPropertyDescriptor,ea=(e,t,o,i)=>{for(var s=i>1?void 0:i?qh(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&jh(t,o,s),s};let wi=class extends nt{constructor(){super(...arguments),this.disabled=!1,this.loading=!1}render(){return B`
      <uc-button ?disabled=${this.disabled} ?loading=${this.loading} tooltip="Clear">
        <uc-icon name="clear"></uc-icon>
      </uc-button>
    `}};wi.styles=bt`
    :host {
      display: inline-block;
      padding: 0;
    }

    uc-button {
      padding: 6px;
    }

    uc-icon {
      font-size: 20px;
    }
  `;ea([F({type:Boolean})],wi.prototype,"disabled",2);ea([F({type:Boolean})],wi.prototype,"loading",2);wi=ea([vt("uc-clear-button")],wi);/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const rn=(e,t,o)=>{const i=new Map;for(let s=t;s<=o;s++)i.set(e[s],s);return i},Gi=Fr(class extends Vr{constructor(e){if(super(e),e.type!==Br.CHILD)throw Error("repeat() can only be used in text expressions")}dt(e,t,o){let i;o===void 0?o=t:t!==void 0&&(i=t);const s=[],r=[];let a=0;for(const n of e)s[a]=i?i(n,a):a,r[a]=o(n,a),a++;return{values:r,keys:s}}render(e,t,o){return this.dt(e,t,o).values}update(e,[t,o,i]){const s=md(e),{values:r,keys:a}=this.dt(t,o,i);if(!Array.isArray(s))return this.ut=a,r;const n=this.ut??(this.ut=[]),c=[];let d,u,h=0,f=s.length-1,m=0,g=r.length-1;for(;h<=f&&m<=g;)if(s[h]===null)h++;else if(s[f]===null)f--;else if(n[h]===a[m])c[m]=qe(s[h],r[m]),h++,m++;else if(n[f]===a[g])c[g]=qe(s[f],r[g]),f--,g--;else if(n[h]===a[g])c[g]=qe(s[h],r[g]),Wo(e,c[g+1],s[h]),h++,g--;else if(n[f]===a[m])c[m]=qe(s[f],r[m]),Wo(e,s[h],s[f]),f--,m++;else if(d===void 0&&(d=rn(a,m,g),u=rn(n,h,f)),d.has(n[h]))if(d.has(n[f])){const b=u.get(a[m]),k=b!==void 0?s[b]:null;if(k===null){const $=Wo(e,s[h]);qe($,r[m]),c[m]=$}else c[m]=qe(k,r[m]),Wo(e,s[h],k),s[b]=null;m++}else Ys(s[f]),f--;else Ys(s[h]),h++;for(;m<=g;){const b=Wo(e,c[g+1]);qe(b,r[m]),c[m++]=b}for(;h<=f;){const b=s[h++];b!==null&&Ys(b)}return this.ut=a,fd(e,c),pe}});var Wh=Object.defineProperty,Kh=Object.getOwnPropertyDescriptor,co=(e,t,o,i)=>{for(var s=i>1?void 0:i?Kh(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&Wh(t,o,s),s};let _e=class extends nt{constructor(){super(...arguments),this.open=!1,this.placeholder="Choose a model",this.models=[],this.toggle=()=>{this.open=!this.open},this.select=e=>{this.selectedModel=e,this.dispatchEvent(new CustomEvent("select",{detail:this.selectedModel,bubbles:!0,composed:!0})),this.open=!1},this.show=async()=>{this.open&&(await this.compute(),this.listEl.classList.add("open"))},this.hide=async()=>{this.open||(this.listEl.classList.remove("open"),this.dispatchEvent(new CustomEvent("popup",{bubbles:!0,composed:!0})))},this.compute=async()=>{const{x:e,y:t}=await Sl(this,this.listEl,{middleware:[Cl(),$l(),_l(),Uh({allowedPlacements:["top-start","bottom-start"]})]});Object.assign(this.listEl.style,{left:`${e}px`,top:`${t}px`})}}connectedCallback(){super.connectedCallback(),this.addEventListener("focusout",()=>this.open=!1)}disconnectedCallback(){this.removeEventListener("focusout",()=>this.open=!1),super.disconnectedCallback()}updated(e){super.updated(e),e.has("open")&&(this.open?this.show():this.hide())}render(){var e;return B`
      <div class="selecter" tabindex="0" @click=${()=>this.toggle()}>
        <div class="value">
          ${((e=this.selectedModel)==null?void 0:e.displayName)||this.placeholder}
        </div>
        <uc-icon class="icon"
          name=${this.open?"chevron-up":"chevron-down"}
        ></uc-icon>
      </div>
      <div class="list" tabindex="0">
        ${Gi(this.models,t=>t.modelId,t=>{var i;const o=((i=this.selectedModel)==null?void 0:i.modelId)===t.modelId;return B`
            <div class="item" ?selected=${o} @click=${()=>this.select(t)}>
              <div class="display">
                ${t.displayName}
                ${o?B`<uc-icon name="check"></uc-icon>`:U}
              </div>
              <div class="description">
                ${t.description}
              </div>
            </div>
          `})}
      </div>
    `}};_e.styles=bt`
    :host {
      position: relative;
      display: block;
      background-color: var(--uc-background-color-0);
      border: 1px solid var(--uc-border-color-low);
      border-radius: 8px;
      padding: 8px 12px;
      box-sizing: border-box;
    }

    .selecter {
      position: relative;
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      cursor: pointer;
      font-size: 14px;
      line-height: 16px;
      gap: 8px;
    }

    /* 리스트 스타일 */
    .list {
      position: absolute;
      width: max-content;
      top: 0;
      left: 0;

      display: flex;
      flex-direction: column;
      visibility: hidden;
      opacity: 0;

      border-radius: 8px;
      border: 1px solid var(--uc-border-color-low);
      background-color: var(--uc-background-color-0);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
      z-index: 1000;

      max-height: 260px;
      overflow: auto;
      box-sizing: border-box;

      scrollbar-color: var(--uc-background-color-800) transparent;
      scrollbar-width: thin;
    }
    .list.open {
      visibility: visible;
      opacity: 1;
    }

    .item {
      position: relative;
      padding: 6px 12px;
      display: flex;
      flex-direction: column;
      transition: background-color 0.2s, color 0.2s;
      box-sizing: border-box;
      cursor: pointer;
      
      .display {
        display: flex;
        flex-direction: row;
        align-items: center;
        justify-content: space-between;
        font-size: 12px;
        line-height: 20px;
        font-weight: 600;
      }

      .description {
        font-size: 12px;
        line-height: 20px;
        font-weight: 300;
        opacity: 0.6;
      }
    }
    .item[selected] {
      color: var(--uc-blue-color-500);
    }
    .item:hover {
      background-color: var(--uc-background-color-300);
    }
  `;co([no(".selecter")],_e.prototype,"selecterEl",2);co([no(".list")],_e.prototype,"listEl",2);co([F({type:Boolean,reflect:!0})],_e.prototype,"open",2);co([F({type:String})],_e.prototype,"placeholder",2);co([F({type:Array})],_e.prototype,"models",2);co([F({type:Object})],_e.prototype,"selectedModel",2);_e=co([vt("uc-model-select")],_e);const Al=(e,t)=>{if(!e)return"";const o=new Date,i=navigator.language,s={year:"numeric",month:"2-digit",day:"2-digit",hour:"2-digit",minute:"2-digit"};let r=new Date(e);const a=e.endsWith("Z")?0:r.getTimezoneOffset()*60*1e3;return r=new Date(r.getTime()-a),r.getFullYear()===o.getFullYear()&&r.getMonth()===o.getMonth()&&r.getDate()===o.getDate()?new Intl.DateTimeFormat(i,{hour:"2-digit",minute:"2-digit"}).format(r):new Intl.DateTimeFormat(i,{...s,...t}).format(r)},Yh=new Map([["arrow-up",`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16">
      <path stroke="currentColor" stroke-width="1.5" d="M8 15a.5.5 0 0 0 .5-.5V2.707l3.146 3.147a.5.5 0 0 0 .708-.708l-4-4a.5.5 0 0 0-.708 0l-4 4a.5.5 0 1 0 .708.708L7.5 2.707V14.5a.5.5 0 0 0 .5.5"/>
    </svg>`],["arrow-down",`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16">
      <path stroke="currentColor" stroke-width="1.5" d="M8 1a.5.5 0 0 1 .5.5v11.793l3.146-3.147a.5.5 0 0 1 .708.708l-4 4a.5.5 0 0 1-.708 0l-4-4a.5.5 0 0 1 .708-.708L7.5 13.293V1.5A.5.5 0 0 1 8 1"/>
    </svg>`],["chevron-up",`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16">
      <path fill-rule="evenodd" d="M7.646 4.646a.5.5 0 0 1 .708 0l6 6a.5.5 0 0 1-.708.708L8 5.707l-5.646 5.647a.5.5 0 0 1-.708-.708z"></path>
    </svg>`],["chevron-down",`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16">
      <path fill-rule="evenodd" d="M1.646 4.646a.5.5 0 0 1 .708 0L8 10.293l5.646-5.647a.5.5 0 0 1 .708.708l-6 6a.5.5 0 0 1-.708 0l-6-6a.5.5 0 0 1 0-.708"></path>
    </svg>`],["square-fill",`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100">
      <rect fill="currentColor" x="10" y="10" width="80" height="80" rx="10" ry="10" />
    </svg>`],["plus",`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16">
      <path d="M8 4a.5.5 0 0 1 .5.5v3h3a.5.5 0 0 1 0 1h-3v3a.5.5 0 0 1-1 0v-3h-3a.5.5 0 0 1 0-1h3v-3A.5.5 0 0 1 8 4"/>
    </svg>`],["minus",`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16">
      <path d="M4 8a.5.5 0 0 1 .5-.5h7a.5.5 0 0 1 0 1h-7A.5.5 0 0 1 4 8z"/>
    </svg>`],["clear",`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 5120 5120">
      <path d="M2745 4480 c-66 -14 -320 -116 -481 -194 -302 -147 -635 -356 -949 -600 -219 -169 -466 -403 -496 -471 -47 -103 2 -244 102 -295 21 -11 83 -29 136 -40 123 -26 164 -38 293 -89 128 -50 291 -138 401 -217 100 -72 341 -303 497 -478 63 -70 140 -148 171 -172 123 -97 288 -161 446 -173 l79 -6 261 -505 c280 -542 301 -574 406 -637 128 -77 294 -82 428 -14 156 80 255 276 230 455 -9 71 -33 120 -337 705 l-221 424 35 68 c76 151 99 341 60 493 -31 119 -192 551 -296 791 -136 318 -394 777 -488 870 -69 69 -185 104 -277 85z m120 -297 c67 -92 197 -320 293 -513 88 -175 238 -527 230 -536 -2 -1 -224 -118 -493 -259 -270 -142 -533 -280 -586 -308 l-97 -52 -83 76 c-189 170 -293 244 -479 342 -167 88 -397 174 -522 193 -21 4 -38 10 -38 14 0 19 305 290 414 367 l34 25 38 -20 c22 -11 65 -40 98 -65 88 -69 145 -73 204 -16 74 73 48 151 -82 245 l-28 21 128 85 c143 95 302 188 320 188 22 0 148 -146 214 -248 74 -115 121 -150 185 -138 67 13 119 95 100 158 -12 38 -92 156 -172 255 -41 51 -72 97 -69 102 5 7 202 88 291 119 52 18 64 14 100 -35z m661 -1412 c44 -127 52 -211 30 -305 -39 -164 -122 -257 -323 -359 -172 -87 -219 -100 -347 -95 -117 5 -183 27 -279 91 -55 38 -207 195 -207 215 0 12 1074 572 1082 564 4 -4 24 -54 44 -111z m245 -1268 c235 -451 244 -470 244 -524 -1 -149 -164 -231 -286 -143 -37 26 -66 76 -284 499 l-243 470 140 72 c78 40 146 77 152 83 5 5 15 10 22 10 6 0 121 -210 255 -467z"></path><path id="pbPy4erNg" d="M622 2609 c-25 -12 -55 -39 -70 -62 -23 -34 -27 -52 -27 -108 0 -60 3 -71 31 -106 45 -55 88 -77 150 -77 138 2 224 144 164 269 -45 92 -157 130 -248 84z"></path><path id="ptfvIiWky" d="M1155 2021 c-48 -22 -79 -54 -100 -103 -28 -69 -12 -144 44 -199 166 -166 424 73 274 252 -54 64 -143 85 -218 50z" />
    </svg>`],["warning",`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16">
      <path d="M7.938 2.016A.13.13 0 0 1 8.002 2a.13.13 0 0 1 .063.016.15.15 0 0 1 .054.057l6.857 11.667c.036.06.035.124.002.183a.2.2 0 0 1-.054.06.1.1 0 0 1-.066.017H1.146a.1.1 0 0 1-.066-.017.2.2 0 0 1-.054-.06.18.18 0 0 1 .002-.183L7.884 2.073a.15.15 0 0 1 .054-.057m1.044-.45a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767z"/>
      <path d="M7.002 12a1 1 0 1 1 2 0 1 1 0 0 1-2 0M7.1 5.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0z"/>
    </svg>`],["danger",`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16">
      <path d="M4.54.146A.5.5 0 0 1 4.893 0h6.214a.5.5 0 0 1 .353.146l4.394 4.394a.5.5 0 0 1 .146.353v6.214a.5.5 0 0 1-.146.353l-4.394 4.394a.5.5 0 0 1-.353.146H4.893a.5.5 0 0 1-.353-.146L.146 11.46A.5.5 0 0 1 0 11.107V4.893a.5.5 0 0 1 .146-.353zM5.1 1 1 5.1v5.8L5.1 15h5.8l4.1-4.1V5.1L10.9 1z"/>
      <path d="M7.002 11a1 1 0 1 1 2 0 1 1 0 0 1-2 0M7.1 4.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0z"/>
    </svg>`],["info",`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16">
      <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14m0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16"/>
      <path d="m8.93 6.588-2.29.287-.082.38.45.083c.294.07.352.176.288.469l-.738 3.468c-.194.897.105 1.319.808 1.319.545 0 1.178-.252 1.465-.598l.088-.416c-.2.176-.492.246-.686.246-.275 0-.375-.193-.304-.533zM9 4.5a1 1 0 1 1-2 0 1 1 0 0 1 2 0"/>
    </svg>`],["success",`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16">
      <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14m0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16"/>
      <path d="m10.97 4.97-.02.022-3.473 4.425-2.093-2.094a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-1.071-1.05"/>
    </svg>`],["x",`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16">
      <path d="M2.146 2.854a.5.5 0 1 1 .708-.708L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8z"/>
    </svg>`],["check",`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16">
      <path d="M12.736 3.97a.733.733 0 0 1 1.047 0c.286.289.29.756.01 1.05L7.88 12.01a.733.733 0 0 1-1.065.02L3.217 8.384a.757.757 0 0 1 0-1.06.733.733 0 0 1 1.047 0l3.052 3.093 5.4-6.425z"/>
    </svg>`]]),Xh=new Map([["blank-avatar","data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAJQAAACTCAYAAABoOTBsAAAAAXNSR0IArs4c6QAAIABJREFUeF7tfQl8G1e198xo3xdLsuzIm+x4j2M7TuKmTVNKKftjKS1Q+EJZCg94pUDZ+gHtgwKv5VEK7aPlsX2FslP4WgqlUJo2bfbEtRMv8W7LtixL8iJrX0Yz73dkHb+Ja1uStVhOrd+vtaKZuffcc/9z7rlnuySx/dnmQAY5QGawre2mtjlAbANqGwQZ5cA2oDLKzu3GtgG1jYGMcmAbUHF2sixLut1uDU3TRpqmBcFgsCQSieh4PJ6IYRgeRVEChmGkJEm6WZZlKIpiGIbxSqVSF0VRk1KpNELTtE2j0bgyOkNbrLFXJaBYlhXPzc2VBwKBuqlJy7uidHRnlInqGYZREAQhJwlCwLAsxbLs8nRyv8OPJLnEOoqiCEAWwzARlmW9fD5/USqRziuUin9KpNLR4mLdEZFIZSFJkt5i2NgQuZc9oFiWpVwuV9PCwkKb3+/fHQgEKkKhUFMkEtEyDCMT8CkiGo3G/kOgIFgARACY9T4Mw8TAhffBv/E3+D0ajXpFQuGcWCIZl8mlFxVS+YtShapTp9P1b2jG8vyhyxZQMzMz+21W622LrsW9LEGUEAQhgblYKWkIksnqFAGooE8uSEmSDEjEklGZQv7n0tLyn6lUquGsEpHDxi8bQLEsK3I6nfvnZmffvjC38IZwOFQZZRjhSgmzGYDiSizoH6Qh0CUQCAg6wgTlMnm3WqN6SqvTPa7T6S7mcP4z3tWWBxTrcmmGnfYvW6emrg+Hw3VCoZAPk0XTdGzi+Hz+mnpQxrm5RoMAKPjweLyYpAK6ENjwGy6TPB4vIJXJXtbrdQ+bzTt/S5JZFp9ZYMCWBZTT6ayx2Wx3eNyut0QikSKUAvAXJgwmSiQSEZFIJMa2V0imLDBztSa5AAKaAOxIC17D34F2oBd+Vyjkg0qV+uclWt1P5YWF9hyRm3Y3Ww5QDoejeXJy8j/cbnc7j8dTh0OBZR0FpFF81xWTUPCfUChMm0npNIA61GqARmUeJRj8G18IGAfQTpLklK6g4FmDsfiugoKCqXRoycWzWwZQLper0mq1ftlut98QDoeV+FZT5NLWHiYCAAQf0E1wqcPfcsHM1fpAhZyrlHMlFNCN9wDdKLVw1wlSNhgMgsS1GwoLHy4rq3hEoVA4N2s8ifrNe0AtLi5qbTbbF61W6800TZvib20MQLFtPbm0g+JOEk4KgImrQyViRjaurzQrrOwDl0BU1lFaAd3w0oRCodgY8D6hQHhRp9P+orpW9X2SLAlkg+Z02sxrQE1NTd00MDDwNYIgamGQABQEz/KbTTAxZnN/h3vhN5gQfNPTYVI6zyIQkF4cB7a5UoKtHCOACfUqGA+8JPBMQUHBCVNh0ScNJlNXOvRl+tm8BJRrZqbCYp++z2F3voXHo2L2o+3Pqhzw6A2Gx4zG4m/q9frpfOBR3gFqeLD/C7aZmU+Gw+FS1DvygVH5SgOPzwfpPFJaWv6p8vLypzebzrwBlNfrNY4MDz4wMzNzE0VRFOhK4XA4tmxtf9bmQDAUiplHeDzKX1Cge7ixselukiT9m8WzvADU6PDwYev01BcikUgDMAJ9aKgzbBZztkK/VNwwCroi6GlqtepEWZn5X41GY/dm0L/pgBodHfrk1OTU3V6vVy+VSpd3NsCczd6hbcaEpNonE4+IAF7BiwhSXalUDptMpbeVlJQ8k2p76d6/aYBiWVbQ3d39qMNuu4HP54tgNwS7GXSXgHTaXvIST28oHCYkEsmyGQX5JhSKF4uKir5XU1Pz74lbydwdmwIot9uts1gsP56cnHy7VCKKAQcABW8Zim74jlvmzA338msJjKF+vz9mzAUwBQKB2HfQQeH3murah3gCwRdLSnJjs8o5oKxWa83ExMQvPB7PPhg4wf5vHBKCCX4H8Q1Gvc12neQ7BIFP6GDG78hH4F3AHyJKTKbf642FHzYYDN5sjyengLLb7ZWjo6O/d7vdrQgakshuPBI6ioHpwHCu3wzNEujtB2Zzw11wglbzw8Fv+B/uRJet9xR1ScAdbjTQaIn/5sZJbXyikX9rBwICD7Rq3V/KKyveV1BQ4N54X4mfzBmgpqen94yOjj4aCoUaY3FANB3TmSBiMpsf6AsYihZmLmjQ3YFLBFxDumDyEYQIqJXuHa7LB8HBjdzkun644OHa19KPglgfUABykFQsQxIyufzJqp1VH1Sr1QvZ4nlOAOVwOHYODw8/4XK56sViccxNEgMTLG1MdkOtUYogkDAmaVlqUEuuDQQc+tBQmuHkrwUC8CXiEgNg4rqBVps0rqsFowvSm9y1JPzSi4qxYThepUp1bN++9teSJBlOr9/Vn846oGZnZ2sHBwd/43a7m8EsAMwH7zlMHLw5UTor41oeLdc5ixPIXfaiDLHsfMXQEXg4HlriJ0nSQ5LkDGS/kCQJXn5IdzGGw2FpNBpVC/iUBMNQuE5paAOXwtUiDbCP9Cd1fUABn0FRR6c66KXGQsOTTaUVh8ksLH9ZBZTH4zF0d3f/1efztQFTubs2/M7nZZWESxRWbmwSgoCOLiUioCQTCAQzSqXyeZ1O9wtIkdJqtZ0EQcSi9DCCEhIfAC+wsZictNzocS82uN2e3dFoVM8NnVmZsIDg4S5z6buX1gcULnkggXEZB1Bptdr/v6dt3zvTB/SlLWRtNiHGu7Oz8w92u/2tMpkstqTAQJbcBLzlSEqMZ8r0wLA9FPkwibiccZVwlqAIqVTap1QqX1Kr1X8rLi7+J0mSvlTpYVmPwWZzXW+3z7x+0eV+K03TKm50JkqkldIq24DC9mFZx1XB5/PFvpeYTN/eWVN3ZyZDjbMCKJZleRcvXvy11Wq9CQYEby0MCCYSAIVKcux7lpc8XObQnYM7MQC1QCAYKSs3PyAUCh81Go0pg2gt0AVdLrPFbv3M1KT1FoFAIEcdi7OULu84sweoJer4/CV7FAAI5gFtfvE5CZeVmz9XVVX1UKov0Fr3ZwVQIyMjnx0fH7+PYRg+SiMYALyx8ejDZZDxsrvJi417ZYyRWCx2FBmND+oLi36sUCgcmWLmynYWFhaa+y/2PkDT9DX4Qq2M3UqU95eYtkRml6VYMVzuAdy43MdeMp5gura29g2Z8v1lHFCjo8MfHR8buw/ivbl2n8SM2dgdqAjjRHGV8Fh2CfG/iAVwK5XKf5pMprsLCwtPbKzH1J6Cpd9isXxkfHz83nA4LAepDFICl2Aw7HKXQWgdl0pMt0qtx9TuBjrEYnFn0+6W12citDijgHI4HMahwYHnw+FQLdfIl9oQU7+ba+OBfrngAkDFoxaiOp3uT83NzR+ARMvUe0nvCavVet3Y2NhjPp/PCPTArgv0SiYaucQIitIDekvfRpWYZgAt7L6FIuETbW37b0w3ZT6jgLrQ1fk724ztJtSTUG9JPKyN38EFE04A17gIZgGJRBLQ6/XfUqvVD2RSV0qVajChjIyMPL2wsFABPAJAYUw8Agh5tlJqpdpXKvdDn0CLubLyzp07a+5N5dmV92YMUENDA3daJye/QZAkhes015KcDpHrPQsSiWvRvkQ6QWgHyWNMJtM9ufa6r0Wz2+2u7enpedbr9ZpiRlZiqTYC16SRGZdMchwHGkCvBYAzDGOvb6h9m9FYcjq5p195V0YANTs9Xdc3NPhCKBQ0AGFgHshVpCV39wbDQ0cphsEYCot+X19ff5gkydBGmZTp5xwOR4vFYnnS4/GUgKcAAYX9cA2vme57tfa4OptEKnnuwIGDbyFJMriRvtMGFMuy/NOnTvzR5/P9C2xNAUyxKIJ4rlz62+LEwwKGYHo3RnkCLSqV6m8trW3AnERbocSdZPgOl8vVevbs2eMUyYq5SzVXUkGX2eYfgBfNOijpi4uKvlFb3/jVjQw5bUBNTVkODw4M/gTyK1cD1EaISuUZjAjgluOB7yqVarK0rOKQ0WgcS6W9XN7b09Pz3/Nzzo9iHQb0BeZCGcdxcsEMfIs702fr6huvKSoq6k2VH2kBCgLlzpw+eVoikZi9Xu+yJRakVPr2leSGgqHCuEzElVu6eEfxV+vqGtNSMJOjYON3sSwrOXPq5NFQOLQXc+9yqYxzKcdlD70ZcoXiqba2fe8gSXIpYC3JT1qA6urq+sn8nPPDSfaVldswdBh1N/i3Xq//566m5utJMp6nnpWeM9PogtN5qKPr5Wf4fL4YIzDQloZ/M9PT6q1w3WAYowZ2MoVCwewwFr+j1Gz+cyr9bxhQEJIyNDR0IhT061LpMBv34k4vrrf56uqrry4uLn85G31luk1wU/X2dD89M2O7HqU6BgVmuq9E7aH+hn4/mVx+Zv/+K64iSXKphE0Snw0Dqru7+5d2u/192XbuJhoDV9+I+6vOXHHgqtdsZm5aIppXXrfbp1/f1dn1DHczg/oUdylPtd1k7keVAb0aqJijTlphrvp4ZWXlD5NpC+7ZEKCmp6dbR0ZGXgwGg7Jc+OLWGwzX1gVMKSsp+VxVTd39yTIgH+6DnfKpk8eHfT5fGTecGE0g2aQRXUBYZxR9rhiRIZbI+hoaGg6qVKr5ZOjYEKA6OzvB2vvGmAEuyzHhiQaBJoP4X/sVB65qkclktkTP5dv1ycnxNw70DzyNlVZyZcvjLrNc1WHZvseSRElJye3V1dUPJsOzlAG1uLhY1dnZeZamaXWMmHjWSjKdZeMekFAYuy2Ty57dt++K67PRT7bb9Pl8RadOHu8lSVKDGw1M3sxm32iVx79cm14srY0hwNd3vr29HYIkE8Zrpwyonp6eHzscjo/AJOYiJjwZZqJDuLqm+q0mU9lfknkmH+85efLYyWAg2I7Ro9k2agIP0KPAtUdh/zEjcXQpTau8vPwj5eXlP03Et5QAxbKs6siRI/18Pt8IccqQcADe8s3+xN+u+ddce91OkiSTWus3m+bV+u/puXCfa2HhC7Bthw++KNmkFXjHzQzixkrFaGCXsn/EYvFLV1xxBSQ3rDvhKQGq+3zXd5yzzju41ulcvEWJGApvmVKpOr9vf3v7Rn1QifrIxfWJiYnPDA8Pfxf4KxQsZQFvdn0HjHgVCATRCnPVdSUlJS+su0lKllEQKHbm9MkOv9/fkAuDW7J04YBVKtXze9r2gTEz4TqfbNu5vm9qaurwwMDAz2NSg0/lRW0H5C9IKb2h8LsNDY13ZARQDofjqr6eC8+xBCFEQOUiPCXRpCINGo3mD80te969Fazja43Jbre/rre39x+4e85lGMtaNCGg4LpYLB44cOXBhvXcMUkveX09Fx6Yttk+jbYR3G7m0pG53qA1as3vmlv3vCcRAPP5+vz8fOP58+e7Y0oxQy+H4mw2zTjHLMvSDY31rysqWnvZSxpQL730wkU6QteiRRXW9mxbcZNhJNpLtFrtb3c3t743mWfy9R6Px1PX0dHRyzAMCYDCajSbSS9XSsJ3rUb9l5Y9+9665sudDLGLs7P7T3ecPSkWi0muVzwfpBMOWKPV/Kalpe3mZMaTr/fMzs5e293d/Vws4jVe3XizeYz8xd0fTdOO9t0tzTK9flXjcVISqru763H7jP0GXO7QQ50Lw1uyk6/RaH7d0tr2vmTvz8f77Hb79X19fX/HIiL5sPnhSiiYd7Dg19TWHC4rMz+2Gg8TAgp2d0eee3aYz+ebuEmCkDwIldM2uw44VylvaW27KR+BkixNNpvtjQMDA08Dn0VCfswDkA9mmZg9ihO7bzQaf1jfsOvjGwKUc3q6rW+w/wQcmwqKOBrb4C3C7JZkGZbN+/h8/umDV19zMJHhLZs0pNu23W4/3Nvb+3N4ScUiwXJSbLrtpvM8RiNwE0Qpkuo4eOgasPm9wkSTUEJZLKOHx0bHfs4N+uJmA+eDYg4M4/P5c7ubW6uT9Yqnw+RsPXvx4sUfTU1N3Yp2qM3Wn2CcK43YsYQKinQ0NbVUa7XaxZW8SAiovt7uh2dmZpbFGxexK4tBZIvRybQLIC8tK3l7RcXOJ5O5Px/vOXXq1NN+v/+NsVWAXPKhbbZKAXxCWxRnhaIry81vLjWb/5EyoM6cPtHj8XgbuCVv8mE7yx0Img5UauVPWlv33ZqPYElEE+iqR48enSAIwoB2qHwyHK+UVBqN5uctrW23pAQolmUVLx59fpqmaTlXGiFSEzEpV9dxORYIBZaDB6+p24xU83THOjo6euvo6OiPlnP02KVC9rlK9liLfm5aF1eQCEXCzquuOtSaEqBmZ2f3XjjfeYb70GZlZaw34FhKd/z08uqa6htKSsr/lO4E5/r5kydPDni93mroF5Y6WPLyRT9FXnDnniTJydY9e5tX6qzr6lCDg4NfmZq03MP153DDUvNFJGNQP0yAVqt9vKW17cZcAyKd/mZmZtp7enpOIj9jcd3xjOJ8UMy5Ugr1OojZr2/Yda3RaLwkbX1dQHV0dLyw6Jo/hLs6aJirKOaDUg40cRIUCYpH+dvb9+wSi9Wj6UxyLp/t6+t7bGJi4v1Ykx3GFAouFbPPBzsUAgr5jCvCzsqd7y8pL//VJSvYeox78egLAzQdiYnhfP+g7gGWXL1B/1RLS9vb8zEFfSUf/f65ku4LA52Li4sFYCgG+x4GvaEheTN5z93hcZzEMcGiVqt/1Nyy52NJAQoyMY4ePWJlooxhMweUqG8cMAwWlMZ4fDlrrjS/t7S04neJnt/s6+c7X/7r7Nzsm2JBdUJhzLUB3+PVUHJSIyoRD3AXzQUUrE4qperJlj1tkF28dPDzemlUHo9Hf/bMqSGCIFSJOsyH66jAwpsDb7ZKqeqtrW84mM0i7+mOe3R09LaR4cFYNgmWfeZWt8sH/QloWwmoGHBIklDIFUf27N33Bq53Yk0dan5+ZldXZzeUDZSny7hsP496BiwXsVMD4gW0DHrDn3e3tL4t2/1vpH04WWJ4ePgZOhLSrRZbBi8ITuRG2s/kM6sBCtqXK+Td1dV1h7gv7ZqAcjisLX09F48xLCvNJHGZbguVcgAShtbA0gdFtGCiysrK7ttZXfulTPebTnssy5JHjx4djkajZkiUhQlD2vGFwPqa+SClEFDcSF34LpXKhhp3NV3Jrc25JqCs1om3DQ4M/hFy79NhXrafBdBgxRD4DksGKuho3S0rL/9MRUXl97JNSzLtsywrP378OPD1+liVGnLpECI0YmJ9K1TM88EWxQUUNzFULBbPNO7afZVarR5JqEONj49+ZGx09Mf58IYkmiikEZVymAR40zmTwZaVln7DXFV9V6K2snkdJNOxY8eO+f3+AyiJuJnXuMHg6oP5wP+VgAI6YfMjFApdjbt2H9RqtT0JAQVHt46Pjf9XNhmcibZhaYDtNixx8MHaAFg7Et0FYNMpMZnuNldVfzPVmkeZoDMQCJSdPXv296FQaN9yBWCIMYoveSuNh1xjcib6T6eN1QAV11ddu5qar9ZqtcvnG6+55A0PD95mGR9/MB8Ma+kwA0GGJRMLCgr+WrXT9GG5vNCebrvJPj82PPwx67Tt85FIpBJ1vmWFO/+qNV4yLKCTq0LARZSaJEm6GhqbDhUWFl5ILKGGBv7vmMUCb3OyfMvL+7juIWSEVCodLzIWfqesouqRbBo/3W53wfTE1BcWFl2f9Xq9fKAFw6ZRZ2KJlArE5ZzHCChux5zfPLuamq/V6/XnXjWAwsIT8BcVXlDc4UCjAm3B48Um4/e02sLjmZwpOKB7aKj/dqdj9gPhcKiRexYfuqvyIV48mTFnDFBjw4OfGrNYvp9Mp/l8z0rHNobeYD0klmX9Go2mt6iw8EemsopH08k8djqdxY6Z6U/MLcy/JxKOVCJ4uFKea23OFzvTRuePIkl3w67dIKE6Ekuo4eHbxi1jSdUE2ihBuXiOW/MIox+xXgDqVagjCASClyUSyYhcoTinUaq75CpBdyQiCK5mbYfljCTDhqA3Yp53z185P+faFwgEWgmCjZXjAbCgLQkBhcZKblGxrapSxHnmqqtvTFKH2iK7vESgXGu3BL9zLdTc7Tlnkhei0aiHJKkARVGRuDpJMgwjYBhGwbKskqIoGdDAVVxxOeMmdUD7uHxstWVvNf0JAdW0uyW5Xd7o8PBhy8R4rHDDVv6sdB5zbT1rLTk4+fAX7VkoXYAXWP+Su3yiJORKIy7QkIdcfq6mn+Q7r7m7PpIk58BsUFBQ0JdwyRsbHn67ZWL8dwzLCvN9kOvRhzE8qJxjMB5Kp5WWaAQEShzMjYP78RoXcLiLXE1CcYLRlklcTVJtJf5yAUVRlG13cys44BNbyp1OZ2tvb+/zTDSizOcBrzQLoARYDwCXSA6SF7OoX5J3Fs8/5MZ0x42hcPgeyQUUAnOl4r0SONAnd7eHkhKrKK8E6/Ibv8lmG8xyilvGY24u3DEL+PyhpubW5Hx58/Pzuzo7O4+SBKPJZ0ABbVzdBMsigyUX/ovFZ8dNBlzdJb6ERViCWuDxeAsikSggkUi6JRLJqEgkGobdHkVRjEgkmiVJEoA0SlFUCUEQM5FIBEwBfDixlKIoQTAYLPF4PI2BQIAHxkuCIDQkSSpIklTTNB07AZKrs0Hfy9EEcV8eXoexIJDRlbSZ/Ad60NyCZ8Lgki8SiXrqG3Yd4saVr2m1hHio06dP91Akm9cBdlw9iCtp8Ixd3MnFJYaHzxd0S6TiGYVCPiMWS55Wqwv61Gp1Rs+DAYe6w+EoCwQCV/j9/jqfz9fu9XpLGYYBQMYOC0KQY9FbkADcg6bhHnAnYe3yzQQVgB1rqmL4L/wml8tfaNu7//UkSS7VcFwvwA6Y8vzzzw8RbLRiMweTqG9c8rhSCp4BIMUkAcsElErloF6vf0yr1f9do9EMchmQqP1MXYeUNDh40W63/x+Hw/EvgUCglCRJMQAKQIPJH9w4KO5ynik6NtoOFnfFHSzwe7WEkHX9KseOHTsZDgXaN0pELp7j2nRQwYYJEovFTqFA+FRVtflBmUw9lG8nK9jt9iudTuftQb/3wKLbvQOkExTBBckKlnwYCxZTzQUf1+sD9UFUJ+BeoK+ouPj+hoZdn+M+uy6gLly48PtZpz2vU5LgLUZpBAOWSKWjOp3u+waD8alML2XZmFiPx2NwzTlvmnHYDy8uuvfC5OEyhzpgNvpNpU3uRodr6TdXmm+vqKi6xPi9LqCmp6ev7r/Y+zyYXlIhIJf3sgwZU8rFYrGrUG/4VVVt9W1bsc4mxEqNDA18fdpm+zBN00W4BG62HRB3o9xdaHwDEYG8PIPBcCxpCQWnJnScO3OWIAh1LkGSSl8kwYvo9bpfF+3Y8W2ugS2VNvLpXp/PWTQxPvMl5+zs4XA4vHRaxSZ+VpplgBSgic/nQ7RmnUajcSUNKFDMXzz6PETj1YL9AdZ4jH3GNyfbvihYzvAEcfiOlut4utRk0+6WWwsKCv6+iTzPStdwQNPY2MhvfF5vNdbh4p56kCsdC/UnmHsoMoc6q0QiPnvgyqv3rRx8wmCn06dPPBEMBN8GE4j2CFTOcuE6QNGPFmu0SBv0hifNVabbpNKCyazMaB406nQ6FW7X/H9MTk5+PBQOUxCZisZRVNhzsSRCH7hZwHgunV7/jaam5lecS5wQUP19PffaZma+iMovNo7IzTbfoV/Y9WCaNrylO4qLH6qtb/x8Pp10nk0+jIyM3D41NXVPKBRSAJDQ3hYzMGb50FK0ma3Y6YV2Vu98s8lU9lzKEgqyXy72XXwCJQXXZ4XO0WwykzsQYGaBVnt/3Yqtajb7z5e2R0dH3z8xMfFdmqb1uFLElOMcAAr7Q3cWRVHWxl276wsKCtwpA8rtdtacPdN5gmVZLTfqETNLsl1hjauUFhmN91fX1t+5letopgPQiYmJvRaL5alAIFC4XN80y8fLoS8PY7vgr1wuP7W//cCVq4VPJ1zygAGnTh3v9Lg9zXg4IKzjuQIU9A9Saoep5MHa2rrb05mQy+HZ8fHxm0dGRn4Iy59UKs36aWAYloP6Msy9qaTkwbq6hlXnIilADQ323zs1NfVFroELvfPZniToU6/T/3LX7uaPvFp0pkQ8HRgYuGNqaupbJEkKc3EAJhdUMB+7mna92WAoeno1OpMClMMxWTXQP9IRDoeV0Dh3G5ttO4lYLDp7xYGDUMKYScToV9P1ycnJr128ePEuPi+pKdwwazAzezkxlSQnr9mzt5lc4wzipKmB4mNut/sQ1x4Vs0lkaA3Homa4LQV7l1KhGGhubXuTWCzeMsXDNjxzKT4IJoWBgYHnA37vHrARYWQC8A/sRXHjY9plFXGXF6GZmB1SoVD8uKWl5aNrkZs0oAYGBu60WCzfwm3rcvB/moDiRjxinFDMl8WyfnNF5TtLKyouO6NlithZ83aHw9Hc19v9bCgU0uFhTsBPeDmxrFG6Kwju6sORKLTpq6uru6a4uHg5Dy/lXR4+MDs7W9fV1XVcKBRquAUp0pVQaNfiHkoEwCreYXqkrq7+9lfrji5Z0A3293113GL5OnoTcLMEQACple6JoAhQAJRAIDh39dVXg/qxZnZq0hIKnJfnzp07tri4eAAGiyZ4KC6azodbmwDepljef5QePHTo2qZtJTwxZ1mXS3NucOA5t3uxhVvSCDdN6brGlg2bBEUYjca76uvr71mPqqQBBY1YrdYPnT9//qewluJ2nls9JPHwX3kHhphyYrqjpmLTh6pqan6xkfZejc9MTIy9p/9i/69EIhGFfMQIy5VJGBvhT8xvKhB5WlpartZoNF0ZAxSkWB89erSLZdl6DK2F6iHpfHCtR9FMUlTHoUOvObAZUZXpjGMzn4V6qB3nzvzV4/Fcj8kEmTI44y5Ppda+0N7e/ppE40xJQkFjw8PD/zY1NfUQEA6fdE3/3HgbgUAQLiouOlxVVZP3xVYTMTbX151Oa81A//CJYDCozWRmctzdsli1s+ZNO3bsgBKZ635SBpTP5ys6d+7ciWg0Wr607qVXPQSACRZfUCBlMtkL7Vdm+ZCIAAAM9ElEQVRcee1WDJBLxOhsX2dZVnj29Im/+PyB18VDe5YP/Um3b6VS+UTb3v3vSKadlAEFjXZ2dv4/l8t1C+z24Dh4TE+KSax4Xj833GQ9QjDkNRwOhxt37T5kNBpPJUP49j2v5MD09OQ7+y/2Pw4bKEzLSkaHwmUNdGP4Di83higxDEM3NTbdWFhc/EQyPN8QoObm5kx9fX0gXktAh8Kog2XbFKcoVSIi0A4lk8n6zJU72/V6vSfRM9vXV+cAy7KS850dRxZcrnYEUqJdHvc6pktB67hj1KjVx1sK9NeRFRVLJQIzveRhe319fV+x2Wz3wC4PQYEpNmhMSyb4C1PEK6uqPm82V30nEcHb19fnAOz4RkdGf4N2vUSGTQQRAAgUeSzNDc/RNO1trGt4Z+GOHc8my/cNSSho3Ov1Fl64cOFIwO+tx6UO/nJBlYy4jRPqaNu7v16pVM4lS/j2fatzwO12Vp8/3/dcOBQyYQzberxa6avDuYS5U8gVT7Tt2//OVHTaDQMKOh4ZGbnLMj76NVQCsTgEEpVMRAJIMZVK9Xjb3v15na61lQDc1dXxm4X5hfdgYFwiQGEBEbQJxnRjgWCurqr6OoPJtK7daWXbaQGKZVnpubOn/7G4uHglKteY+bpa5ZHVBgYi11xuvr7MbE5arG6lyd0MWsfGhj82Njr2w2T65uq9uKIAwPSGwkdaWlo/kUwb3HvSAhQ0ZLNZ6i3jUy95PB4tRgqstH6vR5RYLJ7a1dTcyq3Gn+ogtu+/lAOLi46qjnPnOxiGgYJo67KHm0kD8wagUipV5yvMlVcZDAZvqrxNG1DQYXf3+e847PY7VpoPklnyFArFn/bua78hVcK371+fA8deOnqWpum2RHosRhOg7kuSpL20rPRWs3nnUxvhcUYA5fV6jR0dHUeg0ohELIzt+rCSCIakYAmYaDgUuw7e8RAdJSrM5k9UVFQ+shHit59ZmwMTExPv7e/v/zXYCTHRY2UuZezf5NLJKxAyBPYnjUbzk7179340FUU8o0seNma326/v7e39LcFGY0VL4YP+JK7kIqJxlw1FEREmajvQcmiPTC+zbYMjsxyYn59vunDhwj+idLgQIwZwXlBqwb+jzJIdMV7p71xjY+M7CgoKpjZKTUYkFHbe39//jQnL2JcB7fg2YNYESio+uZR0EBukUHD+mmuua94o8dvPrc0BlmWpM2fOvOTzug9ggsFKL0bMGk4sSTCCIKYrKytvLC8vT+ivW4/vGQUUWGo7O889OeucfR0QD8DCjGOMThDxebHfYNnTGYvubWxsunMbGNnhwPnz5x+Yn3N+GlpfLTE3ZiUnYwdWMmVlZd+rq6u7I11KMgooIGZhYaG8p6fn8Ug4uGdl1ilcx5RmoVA4XVffeI3BYIBTQ7c/WeDA0NDQZycsY/ejZIJVAmPNYW5AzwVAKRSKJ/ft23dzJmpoZRxQwBer1XpgZHjoD8FgoBhPXsJBwV8wnCnk8o4rrjx4kCTJQBZ4ud0kFAOdmdnf033+CI/Hix2iuTINDlYKiVR+tq6u7rWZ8qFmBVBA/ODg4C02m+2/mGhEhkFfGEMF11VK5aNt+9o/uD3z2eMA1El9ueNsRzQahdqey+EsaBkXCoUWU0nZByoqKo5mioqsAQoI7Ozs/Nb8nPNOeDMgNIJz4jcrVyjub2nZ8/lMDWS7ndU5cPzYi13BYHA3JoNgoQ2ZTL5YoNF+rKa+PqPBjFkFFCjpHR0dD7vd7lsgKgG3qyzLLpaUln6lpqYu7w943OpAPXH82F+8Xs+b4fQtUDXA1iSXy/2GwsJ7amrq7s30+LIKqPi6LTp27NgvCTb6LhgQbFVJkhwzl1V8oqSi4plMD2i7vUs5MDjYf59tevoLgUAgtkpEo1HaaDR+pyFLu+usAyoOKvXx48chDv09dCTE5wsEvU1NtW/SaosntgGQXQ5MT09cPzgw9CRN02LYxRUVFz9UV9cAFWyycohPTgAFLHO73bquri5Q0q+N0JF5c5n5e5XV1Ul5xLPL8su3dVDKh4cGfuDxeN8cCgX9ppKSn9TW1t+dzYyinAEqLqnkp06d+s9AIHAVHBtWXl7+wM6dO/9w+U7p5o0MylUPDAz8aGF+tpkiqYixuOh39fWNd2W76EhOAYWgOnfu3L0ej6c9HA4HTCbTCw0NDV/fTjnPHPgmJydvmJqaumVhYaFGqZAFtAXan9XVNT6UbTDFTBOZG0ZqLb388sufDQQC71hcXJTo9XpnSUnJ541GI1Qc3v5skANQtXlycvLG4eHhLwaDQZlSqXQUFRm+W1lZ/acNNpnyY5sGKKB0YGDg3Tab7WPhcNgkEokcxcXF/63T6Z5aWfs65VG9Ch+AYiaTk5OfnpubOxSNRimVSjVoMBi+bTabX8wlOzYVUDDQ6enp2rGxsfui0Wit3+8PSySSgebm5o9yj8zKJUO2Yl89PT23wrkxDMMURiIRl1arPVZRUfGpTLlTUuHJpgMKiLXb7YVWq/XT8/PzbxMKhSKBQNAnFAqfKS8vf2y1SrOpDPByvndubq5hdHT037xe71ui0aiaz+ePazSaPzY1Nd1PkuSm5DfmBaBw0ru7u99vs9k+zOPxCuEATIlEMlxeXv7N4uLi5WPcL2eApDK2/v7+b8/MzBxiGGZHIBBg1Wr1xcrKyrs2O/M6rwAFDLXZbPUWi+WTkEnD5/M1QqFwQS6X/7WkpOSXOp3uYipMv9zuhTP3RkdH91mt1n/3+/0lAoFAAGeuaDSao8XFxffrdDrrZo857wAVNy2Qw8PD75uenr7Z7/fX8Hg8IUmSVr1e/4LZbP7PV1tCKERfWq3WN01OTt7q9XprBQIB1IyfD4VC48CPysrKf242kLD/vAQUEjc1NWWy2+2f8ng8r41Go5UURYUZhrGr1epnd+zY8bPCwsK+XNhWNmuyWJaVWyyWfU6n84M+n+9qiqIMkUgkSNP0QGFh4RO7d+9+MBNBcZkcX14DCgc6PT29Z2Ji4lMul6uVz+eb+Hw+Q1HUiFQqPVteXv6oTqeDI9gum0/s0CC3+0Ozs7MHgsHgLoIgjPHQn6mCgoIjZWVlD+v1+sF8HPCWABQyzmKxvM5qtd7i8XgOwNvK4/GCgUBgRqvVThcVFT0ik8me1el03mw5PrM5gVAd0OFw1Ntstpvn5+ffQNN0MUVRcLJ6iKZpi1wuv1BWVvYDk8l0Mpt0pNv2lgJU3G4lDQQC1zidzne5XK5mHo8HjKdIknTyeLwxqVQ6rtfr/6bX61+WyfI7PYtlWeXs7Kxxbm7u3fPz83Du8CGWZQtpmoYDL0M8Hm9crVY/p1ar/1BeXn4ym07ddIG0JXSoRIN0u901Fy5c+FI0Gm1kWRaOsVeyLBsCPYtl2VmSJGebmprulclko3K53J6ovVxcBwXbbrfXz8zMvM/pdO6PRqN6Pp9vEAgEQDsVCoUg/dum1Wp7CgsLf1BaWvpSLujKVB9bTkKtNnA4/dLhcBzw+XzXRSKRcoZhtFDFjcfjeRiGmWMYxicUCifkcvm8RqPpVCgUoxKJxCqVShdJkvRlipkr23G73QVwihfszBYWFgp8Pl9rKBTSRKPRMoZh9HBNLBaLgsFghGXZOZlM1qvT6f6h0+mOA6CyRVc2270sAMVl0OTkZJXNZvtXj8fTFI3GsphBD5FTFMUHY2k4HA6yLBsQiUQBiUQSIAgmIJfLT8vlsvMikXBKIlHQKpWqK5XdI8uymtnZ2Qqv12sIBAJtgUBAEQgEGsPhsIimaehfDaYPHo/HZxiGpGmaIUkyyOfzfaFQaMFgMPTt2LHjp3q9/sJ6ReWzCYRMtX3ZAQoZA8Fl8/PzzYuLi61+v785EAiUkySpJUlSARIsEolQDMMAoIICgdDP41Gg/IYJgg2TJOVnWTYSiYRZpVI9C5naBEEFKIqM5UsyDKNhWWaeZZmSaJSAdkRgdITyRiRJSuBcAIIgxKBo8/l8eTQahWfmCYJw8Xg8h0gkAt3oZbVa3QFS6XLyW162gOK+cSzLin0+n8rlcjXMzs5Wezye14fD4QKCIGQEwYh5PErOMCyctCUiSZLi8wXkEnhYKpYMuZSCBCGzsbBZuMCyLBP7H0PAhoCKn69CLoGUcDMMA7oQ6HN+iUTiUKlU3Tqd7oRSqeySyWTufLMfbUuoNDkAEszj8ZRBaUeaDreEQiEtHN9G01EdACsapdXRaBTqKwngPB6GYeFEIwIkFByDTJJUhCSJgFAo8ZEkuciyLA3Wa4FA4JbJZOMymWwI3EYGg2GEJElHmuRumcdfFRIqmdmA0wgIghD5/X4lSQZ5gQChgiUMhBNN0yRFMQKo9c/nQx1RKsLn81k+n40yDC8EEkksFjMSiQTO4A28mqNPtwGVDNq270maA9uASppV2zcmw4FtQCXDpe17kubA/wABdFQLi5zmnQAAAABJRU5ErkJggg=="]]);var Qh=Object.defineProperty,Zh=Object.getOwnPropertyDescriptor,oa=(e,t,o,i)=>{for(var s=i>1?void 0:i?Zh(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&Qh(t,o,s),s};let xi=class extends nt{updated(e){super.updated(e),e.has("name")&&this.name&&(this.data=Yh.get(this.name)||"")}render(){var t;const e=(t=this.data)==null?void 0:t.trim();return e!=null&&e.startsWith("<svg")?Qn(e):U}};xi.styles=bt`
    :host {
      display: inline-flex;
      font-size: 16px;
      color: inherit;
    }

    svg {
      width: 1em;
      height: 1em;
      fill: currentColor;
    }
  `;oa([Yn()],xi.prototype,"data",2);oa([F({type:String})],xi.prototype,"name",2);xi=oa([vt("uc-icon")],xi);var Gh=Object.getOwnPropertyDescriptor,Jh=(e,t,o,i)=>{for(var s=i>1?void 0:i?Gh(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=a(s)||s);return s};let ur=class extends nt{render(){return B`
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
        <g class="spinner">
          <rect x="11" y="1" width="2" height="5" opacity=".14"/>
          <rect x="11" y="1" width="2" height="5" transform="rotate(30 12 12)" opacity=".29"/>
          <rect x="11" y="1" width="2" height="5" transform="rotate(60 12 12)" opacity=".43"/>
          <rect x="11" y="1" width="2" height="5" transform="rotate(90 12 12)" opacity=".57"/>
          <rect x="11" y="1" width="2" height="5" transform="rotate(120 12 12)" opacity=".71"/>
          <rect x="11" y="1" width="2" height="5" transform="rotate(150 12 12)" opacity=".86"/>
          <rect x="11" y="1" width="2" height="5" transform="rotate(180 12 12)"/>
        </g>
      </svg>
    `}};ur.styles=bt`
    :host {
      display: inline-flex;
    }

    svg {
      width: 1em;
      height: 1em;
      fill: currentColor;
    }

    .spinner {
      transform-origin:center;
      animation:spinner_action .75s step-end infinite
    }
    
    @keyframes spinner_action {
      8.3%{transform:rotate(30deg)}
      16.6%{transform:rotate(60deg)}
      25%{transform:rotate(90deg)}
      33.3%{transform:rotate(120deg)}
      41.6%{transform:rotate(150deg)}
      50%{transform:rotate(180deg)}
      58.3%{transform:rotate(210deg)}
      66.6%{transform:rotate(240deg)}
      75%{transform:rotate(270deg)}
      83.3%{transform:rotate(300deg)}
      91.6%{transform:rotate(330deg)}
      100%{transform:rotate(360deg)}
    }
  `;ur=Jh([vt("uc-spinner")],ur);var tu=Object.getOwnPropertyDescriptor,eu=(e,t,o,i)=>{for(var s=i>1?void 0:i?tu(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=a(s)||s);return s};let pr=class extends nt{render(){return B`
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
        <rect class="bounce-bar" x="1" y="6" width="2.8" height="12" />
        <rect class="bounce-bar delay-1" x="5.8" y="6" width="2.8" height="12" />
        <rect class="bounce-bar delay-2" x="10.6" y="6" width="2.8" height="12" />
        <rect class="bounce-bar delay-3" x="15.4" y="6" width="2.8" height="12" />
        <rect class="bounce-bar delay-4" x="20.2" y="6" width="2.8" height="12" />
      </svg>
    `}};pr.styles=bt`
    :host {
      display: inline-flex;
      font-size: 32px;
      color: inherit;
    }

    svg {
      width: 1em;
      height: 1em;
      fill: currentColor;
    }

    .bounce-bar {
      animation: bounce 0.9s linear infinite;
      animation-delay: -0.9s;
    }
    .delay-1 {
      animation-delay: -0.8s;
    }
    .delay-2 {
      animation-delay: -0.7s;
    }
    .delay-3 {
      animation-delay: -0.6s;
    }
    .delay-4 {
      animation-delay: -0.5s;
    }

    @keyframes bounce {
      0%, 66.66% {
        animation-timing-function: cubic-bezier(0.36, 0.61, 0.3, 0.98);
        y: 6px;
        height: 12px;
      }
      33.33% {
        animation-timing-function: cubic-bezier(0.36, 0.61, 0.3, 0.98);
        y: 1px;
        height: 22px;
      }
    }
  `;pr=eu([vt("uc-bar-loader")],pr);var ou=Object.getOwnPropertyDescriptor,iu=(e,t,o,i)=>{for(var s=i>1?void 0:i?ou(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=a(s)||s);return s};let fr=class extends nt{render(){return B`
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
        <circle class="pulser" cx="12" cy="12" r="0"/>
      </svg>
    `}};fr.styles=bt`
    :host {
      display: inline-flex;
      font-size: 32px;
      color: inherit;
    }

    svg {
      width: 1em;
      height: 1em;
      fill: currentColor;
    }

    .pulser {
      animation: pulse 2s cubic-bezier(0.52,.6,.25,.99) infinite;
    }
      
    @keyframes pulse {
      0% {
        r: 0;
        opacity: 1;
      }
      100% {
        r: 12px;
        opacity: 0;
      }
    }
  `;fr=iu([vt("uc-circle-pulser")],fr);var su=Object.defineProperty,ru=Object.getOwnPropertyDescriptor,ia=(e,t,o,i)=>{for(var s=i>1?void 0:i?ru(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&su(t,o,s),s};let ki=class extends nt{constructor(){super(...arguments),this.observer=new ResizeObserver(this.updateFillHeight.bind(this)),this.lastCount=0,this.messages=[],this.scrollToTop=async()=>{await this.updateComplete,this.messagesEl&&requestAnimationFrame(()=>{this.messagesEl.scrollTo({top:0,behavior:"smooth"})})},this.scrollToBottom=async()=>{await this.updateComplete,!(!this.messagesEl||this.messagesEl.scrollHeight===0)&&requestAnimationFrame(()=>{this.messagesEl.scrollTo({top:this.messagesEl.scrollHeight,behavior:"smooth"})})},this.contentChanged=e=>{var o;const t=e.target;if(t instanceof Ao){const i=this.messages[this.messages.length-1];if(i.role==="assistant"){const s=((o=t.value)==null?void 0:o.index)||0;i.content&&i.content[s]&&(i.content[s]=t.value,this.dispatchEvent(new CustomEvent("tool-change",{detail:this.messages})))}}}}firstUpdated(e){super.firstUpdated(e),this.observer.observe(this),this.updateFillHeight()}updated(e){if(super.updated(e),e.has("messages")&&this.messages){if(this.lastCount===this.messages.length)return;this.lastCount=this.messages.length,this.scrollToBottom()}}disconnectedCallback(){this.observer.disconnect(),super.disconnectedCallback()}render(){return B`
      <div class="scroller top" @click=${this.scrollToTop}>
        <uc-icon name="chevron-up"></uc-icon>
      </div>
      <div class="messages">
        ${Gi(this.messages,e=>e.timestamp,e=>e.role==="user"?B`
              <sent-message
                .timestamp=${e.timestamp}>
                ${e.content&&e.content.length>0?Gi(e.content,t=>t.index,t=>t.type==="text"?B`<text-block .value=${t.value}></text-block>`:U):U}
              </sent-message>`:B`
              <received-message
                .name=${e.name}
                .avatar=${e.avatar}
                .timestamp=${e.timestamp}>
                ${e.content&&e.content.length>0?Gi(e.content,t=>t.index,t=>{var i;const o=((i=e.content)==null?void 0:i.length)===(t.index||0)+1;return t.type==="thinking"?B`<thinking-block .value=${t.value} ?loading=${o}></thinking-block>`:t.type==="text"?B`<marked-block .value=${t.value}></marked-block>`:t.type==="tool"?B`<tool-block .value=${t} @tool-change=${this.contentChanged}></tool-block>`:U}):U}
              </received-message>`)}
      </div>
      <div class="scroller bottom" @click=${this.scrollToBottom}>
        <uc-icon name="chevron-down"></uc-icon>
      </div>
    `}updateFillHeight(){const e=this.getBoundingClientRect().height,t=getComputedStyle(this.messagesEl),o=getComputedStyle(this),i=t.getPropertyValue("padding-bottom"),s=t.getPropertyValue("padding-top"),r=o.getPropertyValue("--messages-gap"),a=e-(parseFloat(s)??0)-(parseFloat(r)??0)-(parseFloat(i)??0);this.style.setProperty("--fill-height",`${a}px`)}};ki.styles=bt`
    :host {
      position: relative;
      display: block;
      width: 100%;
      height: 100%;
      overflow: hidden;

      --messages-padding: 10px 20% 10px 20%;
      --messages-gap: 24px;
      --fill-height: 100%;
    }

    .scroller {
      position: absolute;
      display: flex;
      justify-content: center;
      align-items: center;
      width: 32px;
      height: 32px;
      right: 32px;
      border-radius: 50%;
      cursor: pointer;
      border: 1px solid var(--uc-border-color-mid);
    }
    .scroller.top {
      top: 16px;
    }
    .scroller.bottom {
      bottom: 16px;
    }

    .messages {
      display: block;
      width: 100%;
      height: 100%;
      padding: var(--messages-padding);
      box-sizing: border-box;
      overflow-y: auto;
    }
    .messages > *:not(:last-child) {
      margin-bottom: var(--messages-gap);
    }
    .messages > :last-child {
      min-height: var(--fill-height);
    }

    sent-message {
      width: auto;
      height: auto;
      align-self: flex-end;
    }
    
    received-message {
      width: 100%;
      height: auto;
      align-self: flex-start;
    }
  `;ia([no(".messages")],ki.prototype,"messagesEl",2);ia([F({type:Array})],ki.prototype,"messages",2);ki=ia([vt("message-box")],ki);/**
 * @license
 * Copyright 2018 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const an=e=>e??U;var au=Object.defineProperty,nu=Object.getOwnPropertyDescriptor,ho=(e,t,o,i)=>{for(var s=i>1?void 0:i?nu(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&au(t,o,s),s};let $e=class extends nt{constructor(){super(...arguments),this.disabled=!0,this.loading=!1,this.value="",this.handleInput=e=>{const t=e.target;this.value=t.value,this.disabled=!t.value.trim()},this.handleKeydown=e=>{if(e.key==="Enter"&&!e.shiftKey){if(e.preventDefault(),this.loading)return;this.dispatchSendEvent()}},this.dispatchSendEvent=()=>{if(this.loading)return;const e=this.value.trim();e&&(this.dispatchEvent(new CustomEvent("send",{bubbles:!0,composed:!0,detail:e})),this.value="",this.disabled=!this.value.trim())},this.dispatchStopEvent=()=>{this.dispatchEvent(new CustomEvent("stop",{bubbles:!0,composed:!0}))}}updated(e){super.updated(e),e.has("minRows")&&this.minRows&&this.style.setProperty("--min-rows",`${this.minRows}`),e.has("maxRows")&&this.maxRows&&this.style.setProperty("--max-rows",`${this.maxRows}`)}render(){return B`
      <div class="container">
        <!-- Input -->
        <div class="input-area">
          <textarea
            spellcheck="false"
            placeholder=${an(this.placeholder)}
            rows=${an(this.minRows)}
            .value=${this.value}
            @input=${this.handleInput}
            @keydown=${this.handleKeydown}
          ></textarea>
          <div class="filler">${this.value}</div>
        </div>

        <!-- Button Control -->
        <div class="control-area">
          <slot></slot>
          
          <uc-button class="send-btn"
            ?disabled=${this.loading?!1:this.disabled}
            @click=${this.loading?this.dispatchStopEvent:this.dispatchSendEvent}>
            <uc-icon
              name=${this.loading?"square-fill":"arrow-up"}
            ></uc-icon>
          </uc-button>
        </div>
      </div>
    `}};$e.styles=bt`
    :host {
      display: block;
      font-size: 16px;
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      
      line-height: 1.5;
      --min-rows: 2;
      --max-rows: 10;
    }

    .container {
      display: flex;
      flex-direction: column;
      gap: 4px;
      padding: 16px 16px;
      box-sizing: border-box;
      border: 1px solid var(--uc-border-color-low);
      border-radius: 16px;
      background-color: var(--uc-background-color-0);
      box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    }

    .input-area {
      position: relative;
      box-sizing: border-box;
      max-height: calc(var(--max-rows) * 1.5em);

      textarea {
        position: absolute;
        top: 0;
        left: 0;
        bottom: 0;
        right: 0;
        padding: 0;
        margin: 0;
        border: none;
        resize: none;
        outline: none;
        color: currentColor;
        background-color: transparent;
        font-size: inherit;
        line-height: inherit;
        font-family: inherit;
        overflow: auto;
      }

      .filler {
        min-height: calc(var(--min-rows) * 1.5em);
        display: block;
        visibility: hidden;
        pointer-events: none;
        font-size: inherit;
        line-height: inherit;
        word-break: break-word;
        white-space: pre-wrap;
      }
    }

    .control-area {
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: flex-end;
      gap: 8px;

      .send-btn {
        font-size: 12px;
        color: var(--uc-background-color-0);
        background-color: var(--uc-background-color-1000);
      }
    }
  `;ho([F({type:String,reflect:!0})],$e.prototype,"placeholder",2);ho([F({type:Number,attribute:"min-rows",reflect:!0})],$e.prototype,"minRows",2);ho([F({type:Number,attribute:"max-rows",reflect:!0})],$e.prototype,"maxRows",2);ho([F({type:Boolean,reflect:!0})],$e.prototype,"disabled",2);ho([F({type:Boolean,reflect:!0})],$e.prototype,"loading",2);ho([F({type:String,reflect:!0})],$e.prototype,"value",2);$e=ho([vt("message-input")],$e);var lu=Object.defineProperty,cu=Object.getOwnPropertyDescriptor,Ae=(e,t,o,i)=>{for(var s=i>1?void 0:i?cu(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&lu(t,o,s),s};let Gt=class extends nt{constructor(){super(...arguments),this.expanded=!1,this.status="warning",this.open=!1,this.maxRows=3,this.timeout=0,this.startTimer=async()=>{await this.updateComplete,this.clearTimer(),this.progressBarEl.animate([{transform:"scaleX(0)"},{transform:"scaleX(1)"}],{duration:this.timeout,easing:"linear",fill:"forwards"}),this.timeoutId=window.setTimeout(()=>{this.open=!1},this.timeout)}}updated(e){super.updated(e),e.has("status")&&this.status&&this.style.setProperty("--primary-color",`var(--uc-${this.status}-color)`),e.has("maxRows")&&this.open&&this.style.setProperty("--max-rows",`${this.maxRows}`),e.has("open")&&this.open&&this.timeout>0&&this.startTimer(),e.has("value")&&this.value&&this.startTimer()}render(){return B`
      <div class="container">
        <div class="progress">
          <div class="progress-bar"></div>
        </div>
        <div class="header">
          <uc-icon class="icon" name=${this.status}></uc-icon>
          <div class="title">${this.headline||this.status.toUpperCase()}</div>
          <div class="flex"></div>
          <uc-icon class="close-btn" name="x" @click=${()=>this.open=!1}></uc-icon>
        </div>
        <div class="body">
          ${this.value}
          ${this.expanded?U:B`<uc-icon class="expand-btn" name="chevron-down" @click=${()=>this.expanded=!0}></uc-icon>`}
        </div>
      </div>
    `}clearTimer(){this.timeoutId&&(clearTimeout(this.timeoutId),this.timeoutId=void 0)}};Gt.styles=bt`
    :host {
      opacity: 0;
      pointer-events: none;
      transform: translateY(20px);
      transition: opacity 0.3s ease, transform 0.3s ease;

      --primary-color: var(--uc-danger-color);
      --max-width: 500px;
      --max-height: 300px;
      --max-rows: 3;
    }
    :host([open]) {
      opacity: 1;
      pointer-events: auto;
      transform: translateY(0);
    }

    .container {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      justify-content: flex-start;
      max-width: var(--max-width);
      background-color: var(--uc-background-color-0);
      border: 1px solid var(--uc-border-color-mid);
      border-radius: 4px;
      box-shadow: 0 2px 10px var(--uc-shadow-color-mid);
      overflow: hidden;
    }

    .progress {
      display: block;
      height: 3px;
      width: 100%;

      .progress-bar {
        background-color: var(--primary-color);
        height: 100%;
        width: 100%;
        transform-origin: left;
      }
    }

    .header {
      width: 100%;
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      padding: 6px 8px 0px 8px;
      gap: 8px;
      box-sizing: border-box;

      .icon {
        font-size: 18px;
        color: var(--primary-color);
      }
      
      .title {
        font-size: 14px;
        font-weight: 600;
        line-height: 18px;
      }

      .flex {
        flex: 1;
      }

      .close-btn {
        font-size: 18px;
        color: var(--primary-color);
        cursor: pointer;
      }
      .close-btn:hover {
        opacity: 0.7;
      }
    }
  
    .body {
      width: 100%;
      font-size: 14px;
      font-weight: 300;
      line-height: 1.5;
      max-height: calc(1.5em * var(--max-rows) + 8px);
      padding: 4px 8px;
      box-sizing: border-box;
      overflow: hidden;
      transition: max-height 0.3s ease;

      .expand-btn {
        position: absolute;
        bottom: 4px;
        left: 50%;
        transform: translateX(-50%);
        color: rgba(171, 171, 171, 0.5);
        cursor: pointer;
      }
      .expand-btn:hover {
        opacity: 0.7;
      }
    }
    .body.expanded {
      overflow: auto;
      max-height: var(--max-height);
    }
  `;Ae([no(".progress-bar")],Gt.prototype,"progressBarEl",2);Ae([Yn()],Gt.prototype,"expanded",2);Ae([F({type:String})],Gt.prototype,"status",2);Ae([F({type:Boolean,reflect:!0})],Gt.prototype,"open",2);Ae([F({type:Number})],Gt.prototype,"maxRows",2);Ae([F({type:Number})],Gt.prototype,"timeout",2);Ae([F({type:String})],Gt.prototype,"headline",2);Ae([F({type:String})],Gt.prototype,"value",2);Gt=Ae([vt("message-alert")],Gt);var du=Object.defineProperty,hu=Object.getOwnPropertyDescriptor,zl=(e,t,o,i)=>{for(var s=i>1?void 0:i?hu(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&du(t,o,s),s};let ms=class extends nt{render(){return B`
      <div class="container">
        <div class="body">
          <slot></slot>
        </div>
        <div class="footer">
          ${Al(this.timestamp)}
        </div>
      </div>
    `}};ms.styles=bt`
    :host {
      display: block;
      width: 100%;
      height: auto;
    }

    .container {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      justify-content: flex-start;
    }

    .body {
      display: inline-flex;
      padding: 8px;
      border: none;
      border-radius: 8px;
      box-sizing: border-box;
      background-color: var(--uc-background-color-200);
    }

    .footer {
      align-self: flex-end;
      font-size: 12px;
      line-height: 1.5;
      opacity: 0.7;
      margin-top: 12px;
    }
  `;zl([F({type:String})],ms.prototype,"timestamp",2);ms=zl([vt("sent-message")],ms);var uu=Object.defineProperty,pu=Object.getOwnPropertyDescriptor,Ps=(e,t,o,i)=>{for(var s=i>1?void 0:i?pu(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&uu(t,o,s),s};let Eo=class extends nt{render(){return B`
      <div class="container">
        <!-- Avatar -->
        <img class="avatar" 
          src=${this.avatar||Xh.get("blank-avatar")||""}
          alt="Avatar" />

        <!-- Main -->
        <div class="main">
          ${this.name?B`<div class="header">${this.name}</div>`:U}
          <div class="body">
            <slot></slot>
          </div>
          ${this.timestamp?B`<div class="footer">${Al(this.timestamp)}</div>`:U}
        </div>
      </div>
    `}};Eo.styles=bt`
    :host {
      display: block;
      width: 100%;
    }

    .container {
      display: flex;
      flex-direction: row;
      width: 100%;
      height: auto;
    }

    .avatar {
      width: 34px;
      height: 34px;
      border: 1px solid var(--uc-border-color-mid);
      border-radius: 50%;
      margin-right: 16px;
      box-sizing: border-box;
    }

    .main {
      flex: 1;
      max-width: calc(100% - 50px);
      display: flex;
      flex-direction: column;

      .header {
        align-self: flex-start;
        font-family: 'Roboto', sans-serif;
        font-size: 16px;
        font-weight: 600;
        line-height: 1.5;
        margin-bottom: 12px;
      }

      .body {
        display: flex;
        flex-direction: column;
        gap: 12px;
        background-color: transparent;
      }

      .footer {
        align-self: flex-end;
        font-size: 12px;
        line-height: 1.5;
        opacity: 0.7;
        margin-top: 12px;
      }
    }
  `;Ps([F({type:String})],Eo.prototype,"avatar",2);Ps([F({type:String})],Eo.prototype,"name",2);Ps([F({type:String})],Eo.prototype,"timestamp",2);Eo=Ps([vt("received-message")],Eo);var fu=Object.defineProperty,mu=Object.getOwnPropertyDescriptor,El=(e,t,o,i)=>{for(var s=i>1?void 0:i?mu(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&fu(t,o,s),s};let gs=class extends nt{constructor(){super(...arguments),this.status="pending"}render(){return B`
      <div class="container">
        <div class="title">
          ${this.status.toUpperCase()}
        </div>
        <div class="indicator">
          ${this.status==="pending"?B`<uc-circle-pulser></uc-circle-pulser>`:B`<uc-bar-loader></uc-bar-loader>`}
        </div>
      </div>
    `}};gs.styles=bt`
    :host {
      display: block;
      width: 160px;
      height: 80px;

      padding: 8px;
      border-radius: 8px;
      border: 1px solid var(--uc-border-color-low);
      background-color: var(--uc-background-color-0);
      box-shadow: 0 1px 3px var(--uc-shadow-color-low);
      box-sizing: border-box;
    }
    
    .container {
      width: 100%;
      height: 100%;
      display: flex;
      flex-direction: column;
    }
    
    .title {
      font-size: 12px;
      line-height: 18px;
      font-weight: 600;
      color: var(--uc-text-color-high);
    }

    .indicator {
      width: 100%;
      height: calc(100% - 18px);
      display: flex;
      align-items: center;
      justify-content: center;
    }

    uc-circle-pulser {
      font-size: 40px;
    }

    uc-bar-loader {
      font-size: 40px;
    }
  `;El([F({type:String})],gs.prototype,"status",2);gs=El([vt("status-panel")],gs);var gu=Object.defineProperty,bu=Object.getOwnPropertyDescriptor,Mo=(e,t,o,i)=>{for(var s=i>1?void 0:i?bu(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&gu(t,o,s),s};let Ie=class extends nt{constructor(){super(...arguments),this.warningThreshold=.6,this.criticalThreshold=.8,this.maxValue=0,this.value=0,this.updateValue=async()=>{if(await this.updateComplete,this.value<0||this.maxValue<1)return;const e=Math.min(this.value/this.maxValue,1);this.gaugeBarEl.style.transform=`scaleX(${e})`,e>=this.criticalThreshold?(this.gaugeBarEl.classList.remove("warning"),this.gaugeBarEl.classList.add("critical")):e>=this.warningThreshold?(this.gaugeBarEl.classList.remove("critical"),this.gaugeBarEl.classList.add("warning")):this.gaugeBarEl.classList.remove("warning","critical")},this.formatValue=e=>e.toLocaleString()}updated(e){super.updated(e),(e.has("value")||e.has("maxValue"))&&this.updateValue()}render(){return B`
      <div class="container">
        <div class="title">
          TOKEN USAGE
        </div>
        <div class="counters">
          <div class="usage-count">
            ${this.formatValue(this.value)}
          </div>
          <div class="max-count">
            ${this.maxValue>0?this.formatValue(this.maxValue):"unknown"}
          </div>
        </div>
        ${this.maxValue>0?B`<div class="gauge"><div class="gauge-bar"></div></div>`:U}
      </div>
    `}};Ie.styles=bt`
    :host {
      display: block;
      width: 160px;
      height: 80px;

      padding: 8px;
      border-radius: 8px;
      border: 1px solid var(--uc-border-color-low);
      background-color: var(--uc-background-color-0);
      box-shadow: 0 1px 3px var(--uc-shadow-color-low);
      box-sizing: border-box;
    }

    .container {
      display: flex;
      flex-direction: column;
      width: 100%;
      gap: 6px;
    }

    .title {
      font-size: 12px;
      font-weight: 600;
      line-height: 1.5;
    }

    .counters {
      font-size: 12px;
      display: flex;
      flex-direction: row;
      align-items: baseline;
      justify-content: space-between;
      width: 100%;

      .usage-count {
        font-weight: 600;
        color: var(--uc-blue-color-600);
      }

      .max-count {
        font-weight: 300;
        color: var(--uc-text-color-mid);
      }
    }

    .gauge {
      width: 100%;
      height: 8px;
      border-radius: 4px;
      border: 1px solid var(--uc-border-color-low);
      background-color: var(--uc-background-color-300);
      box-sizing: border-box;
      overflow: hidden;

      .gauge-bar {
        width: 100%;
        height: 100%;
        background-color: var(--uc-success-color);
        transform-origin: left;
        transform: scaleX(0);
        transition: transform 0.35s cubic-bezier(0.4, 0, 0.2, 1);
      }
      .gauge-bar.warning {
        background-color: var(--uc-warning-color);
      }
      .gauge-bar.critical {
        background-color: var(--uc-danger-color);
      }
    }
  `;Mo([no(".gauge-bar")],Ie.prototype,"gaugeBarEl",2);Mo([F({type:Number})],Ie.prototype,"warningThreshold",2);Mo([F({type:Number})],Ie.prototype,"criticalThreshold",2);Mo([F({type:Number})],Ie.prototype,"maxValue",2);Mo([F({type:Number})],Ie.prototype,"value",2);Ie=Mo([vt("token-panel")],Ie);var Tl=Object.defineProperty,vu=Object.defineProperties,yu=Object.getOwnPropertyDescriptor,wu=Object.getOwnPropertyDescriptors,nn=Object.getOwnPropertySymbols,xu=Object.prototype.hasOwnProperty,ku=Object.prototype.propertyIsEnumerable,Qs=(e,t)=>(t=Symbol[e])?t:Symbol.for("Symbol."+e),sa=e=>{throw TypeError(e)},ln=(e,t,o)=>t in e?Tl(e,t,{enumerable:!0,configurable:!0,writable:!0,value:o}):e[t]=o,ze=(e,t)=>{for(var o in t||(t={}))xu.call(t,o)&&ln(e,o,t[o]);if(nn)for(var o of nn(t))ku.call(t,o)&&ln(e,o,t[o]);return e},Oi=(e,t)=>vu(e,wu(t)),l=(e,t,o,i)=>{for(var s=i>1?void 0:i?yu(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&Tl(t,o,s),s},Pl=(e,t,o)=>t.has(e)||sa("Cannot "+o),_u=(e,t,o)=>(Pl(e,t,"read from private field"),t.get(e)),$u=(e,t,o)=>t.has(e)?sa("Cannot add the same private member more than once"):t instanceof WeakSet?t.add(e):t.set(e,o),Cu=(e,t,o,i)=>(Pl(e,t,"write to private field"),t.set(e,o),o),Su=function(e,t){this[0]=e,this[1]=t},Au=e=>{var t=e[Qs("asyncIterator")],o=!1,i,s={};return t==null?(t=e[Qs("iterator")](),i=r=>s[r]=a=>t[r](a)):(t=t.call(e),i=r=>s[r]=a=>{if(o){if(o=!1,r==="throw")throw a;return a}return o=!0,{done:!1,value:new Su(new Promise(n=>{var c=t[r](a);c instanceof Object||sa("Object expected"),n(c)}),1)}}),s[Qs("iterator")]=()=>s,i("next"),"throw"in t?i("throw"):s.throw=r=>{throw r},"return"in t&&i("return"),s},Xo=new WeakMap,Qo=new WeakMap,Zo=new WeakMap,Zs=new WeakSet,Hi=new WeakMap,Ee=class{constructor(e,t){this.handleFormData=o=>{const i=this.options.disabled(this.host),s=this.options.name(this.host),r=this.options.value(this.host),a=this.host.tagName.toLowerCase()==="sl-button";this.host.isConnected&&!i&&!a&&typeof s=="string"&&s.length>0&&typeof r<"u"&&(Array.isArray(r)?r.forEach(n=>{o.formData.append(s,n.toString())}):o.formData.append(s,r.toString()))},this.handleFormSubmit=o=>{var i;const s=this.options.disabled(this.host),r=this.options.reportValidity;this.form&&!this.form.noValidate&&((i=Xo.get(this.form))==null||i.forEach(a=>{this.setUserInteracted(a,!0)})),this.form&&!this.form.noValidate&&!s&&!r(this.host)&&(o.preventDefault(),o.stopImmediatePropagation())},this.handleFormReset=()=>{this.options.setValue(this.host,this.options.defaultValue(this.host)),this.setUserInteracted(this.host,!1),Hi.set(this.host,[])},this.handleInteraction=o=>{const i=Hi.get(this.host);i.includes(o.type)||i.push(o.type),i.length===this.options.assumeInteractionOn.length&&this.setUserInteracted(this.host,!0)},this.checkFormValidity=()=>{if(this.form&&!this.form.noValidate){const o=this.form.querySelectorAll("*");for(const i of o)if(typeof i.checkValidity=="function"&&!i.checkValidity())return!1}return!0},this.reportFormValidity=()=>{if(this.form&&!this.form.noValidate){const o=this.form.querySelectorAll("*");for(const i of o)if(typeof i.reportValidity=="function"&&!i.reportValidity())return!1}return!0},(this.host=e).addController(this),this.options=ze({form:o=>{const i=o.form;if(i){const r=o.getRootNode().querySelector(`#${i}`);if(r)return r}return o.closest("form")},name:o=>o.name,value:o=>o.value,defaultValue:o=>o.defaultValue,disabled:o=>{var i;return(i=o.disabled)!=null?i:!1},reportValidity:o=>typeof o.reportValidity=="function"?o.reportValidity():!0,checkValidity:o=>typeof o.checkValidity=="function"?o.checkValidity():!0,setValue:(o,i)=>o.value=i,assumeInteractionOn:["sl-input"]},t)}hostConnected(){const e=this.options.form(this.host);e&&this.attachForm(e),Hi.set(this.host,[]),this.options.assumeInteractionOn.forEach(t=>{this.host.addEventListener(t,this.handleInteraction)})}hostDisconnected(){this.detachForm(),Hi.delete(this.host),this.options.assumeInteractionOn.forEach(e=>{this.host.removeEventListener(e,this.handleInteraction)})}hostUpdated(){const e=this.options.form(this.host);e||this.detachForm(),e&&this.form!==e&&(this.detachForm(),this.attachForm(e)),this.host.hasUpdated&&this.setValidity(this.host.validity.valid)}attachForm(e){e?(this.form=e,Xo.has(this.form)?Xo.get(this.form).add(this.host):Xo.set(this.form,new Set([this.host])),this.form.addEventListener("formdata",this.handleFormData),this.form.addEventListener("submit",this.handleFormSubmit),this.form.addEventListener("reset",this.handleFormReset),Qo.has(this.form)||(Qo.set(this.form,this.form.reportValidity),this.form.reportValidity=()=>this.reportFormValidity()),Zo.has(this.form)||(Zo.set(this.form,this.form.checkValidity),this.form.checkValidity=()=>this.checkFormValidity())):this.form=void 0}detachForm(){if(!this.form)return;const e=Xo.get(this.form);e&&(e.delete(this.host),e.size<=0&&(this.form.removeEventListener("formdata",this.handleFormData),this.form.removeEventListener("submit",this.handleFormSubmit),this.form.removeEventListener("reset",this.handleFormReset),Qo.has(this.form)&&(this.form.reportValidity=Qo.get(this.form),Qo.delete(this.form)),Zo.has(this.form)&&(this.form.checkValidity=Zo.get(this.form),Zo.delete(this.form)),this.form=void 0))}setUserInteracted(e,t){t?Zs.add(e):Zs.delete(e),e.requestUpdate()}doAction(e,t){if(this.form){const o=document.createElement("button");o.type=e,o.style.position="absolute",o.style.width="0",o.style.height="0",o.style.clipPath="inset(50%)",o.style.overflow="hidden",o.style.whiteSpace="nowrap",t&&(o.name=t.name,o.value=t.value,["formaction","formenctype","formmethod","formnovalidate","formtarget"].forEach(i=>{t.hasAttribute(i)&&o.setAttribute(i,t.getAttribute(i))})),this.form.append(o),o.click(),o.remove()}}getForm(){var e;return(e=this.form)!=null?e:null}reset(e){this.doAction("reset",e)}submit(e){this.doAction("submit",e)}setValidity(e){const t=this.host,o=!!Zs.has(t),i=!!t.required;t.toggleAttribute("data-required",i),t.toggleAttribute("data-optional",!i),t.toggleAttribute("data-invalid",!e),t.toggleAttribute("data-valid",e),t.toggleAttribute("data-user-invalid",!e&&o),t.toggleAttribute("data-user-valid",e&&o)}updateValidity(){const e=this.host;this.setValidity(e.validity.valid)}emitInvalidEvent(e){const t=new CustomEvent("sl-invalid",{bubbles:!1,composed:!1,cancelable:!0,detail:{}});e||t.preventDefault(),this.host.dispatchEvent(t)||e==null||e.preventDefault()}},Os=Object.freeze({badInput:!1,customError:!1,patternMismatch:!1,rangeOverflow:!1,rangeUnderflow:!1,stepMismatch:!1,tooLong:!1,tooShort:!1,typeMismatch:!1,valid:!0,valueMissing:!1}),zu=Object.freeze(Oi(ze({},Os),{valid:!1,valueMissing:!0})),Eu=Object.freeze(Oi(ze({},Os),{valid:!1,customError:!0})),Tu=O`
  :host {
    display: block;
    outline: 0;
    z-index: 0;
  }

  :host(:focus) {
    outline: none;
  }

  slot:not([name])::slotted(sl-icon) {
    margin-inline-end: var(--sl-spacing-x-small);
  }

  .tree-item {
    position: relative;
    display: flex;
    align-items: stretch;
    flex-direction: column;
    color: var(--sl-color-neutral-700);
    cursor: pointer;
    user-select: none;
    -webkit-user-select: none;
  }

  .tree-item__checkbox {
    pointer-events: none;
  }

  .tree-item__expand-button,
  .tree-item__checkbox,
  .tree-item__label {
    font-family: var(--sl-font-sans);
    font-size: var(--sl-font-size-medium);
    font-weight: var(--sl-font-weight-normal);
    line-height: var(--sl-line-height-dense);
    letter-spacing: var(--sl-letter-spacing-normal);
  }

  .tree-item__checkbox::part(base) {
    display: flex;
    align-items: center;
  }

  .tree-item__indentation {
    display: block;
    width: 1em;
    flex-shrink: 0;
  }

  .tree-item__expand-button {
    display: flex;
    align-items: center;
    justify-content: center;
    box-sizing: content-box;
    color: var(--sl-color-neutral-500);
    padding: var(--sl-spacing-x-small);
    width: 1rem;
    height: 1rem;
    flex-shrink: 0;
    cursor: pointer;
  }

  .tree-item__expand-button {
    transition: var(--sl-transition-medium) rotate ease;
  }

  .tree-item--expanded .tree-item__expand-button {
    rotate: 90deg;
  }

  .tree-item--expanded.tree-item--rtl .tree-item__expand-button {
    rotate: -90deg;
  }

  .tree-item--expanded slot[name='expand-icon'],
  .tree-item:not(.tree-item--expanded) slot[name='collapse-icon'] {
    display: none;
  }

  .tree-item:not(.tree-item--has-expand-button) .tree-item__expand-icon-slot {
    display: none;
  }

  .tree-item__expand-button--visible {
    cursor: pointer;
  }

  .tree-item__item {
    display: flex;
    align-items: center;
    border-inline-start: solid 3px transparent;
  }

  .tree-item--disabled .tree-item__item {
    opacity: 0.5;
    outline: none;
    cursor: not-allowed;
  }

  :host(:focus-visible) .tree-item__item {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
    z-index: 2;
  }

  :host(:not([aria-disabled='true'])) .tree-item--selected .tree-item__item {
    background-color: var(--sl-color-neutral-100);
    border-inline-start-color: var(--sl-color-primary-600);
  }

  :host(:not([aria-disabled='true'])) .tree-item__expand-button {
    color: var(--sl-color-neutral-600);
  }

  .tree-item__label {
    display: flex;
    align-items: center;
    transition: var(--sl-transition-fast) color;
  }

  .tree-item__children {
    display: block;
    font-size: calc(1em + var(--indent-size, var(--sl-spacing-medium)));
  }

  /* Indentation lines */
  .tree-item__children {
    position: relative;
  }

  .tree-item__children::before {
    content: '';
    position: absolute;
    top: var(--indent-guide-offset);
    bottom: var(--indent-guide-offset);
    left: calc(1em - (var(--indent-guide-width) / 2) - 1px);
    border-inline-end: var(--indent-guide-width) var(--indent-guide-style) var(--indent-guide-color);
    z-index: 1;
  }

  .tree-item--rtl .tree-item__children::before {
    left: auto;
    right: 1em;
  }

  @media (forced-colors: active) {
    :host(:not([aria-disabled='true'])) .tree-item--selected .tree-item__item {
      outline: dashed 1px SelectedItem;
    }
  }
`,Pu=O`
  :host {
    display: inline-block;
  }

  .checkbox {
    position: relative;
    display: inline-flex;
    align-items: flex-start;
    font-family: var(--sl-input-font-family);
    font-weight: var(--sl-input-font-weight);
    color: var(--sl-input-label-color);
    vertical-align: middle;
    cursor: pointer;
  }

  .checkbox--small {
    --toggle-size: var(--sl-toggle-size-small);
    font-size: var(--sl-input-font-size-small);
  }

  .checkbox--medium {
    --toggle-size: var(--sl-toggle-size-medium);
    font-size: var(--sl-input-font-size-medium);
  }

  .checkbox--large {
    --toggle-size: var(--sl-toggle-size-large);
    font-size: var(--sl-input-font-size-large);
  }

  .checkbox__control {
    flex: 0 0 auto;
    position: relative;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: var(--toggle-size);
    height: var(--toggle-size);
    border: solid var(--sl-input-border-width) var(--sl-input-border-color);
    border-radius: 2px;
    background-color: var(--sl-input-background-color);
    color: var(--sl-color-neutral-0);
    transition:
      var(--sl-transition-fast) border-color,
      var(--sl-transition-fast) background-color,
      var(--sl-transition-fast) color,
      var(--sl-transition-fast) box-shadow;
  }

  .checkbox__input {
    position: absolute;
    opacity: 0;
    padding: 0;
    margin: 0;
    pointer-events: none;
  }

  .checkbox__checked-icon,
  .checkbox__indeterminate-icon {
    display: inline-flex;
    width: var(--toggle-size);
    height: var(--toggle-size);
  }

  /* Hover */
  .checkbox:not(.checkbox--checked):not(.checkbox--disabled) .checkbox__control:hover {
    border-color: var(--sl-input-border-color-hover);
    background-color: var(--sl-input-background-color-hover);
  }

  /* Focus */
  .checkbox:not(.checkbox--checked):not(.checkbox--disabled) .checkbox__input:focus-visible ~ .checkbox__control {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  /* Checked/indeterminate */
  .checkbox--checked .checkbox__control,
  .checkbox--indeterminate .checkbox__control {
    border-color: var(--sl-color-primary-600);
    background-color: var(--sl-color-primary-600);
  }

  /* Checked/indeterminate + hover */
  .checkbox.checkbox--checked:not(.checkbox--disabled) .checkbox__control:hover,
  .checkbox.checkbox--indeterminate:not(.checkbox--disabled) .checkbox__control:hover {
    border-color: var(--sl-color-primary-500);
    background-color: var(--sl-color-primary-500);
  }

  /* Checked/indeterminate + focus */
  .checkbox.checkbox--checked:not(.checkbox--disabled) .checkbox__input:focus-visible ~ .checkbox__control,
  .checkbox.checkbox--indeterminate:not(.checkbox--disabled) .checkbox__input:focus-visible ~ .checkbox__control {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  /* Disabled */
  .checkbox--disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .checkbox__label {
    display: inline-block;
    color: var(--sl-input-label-color);
    line-height: var(--toggle-size);
    margin-inline-start: 0.5em;
    user-select: none;
    -webkit-user-select: none;
  }

  :host([required]) .checkbox__label::after {
    content: var(--sl-input-required-content);
    color: var(--sl-input-required-content-color);
    margin-inline-start: var(--sl-input-required-content-offset);
  }
`,Io=(e="value")=>(t,o)=>{const i=t.constructor,s=i.prototype.attributeChangedCallback;i.prototype.attributeChangedCallback=function(r,a,n){var c;const d=i.getPropertyOptions(e),u=typeof d.attribute=="string"?d.attribute:e;if(r===u){const h=d.converter||$o,m=(typeof h=="function"?h:(c=h==null?void 0:h.fromAttribute)!=null?c:$o.fromAttribute)(n,d.type);this[e]!==m&&(this[o]=m)}s.call(this,r,a,n)}},uo=O`
  .form-control .form-control__label {
    display: none;
  }

  .form-control .form-control__help-text {
    display: none;
  }

  /* Label */
  .form-control--has-label .form-control__label {
    display: inline-block;
    color: var(--sl-input-label-color);
    margin-bottom: var(--sl-spacing-3x-small);
  }

  .form-control--has-label.form-control--small .form-control__label {
    font-size: var(--sl-input-label-font-size-small);
  }

  .form-control--has-label.form-control--medium .form-control__label {
    font-size: var(--sl-input-label-font-size-medium);
  }

  .form-control--has-label.form-control--large .form-control__label {
    font-size: var(--sl-input-label-font-size-large);
  }

  :host([required]) .form-control--has-label .form-control__label::after {
    content: var(--sl-input-required-content);
    margin-inline-start: var(--sl-input-required-content-offset);
    color: var(--sl-input-required-content-color);
  }

  /* Help text */
  .form-control--has-help-text .form-control__help-text {
    display: block;
    color: var(--sl-input-help-text-color);
    margin-top: var(--sl-spacing-3x-small);
  }

  .form-control--has-help-text.form-control--small .form-control__help-text {
    font-size: var(--sl-input-help-text-font-size-small);
  }

  .form-control--has-help-text.form-control--medium .form-control__help-text {
    font-size: var(--sl-input-help-text-font-size-medium);
  }

  .form-control--has-help-text.form-control--large .form-control__help-text {
    font-size: var(--sl-input-help-text-font-size-large);
  }

  .form-control--has-help-text.form-control--radio-group .form-control__help-text {
    margin-top: var(--sl-spacing-2x-small);
  }
`,Ot=class{constructor(e,...t){this.slotNames=[],this.handleSlotChange=o=>{const i=o.target;(this.slotNames.includes("[default]")&&!i.name||i.name&&this.slotNames.includes(i.name))&&this.host.requestUpdate()},(this.host=e).addController(this),this.slotNames=t}hasDefaultSlot(){return[...this.host.childNodes].some(e=>{if(e.nodeType===e.TEXT_NODE&&e.textContent.trim()!=="")return!0;if(e.nodeType===e.ELEMENT_NODE){const t=e;if(t.tagName.toLowerCase()==="sl-visually-hidden")return!1;if(!t.hasAttribute("slot"))return!0}return!1})}hasNamedSlot(e){return this.host.querySelector(`:scope > [slot="${e}"]`)!==null}test(e){return e==="[default]"?this.hasDefaultSlot():this.hasNamedSlot(e)}hostConnected(){this.host.shadowRoot.addEventListener("slotchange",this.handleSlotChange)}hostDisconnected(){this.host.shadowRoot.removeEventListener("slotchange",this.handleSlotChange)}};function Ou(e){if(!e)return"";const t=e.assignedNodes({flatten:!0});let o="";return[...t].forEach(i=>{i.nodeType===Node.TEXT_NODE&&(o+=i.textContent)}),o}var mr="";function cn(e){mr=e}function Lu(e=""){if(!mr){const t=[...document.getElementsByTagName("script")],o=t.find(i=>i.hasAttribute("data-shoelace"));if(o)cn(o.getAttribute("data-shoelace"));else{const i=t.find(r=>/shoelace(\.min)?\.js($|\?)/.test(r.src)||/shoelace-autoloader(\.min)?\.js($|\?)/.test(r.src));let s="";i&&(s=i.getAttribute("src")),cn(s.split("/").slice(0,-1).join("/"))}}return mr.replace(/\/$/,"")+(e?`/${e.replace(/^\//,"")}`:"")}var Ru={name:"default",resolver:e=>Lu(`assets/icons/${e}.svg`)},Du=Ru,dn={caret:`
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <polyline points="6 9 12 15 18 9"></polyline>
    </svg>
  `,check:`
    <svg part="checked-icon" class="checkbox__icon" viewBox="0 0 16 16">
      <g stroke="none" stroke-width="1" fill="none" fill-rule="evenodd" stroke-linecap="round">
        <g stroke="currentColor">
          <g transform="translate(3.428571, 3.428571)">
            <path d="M0,5.71428571 L3.42857143,9.14285714"></path>
            <path d="M9.14285714,0 L3.42857143,9.14285714"></path>
          </g>
        </g>
      </g>
    </svg>
  `,"chevron-down":`
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-chevron-down" viewBox="0 0 16 16">
      <path fill-rule="evenodd" d="M1.646 4.646a.5.5 0 0 1 .708 0L8 10.293l5.646-5.647a.5.5 0 0 1 .708.708l-6 6a.5.5 0 0 1-.708 0l-6-6a.5.5 0 0 1 0-.708z"/>
    </svg>
  `,"chevron-left":`
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-chevron-left" viewBox="0 0 16 16">
      <path fill-rule="evenodd" d="M11.354 1.646a.5.5 0 0 1 0 .708L5.707 8l5.647 5.646a.5.5 0 0 1-.708.708l-6-6a.5.5 0 0 1 0-.708l6-6a.5.5 0 0 1 .708 0z"/>
    </svg>
  `,"chevron-right":`
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-chevron-right" viewBox="0 0 16 16">
      <path fill-rule="evenodd" d="M4.646 1.646a.5.5 0 0 1 .708 0l6 6a.5.5 0 0 1 0 .708l-6 6a.5.5 0 0 1-.708-.708L10.293 8 4.646 2.354a.5.5 0 0 1 0-.708z"/>
    </svg>
  `,copy:`
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-copy" viewBox="0 0 16 16">
      <path fill-rule="evenodd" d="M4 2a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V2Zm2-1a1 1 0 0 0-1 1v8a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1V2a1 1 0 0 0-1-1H6ZM2 5a1 1 0 0 0-1 1v8a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1v-1h1v1a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h1v1H2Z"/>
    </svg>
  `,eye:`
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-eye" viewBox="0 0 16 16">
      <path d="M16 8s-3-5.5-8-5.5S0 8 0 8s3 5.5 8 5.5S16 8 16 8zM1.173 8a13.133 13.133 0 0 1 1.66-2.043C4.12 4.668 5.88 3.5 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.133 13.133 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755C11.879 11.332 10.119 12.5 8 12.5c-2.12 0-3.879-1.168-5.168-2.457A13.134 13.134 0 0 1 1.172 8z"/>
      <path d="M8 5.5a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5zM4.5 8a3.5 3.5 0 1 1 7 0 3.5 3.5 0 0 1-7 0z"/>
    </svg>
  `,"eye-slash":`
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-eye-slash" viewBox="0 0 16 16">
      <path d="M13.359 11.238C15.06 9.72 16 8 16 8s-3-5.5-8-5.5a7.028 7.028 0 0 0-2.79.588l.77.771A5.944 5.944 0 0 1 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.134 13.134 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755-.165.165-.337.328-.517.486l.708.709z"/>
      <path d="M11.297 9.176a3.5 3.5 0 0 0-4.474-4.474l.823.823a2.5 2.5 0 0 1 2.829 2.829l.822.822zm-2.943 1.299.822.822a3.5 3.5 0 0 1-4.474-4.474l.823.823a2.5 2.5 0 0 0 2.829 2.829z"/>
      <path d="M3.35 5.47c-.18.16-.353.322-.518.487A13.134 13.134 0 0 0 1.172 8l.195.288c.335.48.83 1.12 1.465 1.755C4.121 11.332 5.881 12.5 8 12.5c.716 0 1.39-.133 2.02-.36l.77.772A7.029 7.029 0 0 1 8 13.5C3 13.5 0 8 0 8s.939-1.721 2.641-3.238l.708.709zm10.296 8.884-12-12 .708-.708 12 12-.708.708z"/>
    </svg>
  `,eyedropper:`
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-eyedropper" viewBox="0 0 16 16">
      <path d="M13.354.646a1.207 1.207 0 0 0-1.708 0L8.5 3.793l-.646-.647a.5.5 0 1 0-.708.708L8.293 5l-7.147 7.146A.5.5 0 0 0 1 12.5v1.793l-.854.853a.5.5 0 1 0 .708.707L1.707 15H3.5a.5.5 0 0 0 .354-.146L11 7.707l1.146 1.147a.5.5 0 0 0 .708-.708l-.647-.646 3.147-3.146a1.207 1.207 0 0 0 0-1.708l-2-2zM2 12.707l7-7L10.293 7l-7 7H2v-1.293z"></path>
    </svg>
  `,"grip-vertical":`
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-grip-vertical" viewBox="0 0 16 16">
      <path d="M7 2a1 1 0 1 1-2 0 1 1 0 0 1 2 0zm3 0a1 1 0 1 1-2 0 1 1 0 0 1 2 0zM7 5a1 1 0 1 1-2 0 1 1 0 0 1 2 0zm3 0a1 1 0 1 1-2 0 1 1 0 0 1 2 0zM7 8a1 1 0 1 1-2 0 1 1 0 0 1 2 0zm3 0a1 1 0 1 1-2 0 1 1 0 0 1 2 0zm-3 3a1 1 0 1 1-2 0 1 1 0 0 1 2 0zm3 0a1 1 0 1 1-2 0 1 1 0 0 1 2 0zm-3 3a1 1 0 1 1-2 0 1 1 0 0 1 2 0zm3 0a1 1 0 1 1-2 0 1 1 0 0 1 2 0z"></path>
    </svg>
  `,indeterminate:`
    <svg part="indeterminate-icon" class="checkbox__icon" viewBox="0 0 16 16">
      <g stroke="none" stroke-width="1" fill="none" fill-rule="evenodd" stroke-linecap="round">
        <g stroke="currentColor" stroke-width="2">
          <g transform="translate(2.285714, 6.857143)">
            <path d="M10.2857143,1.14285714 L1.14285714,1.14285714"></path>
          </g>
        </g>
      </g>
    </svg>
  `,"person-fill":`
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-person-fill" viewBox="0 0 16 16">
      <path d="M3 14s-1 0-1-1 1-4 6-4 6 3 6 4-1 1-1 1H3zm5-6a3 3 0 1 0 0-6 3 3 0 0 0 0 6z"/>
    </svg>
  `,"play-fill":`
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-play-fill" viewBox="0 0 16 16">
      <path d="m11.596 8.697-6.363 3.692c-.54.313-1.233-.066-1.233-.697V4.308c0-.63.692-1.01 1.233-.696l6.363 3.692a.802.802 0 0 1 0 1.393z"></path>
    </svg>
  `,"pause-fill":`
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-pause-fill" viewBox="0 0 16 16">
      <path d="M5.5 3.5A1.5 1.5 0 0 1 7 5v6a1.5 1.5 0 0 1-3 0V5a1.5 1.5 0 0 1 1.5-1.5zm5 0A1.5 1.5 0 0 1 12 5v6a1.5 1.5 0 0 1-3 0V5a1.5 1.5 0 0 1 1.5-1.5z"></path>
    </svg>
  `,radio:`
    <svg part="checked-icon" class="radio__icon" viewBox="0 0 16 16">
      <g stroke="none" stroke-width="1" fill="none" fill-rule="evenodd">
        <g fill="currentColor">
          <circle cx="8" cy="8" r="3.42857143"></circle>
        </g>
      </g>
    </svg>
  `,"star-fill":`
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-star-fill" viewBox="0 0 16 16">
      <path d="M3.612 15.443c-.386.198-.824-.149-.746-.592l.83-4.73L.173 6.765c-.329-.314-.158-.888.283-.95l4.898-.696L7.538.792c.197-.39.73-.39.927 0l2.184 4.327 4.898.696c.441.062.612.636.282.95l-3.522 3.356.83 4.73c.078.443-.36.79-.746.592L8 13.187l-4.389 2.256z"/>
    </svg>
  `,"x-lg":`
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-x-lg" viewBox="0 0 16 16">
      <path d="M2.146 2.854a.5.5 0 1 1 .708-.708L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8 2.146 2.854Z"/>
    </svg>
  `,"x-circle-fill":`
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-x-circle-fill" viewBox="0 0 16 16">
      <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM5.354 4.646a.5.5 0 1 0-.708.708L7.293 8l-2.647 2.646a.5.5 0 0 0 .708.708L8 8.707l2.646 2.647a.5.5 0 0 0 .708-.708L8.707 8l2.647-2.646a.5.5 0 0 0-.708-.708L8 7.293 5.354 4.646z"></path>
    </svg>
  `},Mu={name:"system",resolver:e=>e in dn?`data:image/svg+xml,${encodeURIComponent(dn[e])}`:""},Iu=Mu,Bu=[Du,Iu],gr=[];function Fu(e){gr.push(e)}function Vu(e){gr=gr.filter(t=>t!==e)}function hn(e){return Bu.find(t=>t.name===e)}var Uu=O`
  :host {
    display: inline-block;
    width: 1em;
    height: 1em;
    box-sizing: content-box !important;
  }

  svg {
    display: block;
    height: 100%;
    width: 100%;
  }
`;function C(e,t){const o=ze({waitUntilFirstUpdate:!1},t);return(i,s)=>{const{update:r}=i,a=Array.isArray(e)?e:[e];i.update=function(n){a.forEach(c=>{const d=c;if(n.has(d)){const u=n.get(d),h=this[d];u!==h&&(!o.waitUntilFirstUpdate||this.hasUpdated)&&this[s](u,h)}}),r.call(this,n)}}}var D=O`
  :host {
    box-sizing: border-box;
  }

  :host *,
  :host *::before,
  :host *::after {
    box-sizing: inherit;
  }

  [hidden] {
    display: none !important;
  }
`,Ji,T=class extends de{constructor(){super(),$u(this,Ji,!1),this.initialReflectedProperties=new Map,Object.entries(this.constructor.dependencies).forEach(([e,t])=>{this.constructor.define(e,t)})}emit(e,t){const o=new CustomEvent(e,ze({bubbles:!0,cancelable:!1,composed:!0,detail:{}},t));return this.dispatchEvent(o),o}static define(e,t=this,o={}){const i=customElements.get(e);if(!i){try{customElements.define(e,t,o)}catch{customElements.define(e,class extends t{},o)}return}let s=" (unknown version)",r=s;"version"in t&&t.version&&(s=" v"+t.version),"version"in i&&i.version&&(r=" v"+i.version),!(s&&r&&s===r)&&console.warn(`Attempted to register <${e}>${s}, but <${e}>${r} has already been registered.`)}attributeChangedCallback(e,t,o){_u(this,Ji)||(this.constructor.elementProperties.forEach((i,s)=>{i.reflect&&this[s]!=null&&this.initialReflectedProperties.set(s,this[s])}),Cu(this,Ji,!0)),super.attributeChangedCallback(e,t,o)}willUpdate(e){super.willUpdate(e),this.initialReflectedProperties.forEach((t,o)=>{e.has(o)&&this[o]==null&&(this[o]=t)})}};Ji=new WeakMap;T.version="2.20.1";T.dependencies={};l([p()],T.prototype,"dir",2);l([p()],T.prototype,"lang",2);/**
 * @license
 * Copyright 2020 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Nu=(e,t)=>(e==null?void 0:e._$litType$)!==void 0,Ol=e=>e.strings===void 0,Hu={},ju=(e,t=Hu)=>e._$AH=t;var Go=Symbol(),ji=Symbol(),Gs,Js=new Map,J=class extends T{constructor(){super(...arguments),this.initialRender=!1,this.svg=null,this.label="",this.library="default"}async resolveIcon(e,t){var o;let i;if(t!=null&&t.spriteSheet)return this.svg=x`<svg part="svg">
        <use part="use" href="${e}"></use>
      </svg>`,this.svg;try{if(i=await fetch(e,{mode:"cors"}),!i.ok)return i.status===410?Go:ji}catch{return ji}try{const s=document.createElement("div");s.innerHTML=await i.text();const r=s.firstElementChild;if(((o=r==null?void 0:r.tagName)==null?void 0:o.toLowerCase())!=="svg")return Go;Gs||(Gs=new DOMParser);const n=Gs.parseFromString(r.outerHTML,"text/html").body.querySelector("svg");return n?(n.part.add("svg"),document.adoptNode(n)):Go}catch{return Go}}connectedCallback(){super.connectedCallback(),Fu(this)}firstUpdated(){this.initialRender=!0,this.setIcon()}disconnectedCallback(){super.disconnectedCallback(),Vu(this)}getIconSource(){const e=hn(this.library);return this.name&&e?{url:e.resolver(this.name),fromLibrary:!0}:{url:this.src,fromLibrary:!1}}handleLabelChange(){typeof this.label=="string"&&this.label.length>0?(this.setAttribute("role","img"),this.setAttribute("aria-label",this.label),this.removeAttribute("aria-hidden")):(this.removeAttribute("role"),this.removeAttribute("aria-label"),this.setAttribute("aria-hidden","true"))}async setIcon(){var e;const{url:t,fromLibrary:o}=this.getIconSource(),i=o?hn(this.library):void 0;if(!t){this.svg=null;return}let s=Js.get(t);if(s||(s=this.resolveIcon(t,i),Js.set(t,s)),!this.initialRender)return;const r=await s;if(r===ji&&Js.delete(t),t===this.getIconSource().url){if(Nu(r)){if(this.svg=r,i){await this.updateComplete;const a=this.shadowRoot.querySelector("[part='svg']");typeof i.mutator=="function"&&a&&i.mutator(a)}return}switch(r){case ji:case Go:this.svg=null,this.emit("sl-error");break;default:this.svg=r.cloneNode(!0),(e=i==null?void 0:i.mutator)==null||e.call(i,this.svg),this.emit("sl-load")}}}render(){return this.svg}};J.styles=[D,Uu];l([z()],J.prototype,"svg",2);l([p({reflect:!0})],J.prototype,"name",2);l([p()],J.prototype,"src",2);l([p()],J.prototype,"label",2);l([p({reflect:!0})],J.prototype,"library",2);l([C("label")],J.prototype,"handleLabelChange",1);l([C(["name","src","library"])],J.prototype,"setIcon",1);/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const ce={ATTRIBUTE:1,CHILD:2,PROPERTY:3,BOOLEAN_ATTRIBUTE:4},Li=e=>(...t)=>({_$litDirective$:e,values:t});let Ri=class{constructor(t){}get _$AU(){return this._$AM._$AU}_$AT(t,o,i){this._$Ct=t,this._$AM=o,this._$Ci=i}_$AS(t,o){return this.update(t,o)}update(t,o){return this.render(...o)}};/**
 * @license
 * Copyright 2018 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const R=Li(class extends Ri{constructor(e){var t;if(super(e),e.type!==ce.ATTRIBUTE||e.name!=="class"||((t=e.strings)==null?void 0:t.length)>2)throw Error("`classMap()` can only be used in the `class` attribute and must be the only part in the attribute.")}render(e){return" "+Object.keys(e).filter(t=>e[t]).join(" ")+" "}update(e,[t]){var i,s;if(this.st===void 0){this.st=new Set,e.strings!==void 0&&(this.nt=new Set(e.strings.join(" ").split(/\s/).filter(r=>r!=="")));for(const r in t)t[r]&&!((i=this.nt)!=null&&i.has(r))&&this.st.add(r);return this.render(t)}const o=e.element.classList;for(const r of this.st)r in t||(o.remove(r),this.st.delete(r));for(const r in t){const a=!!t[r];a===this.st.has(r)||(s=this.nt)!=null&&s.has(r)||(a?(o.add(r),this.st.add(r)):(o.remove(r),this.st.delete(r)))}return Bt}});/**
 * @license
 * Copyright 2018 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const E=e=>e??et;/**
 * @license
 * Copyright 2020 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const io=Li(class extends Ri{constructor(e){if(super(e),e.type!==ce.PROPERTY&&e.type!==ce.ATTRIBUTE&&e.type!==ce.BOOLEAN_ATTRIBUTE)throw Error("The `live` directive is not allowed on child or event bindings");if(!Ol(e))throw Error("`live` bindings can only contain a single expression")}render(e){return e}update(e,[t]){if(t===Bt||t===et)return t;const o=e.element,i=e.name;if(e.type===ce.PROPERTY){if(t===o[i])return Bt}else if(e.type===ce.BOOLEAN_ATTRIBUTE){if(!!t===o.hasAttribute(i))return Bt}else if(e.type===ce.ATTRIBUTE&&o.getAttribute(i)===t+"")return Bt;return ju(e),t}});var mt=class extends T{constructor(){super(...arguments),this.formControlController=new Ee(this,{value:e=>e.checked?e.value||"on":void 0,defaultValue:e=>e.defaultChecked,setValue:(e,t)=>e.checked=t}),this.hasSlotController=new Ot(this,"help-text"),this.hasFocus=!1,this.title="",this.name="",this.size="medium",this.disabled=!1,this.checked=!1,this.indeterminate=!1,this.defaultChecked=!1,this.form="",this.required=!1,this.helpText=""}get validity(){return this.input.validity}get validationMessage(){return this.input.validationMessage}firstUpdated(){this.formControlController.updateValidity()}handleClick(){this.checked=!this.checked,this.indeterminate=!1,this.emit("sl-change")}handleBlur(){this.hasFocus=!1,this.emit("sl-blur")}handleInput(){this.emit("sl-input")}handleInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}handleFocus(){this.hasFocus=!0,this.emit("sl-focus")}handleDisabledChange(){this.formControlController.setValidity(this.disabled)}handleStateChange(){this.input.checked=this.checked,this.input.indeterminate=this.indeterminate,this.formControlController.updateValidity()}click(){this.input.click()}focus(e){this.input.focus(e)}blur(){this.input.blur()}checkValidity(){return this.input.checkValidity()}getForm(){return this.formControlController.getForm()}reportValidity(){return this.input.reportValidity()}setCustomValidity(e){this.input.setCustomValidity(e),this.formControlController.updateValidity()}render(){const e=this.hasSlotController.test("help-text"),t=this.helpText?!0:!!e;return x`
      <div
        class=${R({"form-control":!0,"form-control--small":this.size==="small","form-control--medium":this.size==="medium","form-control--large":this.size==="large","form-control--has-help-text":t})}
      >
        <label
          part="base"
          class=${R({checkbox:!0,"checkbox--checked":this.checked,"checkbox--disabled":this.disabled,"checkbox--focused":this.hasFocus,"checkbox--indeterminate":this.indeterminate,"checkbox--small":this.size==="small","checkbox--medium":this.size==="medium","checkbox--large":this.size==="large"})}
        >
          <input
            class="checkbox__input"
            type="checkbox"
            title=${this.title}
            name=${this.name}
            value=${E(this.value)}
            .indeterminate=${io(this.indeterminate)}
            .checked=${io(this.checked)}
            .disabled=${this.disabled}
            .required=${this.required}
            aria-checked=${this.checked?"true":"false"}
            aria-describedby="help-text"
            @click=${this.handleClick}
            @input=${this.handleInput}
            @invalid=${this.handleInvalid}
            @blur=${this.handleBlur}
            @focus=${this.handleFocus}
          />

          <span
            part="control${this.checked?" control--checked":""}${this.indeterminate?" control--indeterminate":""}"
            class="checkbox__control"
          >
            ${this.checked?x`
                  <sl-icon part="checked-icon" class="checkbox__checked-icon" library="system" name="check"></sl-icon>
                `:""}
            ${!this.checked&&this.indeterminate?x`
                  <sl-icon
                    part="indeterminate-icon"
                    class="checkbox__indeterminate-icon"
                    library="system"
                    name="indeterminate"
                  ></sl-icon>
                `:""}
          </span>

          <div part="label" class="checkbox__label">
            <slot></slot>
          </div>
        </label>

        <div
          aria-hidden=${t?"false":"true"}
          class="form-control__help-text"
          id="help-text"
          part="form-control-help-text"
        >
          <slot name="help-text">${this.helpText}</slot>
        </div>
      </div>
    `}};mt.styles=[D,uo,Pu];mt.dependencies={"sl-icon":J};l([S('input[type="checkbox"]')],mt.prototype,"input",2);l([z()],mt.prototype,"hasFocus",2);l([p()],mt.prototype,"title",2);l([p()],mt.prototype,"name",2);l([p()],mt.prototype,"value",2);l([p({reflect:!0})],mt.prototype,"size",2);l([p({type:Boolean,reflect:!0})],mt.prototype,"disabled",2);l([p({type:Boolean,reflect:!0})],mt.prototype,"checked",2);l([p({type:Boolean,reflect:!0})],mt.prototype,"indeterminate",2);l([Io("checked")],mt.prototype,"defaultChecked",2);l([p({reflect:!0})],mt.prototype,"form",2);l([p({type:Boolean,reflect:!0})],mt.prototype,"required",2);l([p({attribute:"help-text"})],mt.prototype,"helpText",2);l([C("disabled",{waitUntilFirstUpdate:!0})],mt.prototype,"handleDisabledChange",1);l([C(["checked","indeterminate"],{waitUntilFirstUpdate:!0})],mt.prototype,"handleStateChange",1);var qu=O`
  :host {
    --track-width: 2px;
    --track-color: rgb(128 128 128 / 25%);
    --indicator-color: var(--sl-color-primary-600);
    --speed: 2s;

    display: inline-flex;
    width: 1em;
    height: 1em;
    flex: none;
  }

  .spinner {
    flex: 1 1 auto;
    height: 100%;
    width: 100%;
  }

  .spinner__track,
  .spinner__indicator {
    fill: none;
    stroke-width: var(--track-width);
    r: calc(0.5em - var(--track-width) / 2);
    cx: 0.5em;
    cy: 0.5em;
    transform-origin: 50% 50%;
  }

  .spinner__track {
    stroke: var(--track-color);
    transform-origin: 0% 0%;
  }

  .spinner__indicator {
    stroke: var(--indicator-color);
    stroke-linecap: round;
    stroke-dasharray: 150% 75%;
    animation: spin var(--speed) linear infinite;
  }

  @keyframes spin {
    0% {
      transform: rotate(0deg);
      stroke-dasharray: 0.05em, 3em;
    }

    50% {
      transform: rotate(450deg);
      stroke-dasharray: 1.375em, 1.375em;
    }

    100% {
      transform: rotate(1080deg);
      stroke-dasharray: 0.05em, 3em;
    }
  }
`;const br=new Set,wo=new Map;let Ke,ra="ltr",aa="en";const Ll=typeof MutationObserver<"u"&&typeof document<"u"&&typeof document.documentElement<"u";if(Ll){const e=new MutationObserver(Dl);ra=document.documentElement.dir||"ltr",aa=document.documentElement.lang||navigator.language,e.observe(document.documentElement,{attributes:!0,attributeFilter:["dir","lang"]})}function Rl(...e){e.map(t=>{const o=t.$code.toLowerCase();wo.has(o)?wo.set(o,Object.assign(Object.assign({},wo.get(o)),t)):wo.set(o,t),Ke||(Ke=t)}),Dl()}function Dl(){Ll&&(ra=document.documentElement.dir||"ltr",aa=document.documentElement.lang||navigator.language),[...br.keys()].map(e=>{typeof e.requestUpdate=="function"&&e.requestUpdate()})}let Wu=class{constructor(t){this.host=t,this.host.addController(this)}hostConnected(){br.add(this.host)}hostDisconnected(){br.delete(this.host)}dir(){return`${this.host.dir||ra}`.toLowerCase()}lang(){return`${this.host.lang||aa}`.toLowerCase()}getTranslationData(t){var o,i;const s=new Intl.Locale(t.replace(/_/g,"-")),r=s==null?void 0:s.language.toLowerCase(),a=(i=(o=s==null?void 0:s.region)===null||o===void 0?void 0:o.toLowerCase())!==null&&i!==void 0?i:"",n=wo.get(`${r}-${a}`),c=wo.get(r);return{locale:s,language:r,region:a,primary:n,secondary:c}}exists(t,o){var i;const{primary:s,secondary:r}=this.getTranslationData((i=o.lang)!==null&&i!==void 0?i:this.lang());return o=Object.assign({includeFallback:!1},o),!!(s&&s[t]||r&&r[t]||o.includeFallback&&Ke&&Ke[t])}term(t,...o){const{primary:i,secondary:s}=this.getTranslationData(this.lang());let r;if(i&&i[t])r=i[t];else if(s&&s[t])r=s[t];else if(Ke&&Ke[t])r=Ke[t];else return console.error(`No translation found for: ${String(t)}`),String(t);return typeof r=="function"?r(...o):r}date(t,o){return t=new Date(t),new Intl.DateTimeFormat(this.lang(),o).format(t)}number(t,o){return t=Number(t),isNaN(t)?"":new Intl.NumberFormat(this.lang(),o).format(t)}relativeTime(t,o,i){return new Intl.RelativeTimeFormat(this.lang(),i).format(t,o)}};var Ml={$code:"en",$name:"English",$dir:"ltr",carousel:"Carousel",clearEntry:"Clear entry",close:"Close",copied:"Copied",copy:"Copy",currentValue:"Current value",error:"Error",goToSlide:(e,t)=>`Go to slide ${e} of ${t}`,hidePassword:"Hide password",loading:"Loading",nextSlide:"Next slide",numOptionsSelected:e=>e===0?"No options selected":e===1?"1 option selected":`${e} options selected`,previousSlide:"Previous slide",progress:"Progress",remove:"Remove",resize:"Resize",scrollToEnd:"Scroll to end",scrollToStart:"Scroll to start",selectAColorFromTheScreen:"Select a color from the screen",showPassword:"Show password",slideNum:e=>`Slide ${e}`,toggleColorFormat:"Toggle color format"};Rl(Ml);var Ku=Ml,j=class extends Wu{};Rl(Ku);var Di=class extends T{constructor(){super(...arguments),this.localize=new j(this)}render(){return x`
      <svg part="base" class="spinner" role="progressbar" aria-label=${this.localize.term("loading")}>
        <circle class="spinner__track"></circle>
        <circle class="spinner__indicator"></circle>
      </svg>
    `}};Di.styles=[D,qu];var Il=new Map,Yu=new WeakMap;function Xu(e){return e??{keyframes:[],options:{duration:0}}}function un(e,t){return t.toLowerCase()==="rtl"?{keyframes:e.rtlKeyframes||e.keyframes,options:e.options}:e}function K(e,t){Il.set(e,Xu(t))}function ot(e,t,o){const i=Yu.get(e);if(i!=null&&i[t])return un(i[t],o.dir);const s=Il.get(t);return s?un(s,o.dir):{keyframes:[],options:{duration:0}}}function at(e,t,o){return new Promise(i=>{if((o==null?void 0:o.duration)===1/0)throw new Error("Promise-based animations must be finite.");const s=e.animate(t,Oi(ze({},o),{duration:vr()?0:o.duration}));s.addEventListener("cancel",i,{once:!0}),s.addEventListener("finish",i,{once:!0})})}function pn(e){return e=e.toString().toLowerCase(),e.indexOf("ms")>-1?parseFloat(e):e.indexOf("s")>-1?parseFloat(e)*1e3:parseFloat(e)}function vr(){return window.matchMedia("(prefers-reduced-motion: reduce)").matches}function ut(e){return Promise.all(e.getAnimations().map(t=>new Promise(o=>{t.cancel(),requestAnimationFrame(o)})))}function bs(e,t){return e.map(o=>Oi(ze({},o),{height:o.height==="auto"?`${t}px`:o.height}))}/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function fn(e,t,o){return e?t(e):o==null?void 0:o(e)}var lt=class yr extends T{constructor(){super(...arguments),this.localize=new j(this),this.indeterminate=!1,this.isLeaf=!1,this.loading=!1,this.selectable=!1,this.expanded=!1,this.selected=!1,this.disabled=!1,this.lazy=!1}static isTreeItem(t){return t instanceof Element&&t.getAttribute("role")==="treeitem"}connectedCallback(){super.connectedCallback(),this.setAttribute("role","treeitem"),this.setAttribute("tabindex","-1"),this.isNestedItem()&&(this.slot="children")}firstUpdated(){this.childrenContainer.hidden=!this.expanded,this.childrenContainer.style.height=this.expanded?"auto":"0",this.isLeaf=!this.lazy&&this.getChildrenItems().length===0,this.handleExpandedChange()}async animateCollapse(){this.emit("sl-collapse"),await ut(this.childrenContainer);const{keyframes:t,options:o}=ot(this,"tree-item.collapse",{dir:this.localize.dir()});await at(this.childrenContainer,bs(t,this.childrenContainer.scrollHeight),o),this.childrenContainer.hidden=!0,this.emit("sl-after-collapse")}isNestedItem(){const t=this.parentElement;return!!t&&yr.isTreeItem(t)}handleChildrenSlotChange(){this.loading=!1,this.isLeaf=!this.lazy&&this.getChildrenItems().length===0}willUpdate(t){t.has("selected")&&!t.has("indeterminate")&&(this.indeterminate=!1)}async animateExpand(){this.emit("sl-expand"),await ut(this.childrenContainer),this.childrenContainer.hidden=!1;const{keyframes:t,options:o}=ot(this,"tree-item.expand",{dir:this.localize.dir()});await at(this.childrenContainer,bs(t,this.childrenContainer.scrollHeight),o),this.childrenContainer.style.height="auto",this.emit("sl-after-expand")}handleLoadingChange(){this.setAttribute("aria-busy",this.loading?"true":"false"),this.loading||this.animateExpand()}handleDisabledChange(){this.setAttribute("aria-disabled",this.disabled?"true":"false")}handleSelectedChange(){this.setAttribute("aria-selected",this.selected?"true":"false")}handleExpandedChange(){this.isLeaf?this.removeAttribute("aria-expanded"):this.setAttribute("aria-expanded",this.expanded?"true":"false")}handleExpandAnimation(){this.expanded?this.lazy?(this.loading=!0,this.emit("sl-lazy-load")):this.animateExpand():this.animateCollapse()}handleLazyChange(){this.emit("sl-lazy-change")}getChildrenItems({includeDisabled:t=!0}={}){return this.childrenSlot?[...this.childrenSlot.assignedElements({flatten:!0})].filter(o=>yr.isTreeItem(o)&&(t||!o.disabled)):[]}render(){const t=this.localize.dir()==="rtl",o=!this.loading&&(!this.isLeaf||this.lazy);return x`
      <div
        part="base"
        class="${R({"tree-item":!0,"tree-item--expanded":this.expanded,"tree-item--selected":this.selected,"tree-item--disabled":this.disabled,"tree-item--leaf":this.isLeaf,"tree-item--has-expand-button":o,"tree-item--rtl":this.localize.dir()==="rtl"})}"
      >
        <div
          class="tree-item__item"
          part="
            item
            ${this.disabled?"item--disabled":""}
            ${this.expanded?"item--expanded":""}
            ${this.indeterminate?"item--indeterminate":""}
            ${this.selected?"item--selected":""}
          "
        >
          <div class="tree-item__indentation" part="indentation"></div>

          <div
            part="expand-button"
            class=${R({"tree-item__expand-button":!0,"tree-item__expand-button--visible":o})}
            aria-hidden="true"
          >
            ${fn(this.loading,()=>x` <sl-spinner part="spinner" exportparts="base:spinner__base"></sl-spinner> `)}
            <slot class="tree-item__expand-icon-slot" name="expand-icon">
              <sl-icon library="system" name=${t?"chevron-left":"chevron-right"}></sl-icon>
            </slot>
            <slot class="tree-item__expand-icon-slot" name="collapse-icon">
              <sl-icon library="system" name=${t?"chevron-left":"chevron-right"}></sl-icon>
            </slot>
          </div>

          ${fn(this.selectable,()=>x`
              <sl-checkbox
                part="checkbox"
                exportparts="
                    base:checkbox__base,
                    control:checkbox__control,
                    control--checked:checkbox__control--checked,
                    control--indeterminate:checkbox__control--indeterminate,
                    checked-icon:checkbox__checked-icon,
                    indeterminate-icon:checkbox__indeterminate-icon,
                    label:checkbox__label
                  "
                class="tree-item__checkbox"
                ?disabled="${this.disabled}"
                ?checked="${io(this.selected)}"
                ?indeterminate="${this.indeterminate}"
                tabindex="-1"
              ></sl-checkbox>
            `)}

          <slot class="tree-item__label" part="label"></slot>
        </div>

        <div class="tree-item__children" part="children" role="group">
          <slot name="children" @slotchange="${this.handleChildrenSlotChange}"></slot>
        </div>
      </div>
    `}};lt.styles=[D,Tu];lt.dependencies={"sl-checkbox":mt,"sl-icon":J,"sl-spinner":Di};l([z()],lt.prototype,"indeterminate",2);l([z()],lt.prototype,"isLeaf",2);l([z()],lt.prototype,"loading",2);l([z()],lt.prototype,"selectable",2);l([p({type:Boolean,reflect:!0})],lt.prototype,"expanded",2);l([p({type:Boolean,reflect:!0})],lt.prototype,"selected",2);l([p({type:Boolean,reflect:!0})],lt.prototype,"disabled",2);l([p({type:Boolean,reflect:!0})],lt.prototype,"lazy",2);l([S("slot:not([name])")],lt.prototype,"defaultSlot",2);l([S("slot[name=children]")],lt.prototype,"childrenSlot",2);l([S(".tree-item__item")],lt.prototype,"itemElement",2);l([S(".tree-item__children")],lt.prototype,"childrenContainer",2);l([S(".tree-item__expand-button slot")],lt.prototype,"expandButtonSlot",2);l([C("loading",{waitUntilFirstUpdate:!0})],lt.prototype,"handleLoadingChange",1);l([C("disabled")],lt.prototype,"handleDisabledChange",1);l([C("selected")],lt.prototype,"handleSelectedChange",1);l([C("expanded",{waitUntilFirstUpdate:!0})],lt.prototype,"handleExpandedChange",1);l([C("expanded",{waitUntilFirstUpdate:!0})],lt.prototype,"handleExpandAnimation",1);l([C("lazy",{waitUntilFirstUpdate:!0})],lt.prototype,"handleLazyChange",1);var li=lt;K("tree-item.expand",{keyframes:[{height:"0",opacity:"0",overflow:"hidden"},{height:"auto",opacity:"1",overflow:"hidden"}],options:{duration:250,easing:"cubic-bezier(0.4, 0.0, 0.2, 1)"}});K("tree-item.collapse",{keyframes:[{height:"auto",opacity:"1",overflow:"hidden"},{height:"0",opacity:"0",overflow:"hidden"}],options:{duration:200,easing:"cubic-bezier(0.4, 0.0, 0.2, 1)"}});li.define("sl-tree-item");var Qu=O`
  :host {
    --max-width: 20rem;
    --hide-delay: 0ms;
    --show-delay: 150ms;

    display: contents;
  }

  .tooltip {
    --arrow-size: var(--sl-tooltip-arrow-size);
    --arrow-color: var(--sl-tooltip-background-color);
  }

  .tooltip::part(popup) {
    z-index: var(--sl-z-index-tooltip);
  }

  .tooltip[placement^='top']::part(popup) {
    transform-origin: bottom;
  }

  .tooltip[placement^='bottom']::part(popup) {
    transform-origin: top;
  }

  .tooltip[placement^='left']::part(popup) {
    transform-origin: right;
  }

  .tooltip[placement^='right']::part(popup) {
    transform-origin: left;
  }

  .tooltip__body {
    display: block;
    width: max-content;
    max-width: var(--max-width);
    border-radius: var(--sl-tooltip-border-radius);
    background-color: var(--sl-tooltip-background-color);
    font-family: var(--sl-tooltip-font-family);
    font-size: var(--sl-tooltip-font-size);
    font-weight: var(--sl-tooltip-font-weight);
    line-height: var(--sl-tooltip-line-height);
    text-align: start;
    white-space: normal;
    color: var(--sl-tooltip-color);
    padding: var(--sl-tooltip-padding);
    pointer-events: none;
    user-select: none;
    -webkit-user-select: none;
  }
`,Zu=O`
  :host {
    --arrow-color: var(--sl-color-neutral-1000);
    --arrow-size: 6px;

    /*
     * These properties are computed to account for the arrow's dimensions after being rotated 45º. The constant
     * 0.7071 is derived from sin(45), which is the diagonal size of the arrow's container after rotating.
     */
    --arrow-size-diagonal: calc(var(--arrow-size) * 0.7071);
    --arrow-padding-offset: calc(var(--arrow-size-diagonal) - var(--arrow-size));

    display: contents;
  }

  .popup {
    position: absolute;
    isolation: isolate;
    max-width: var(--auto-size-available-width, none);
    max-height: var(--auto-size-available-height, none);
  }

  .popup--fixed {
    position: fixed;
  }

  .popup:not(.popup--active) {
    display: none;
  }

  .popup__arrow {
    position: absolute;
    width: calc(var(--arrow-size-diagonal) * 2);
    height: calc(var(--arrow-size-diagonal) * 2);
    rotate: 45deg;
    background: var(--arrow-color);
    z-index: -1;
  }

  /* Hover bridge */
  .popup-hover-bridge:not(.popup-hover-bridge--visible) {
    display: none;
  }

  .popup-hover-bridge {
    position: fixed;
    z-index: calc(var(--sl-z-index-dropdown) - 1);
    top: 0;
    right: 0;
    bottom: 0;
    left: 0;
    clip-path: polygon(
      var(--hover-bridge-top-left-x, 0) var(--hover-bridge-top-left-y, 0),
      var(--hover-bridge-top-right-x, 0) var(--hover-bridge-top-right-y, 0),
      var(--hover-bridge-bottom-right-x, 0) var(--hover-bridge-bottom-right-y, 0),
      var(--hover-bridge-bottom-left-x, 0) var(--hover-bridge-bottom-left-y, 0)
    );
  }
`;const Be=Math.min,It=Math.max,vs=Math.round,qi=Math.floor,ue=e=>({x:e,y:e}),Gu={left:"right",right:"left",bottom:"top",top:"bottom"},Ju={start:"end",end:"start"};function wr(e,t,o){return It(e,Be(t,o))}function Bo(e,t){return typeof e=="function"?e(t):e}function Fe(e){return e.split("-")[0]}function Fo(e){return e.split("-")[1]}function Bl(e){return e==="x"?"y":"x"}function na(e){return e==="y"?"height":"width"}function so(e){return["top","bottom"].includes(Fe(e))?"y":"x"}function la(e){return Bl(so(e))}function tp(e,t,o){o===void 0&&(o=!1);const i=Fo(e),s=la(e),r=na(s);let a=s==="x"?i===(o?"end":"start")?"right":"left":i==="start"?"bottom":"top";return t.reference[r]>t.floating[r]&&(a=ys(a)),[a,ys(a)]}function ep(e){const t=ys(e);return[xr(e),t,xr(t)]}function xr(e){return e.replace(/start|end/g,t=>Ju[t])}function op(e,t,o){const i=["left","right"],s=["right","left"],r=["top","bottom"],a=["bottom","top"];switch(e){case"top":case"bottom":return o?t?s:i:t?i:s;case"left":case"right":return t?r:a;default:return[]}}function ip(e,t,o,i){const s=Fo(e);let r=op(Fe(e),o==="start",i);return s&&(r=r.map(a=>a+"-"+s),t&&(r=r.concat(r.map(xr)))),r}function ys(e){return e.replace(/left|right|bottom|top/g,t=>Gu[t])}function sp(e){return{top:0,right:0,bottom:0,left:0,...e}}function Fl(e){return typeof e!="number"?sp(e):{top:e,right:e,bottom:e,left:e}}function ws(e){const{x:t,y:o,width:i,height:s}=e;return{width:i,height:s,top:o,left:t,right:t+i,bottom:o+s,x:t,y:o}}function mn(e,t,o){let{reference:i,floating:s}=e;const r=so(t),a=la(t),n=na(a),c=Fe(t),d=r==="y",u=i.x+i.width/2-s.width/2,h=i.y+i.height/2-s.height/2,f=i[n]/2-s[n]/2;let m;switch(c){case"top":m={x:u,y:i.y-s.height};break;case"bottom":m={x:u,y:i.y+i.height};break;case"right":m={x:i.x+i.width,y:h};break;case"left":m={x:i.x-s.width,y:h};break;default:m={x:i.x,y:i.y}}switch(Fo(t)){case"start":m[a]-=f*(o&&d?-1:1);break;case"end":m[a]+=f*(o&&d?-1:1);break}return m}const rp=async(e,t,o)=>{const{placement:i="bottom",strategy:s="absolute",middleware:r=[],platform:a}=o,n=r.filter(Boolean),c=await(a.isRTL==null?void 0:a.isRTL(t));let d=await a.getElementRects({reference:e,floating:t,strategy:s}),{x:u,y:h}=mn(d,i,c),f=i,m={},g=0;for(let b=0;b<n.length;b++){const{name:k,fn:$}=n[b],{x:w,y:_,data:v,reset:y}=await $({x:u,y:h,initialPlacement:i,placement:f,strategy:s,middlewareData:m,rects:d,platform:a,elements:{reference:e,floating:t}});u=w??u,h=_??h,m={...m,[k]:{...m[k],...v}},y&&g<=50&&(g++,typeof y=="object"&&(y.placement&&(f=y.placement),y.rects&&(d=y.rects===!0?await a.getElementRects({reference:e,floating:t,strategy:s}):y.rects),{x:u,y:h}=mn(d,f,c)),b=-1)}return{x:u,y:h,placement:f,strategy:s,middlewareData:m}};async function ca(e,t){var o;t===void 0&&(t={});const{x:i,y:s,platform:r,rects:a,elements:n,strategy:c}=e,{boundary:d="clippingAncestors",rootBoundary:u="viewport",elementContext:h="floating",altBoundary:f=!1,padding:m=0}=Bo(t,e),g=Fl(m),k=n[f?h==="floating"?"reference":"floating":h],$=ws(await r.getClippingRect({element:(o=await(r.isElement==null?void 0:r.isElement(k)))==null||o?k:k.contextElement||await(r.getDocumentElement==null?void 0:r.getDocumentElement(n.floating)),boundary:d,rootBoundary:u,strategy:c})),w=h==="floating"?{x:i,y:s,width:a.floating.width,height:a.floating.height}:a.reference,_=await(r.getOffsetParent==null?void 0:r.getOffsetParent(n.floating)),v=await(r.isElement==null?void 0:r.isElement(_))?await(r.getScale==null?void 0:r.getScale(_))||{x:1,y:1}:{x:1,y:1},y=ws(r.convertOffsetParentRelativeRectToViewportRelativeRect?await r.convertOffsetParentRelativeRectToViewportRelativeRect({elements:n,rect:w,offsetParent:_,strategy:c}):w);return{top:($.top-y.top+g.top)/v.y,bottom:(y.bottom-$.bottom+g.bottom)/v.y,left:($.left-y.left+g.left)/v.x,right:(y.right-$.right+g.right)/v.x}}const ap=e=>({name:"arrow",options:e,async fn(t){const{x:o,y:i,placement:s,rects:r,platform:a,elements:n,middlewareData:c}=t,{element:d,padding:u=0}=Bo(e,t)||{};if(d==null)return{};const h=Fl(u),f={x:o,y:i},m=la(s),g=na(m),b=await a.getDimensions(d),k=m==="y",$=k?"top":"left",w=k?"bottom":"right",_=k?"clientHeight":"clientWidth",v=r.reference[g]+r.reference[m]-f[m]-r.floating[g],y=f[m]-r.reference[m],P=await(a.getOffsetParent==null?void 0:a.getOffsetParent(d));let M=P?P[_]:0;(!M||!await(a.isElement==null?void 0:a.isElement(P)))&&(M=n.floating[_]||r.floating[g]);const I=v/2-y/2,L=M/2-b[g]/2-1,A=Be(h[$],L),Z=Be(h[w],L),tt=A,dt=M-b[g]-Z,it=M/2-b[g]/2+I,pt=wr(tt,it,dt),Lt=!c.arrow&&Fo(s)!=null&&it!==pt&&r.reference[g]/2-(it<tt?A:Z)-b[g]/2<0,At=Lt?it<tt?it-tt:it-dt:0;return{[m]:f[m]+At,data:{[m]:pt,centerOffset:it-pt-At,...Lt&&{alignmentOffset:At}},reset:Lt}}}),np=function(e){return e===void 0&&(e={}),{name:"flip",options:e,async fn(t){var o,i;const{placement:s,middlewareData:r,rects:a,initialPlacement:n,platform:c,elements:d}=t,{mainAxis:u=!0,crossAxis:h=!0,fallbackPlacements:f,fallbackStrategy:m="bestFit",fallbackAxisSideDirection:g="none",flipAlignment:b=!0,...k}=Bo(e,t);if((o=r.arrow)!=null&&o.alignmentOffset)return{};const $=Fe(s),w=so(n),_=Fe(n)===n,v=await(c.isRTL==null?void 0:c.isRTL(d.floating)),y=f||(_||!b?[ys(n)]:ep(n)),P=g!=="none";!f&&P&&y.push(...ip(n,b,g,v));const M=[n,...y],I=await ca(t,k),L=[];let A=((i=r.flip)==null?void 0:i.overflows)||[];if(u&&L.push(I[$]),h){const it=tp(s,a,v);L.push(I[it[0]],I[it[1]])}if(A=[...A,{placement:s,overflows:L}],!L.every(it=>it<=0)){var Z,tt;const it=(((Z=r.flip)==null?void 0:Z.index)||0)+1,pt=M[it];if(pt)return{data:{index:it,overflows:A},reset:{placement:pt}};let Lt=(tt=A.filter(At=>At.overflows[0]<=0).sort((At,kt)=>At.overflows[1]-kt.overflows[1])[0])==null?void 0:tt.placement;if(!Lt)switch(m){case"bestFit":{var dt;const At=(dt=A.filter(kt=>{if(P){const ft=so(kt.placement);return ft===w||ft==="y"}return!0}).map(kt=>[kt.placement,kt.overflows.filter(ft=>ft>0).reduce((ft,ne)=>ft+ne,0)]).sort((kt,ft)=>kt[1]-ft[1])[0])==null?void 0:dt[0];At&&(Lt=At);break}case"initialPlacement":Lt=n;break}if(s!==Lt)return{reset:{placement:Lt}}}return{}}}};async function lp(e,t){const{placement:o,platform:i,elements:s}=e,r=await(i.isRTL==null?void 0:i.isRTL(s.floating)),a=Fe(o),n=Fo(o),c=so(o)==="y",d=["left","top"].includes(a)?-1:1,u=r&&c?-1:1,h=Bo(t,e);let{mainAxis:f,crossAxis:m,alignmentAxis:g}=typeof h=="number"?{mainAxis:h,crossAxis:0,alignmentAxis:null}:{mainAxis:h.mainAxis||0,crossAxis:h.crossAxis||0,alignmentAxis:h.alignmentAxis};return n&&typeof g=="number"&&(m=n==="end"?g*-1:g),c?{x:m*u,y:f*d}:{x:f*d,y:m*u}}const cp=function(e){return e===void 0&&(e=0),{name:"offset",options:e,async fn(t){var o,i;const{x:s,y:r,placement:a,middlewareData:n}=t,c=await lp(t,e);return a===((o=n.offset)==null?void 0:o.placement)&&(i=n.arrow)!=null&&i.alignmentOffset?{}:{x:s+c.x,y:r+c.y,data:{...c,placement:a}}}}},dp=function(e){return e===void 0&&(e={}),{name:"shift",options:e,async fn(t){const{x:o,y:i,placement:s}=t,{mainAxis:r=!0,crossAxis:a=!1,limiter:n={fn:k=>{let{x:$,y:w}=k;return{x:$,y:w}}},...c}=Bo(e,t),d={x:o,y:i},u=await ca(t,c),h=so(Fe(s)),f=Bl(h);let m=d[f],g=d[h];if(r){const k=f==="y"?"top":"left",$=f==="y"?"bottom":"right",w=m+u[k],_=m-u[$];m=wr(w,m,_)}if(a){const k=h==="y"?"top":"left",$=h==="y"?"bottom":"right",w=g+u[k],_=g-u[$];g=wr(w,g,_)}const b=n.fn({...t,[f]:m,[h]:g});return{...b,data:{x:b.x-o,y:b.y-i,enabled:{[f]:r,[h]:a}}}}}},hp=function(e){return e===void 0&&(e={}),{name:"size",options:e,async fn(t){var o,i;const{placement:s,rects:r,platform:a,elements:n}=t,{apply:c=()=>{},...d}=Bo(e,t),u=await ca(t,d),h=Fe(s),f=Fo(s),m=so(s)==="y",{width:g,height:b}=r.floating;let k,$;h==="top"||h==="bottom"?(k=h,$=f===(await(a.isRTL==null?void 0:a.isRTL(n.floating))?"start":"end")?"left":"right"):($=h,k=f==="end"?"top":"bottom");const w=b-u.top-u.bottom,_=g-u.left-u.right,v=Be(b-u[k],w),y=Be(g-u[$],_),P=!t.middlewareData.shift;let M=v,I=y;if((o=t.middlewareData.shift)!=null&&o.enabled.x&&(I=_),(i=t.middlewareData.shift)!=null&&i.enabled.y&&(M=w),P&&!f){const A=It(u.left,0),Z=It(u.right,0),tt=It(u.top,0),dt=It(u.bottom,0);m?I=g-2*(A!==0||Z!==0?A+Z:It(u.left,u.right)):M=b-2*(tt!==0||dt!==0?tt+dt:It(u.top,u.bottom))}await c({...t,availableWidth:I,availableHeight:M});const L=await a.getDimensions(n.floating);return g!==L.width||b!==L.height?{reset:{rects:!0}}:{}}}};function Ls(){return typeof window<"u"}function Vo(e){return Vl(e)?(e.nodeName||"").toLowerCase():"#document"}function Vt(e){var t;return(e==null||(t=e.ownerDocument)==null?void 0:t.defaultView)||window}function ge(e){var t;return(t=(Vl(e)?e.ownerDocument:e.document)||window.document)==null?void 0:t.documentElement}function Vl(e){return Ls()?e instanceof Node||e instanceof Vt(e).Node:!1}function Jt(e){return Ls()?e instanceof Element||e instanceof Vt(e).Element:!1}function me(e){return Ls()?e instanceof HTMLElement||e instanceof Vt(e).HTMLElement:!1}function gn(e){return!Ls()||typeof ShadowRoot>"u"?!1:e instanceof ShadowRoot||e instanceof Vt(e).ShadowRoot}function Mi(e){const{overflow:t,overflowX:o,overflowY:i,display:s}=te(e);return/auto|scroll|overlay|hidden|clip/.test(t+i+o)&&!["inline","contents"].includes(s)}function up(e){return["table","td","th"].includes(Vo(e))}function Rs(e){return[":popover-open",":modal"].some(t=>{try{return e.matches(t)}catch{return!1}})}function Ds(e){const t=da(),o=Jt(e)?te(e):e;return["transform","translate","scale","rotate","perspective"].some(i=>o[i]?o[i]!=="none":!1)||(o.containerType?o.containerType!=="normal":!1)||!t&&(o.backdropFilter?o.backdropFilter!=="none":!1)||!t&&(o.filter?o.filter!=="none":!1)||["transform","translate","scale","rotate","perspective","filter"].some(i=>(o.willChange||"").includes(i))||["paint","layout","strict","content"].some(i=>(o.contain||"").includes(i))}function pp(e){let t=Ve(e);for(;me(t)&&!To(t);){if(Ds(t))return t;if(Rs(t))return null;t=Ve(t)}return null}function da(){return typeof CSS>"u"||!CSS.supports?!1:CSS.supports("-webkit-backdrop-filter","none")}function To(e){return["html","body","#document"].includes(Vo(e))}function te(e){return Vt(e).getComputedStyle(e)}function Ms(e){return Jt(e)?{scrollLeft:e.scrollLeft,scrollTop:e.scrollTop}:{scrollLeft:e.scrollX,scrollTop:e.scrollY}}function Ve(e){if(Vo(e)==="html")return e;const t=e.assignedSlot||e.parentNode||gn(e)&&e.host||ge(e);return gn(t)?t.host:t}function Ul(e){const t=Ve(e);return To(t)?e.ownerDocument?e.ownerDocument.body:e.body:me(t)&&Mi(t)?t:Ul(t)}function _i(e,t,o){var i;t===void 0&&(t=[]),o===void 0&&(o=!0);const s=Ul(e),r=s===((i=e.ownerDocument)==null?void 0:i.body),a=Vt(s);if(r){const n=kr(a);return t.concat(a,a.visualViewport||[],Mi(s)?s:[],n&&o?_i(n):[])}return t.concat(s,_i(s,[],o))}function kr(e){return e.parent&&Object.getPrototypeOf(e.parent)?e.frameElement:null}function Nl(e){const t=te(e);let o=parseFloat(t.width)||0,i=parseFloat(t.height)||0;const s=me(e),r=s?e.offsetWidth:o,a=s?e.offsetHeight:i,n=vs(o)!==r||vs(i)!==a;return n&&(o=r,i=a),{width:o,height:i,$:n}}function ha(e){return Jt(e)?e:e.contextElement}function _o(e){const t=ha(e);if(!me(t))return ue(1);const o=t.getBoundingClientRect(),{width:i,height:s,$:r}=Nl(t);let a=(r?vs(o.width):o.width)/i,n=(r?vs(o.height):o.height)/s;return(!a||!Number.isFinite(a))&&(a=1),(!n||!Number.isFinite(n))&&(n=1),{x:a,y:n}}const fp=ue(0);function Hl(e){const t=Vt(e);return!da()||!t.visualViewport?fp:{x:t.visualViewport.offsetLeft,y:t.visualViewport.offsetTop}}function mp(e,t,o){return t===void 0&&(t=!1),!o||t&&o!==Vt(e)?!1:t}function ro(e,t,o,i){t===void 0&&(t=!1),o===void 0&&(o=!1);const s=e.getBoundingClientRect(),r=ha(e);let a=ue(1);t&&(i?Jt(i)&&(a=_o(i)):a=_o(e));const n=mp(r,o,i)?Hl(r):ue(0);let c=(s.left+n.x)/a.x,d=(s.top+n.y)/a.y,u=s.width/a.x,h=s.height/a.y;if(r){const f=Vt(r),m=i&&Jt(i)?Vt(i):i;let g=f,b=kr(g);for(;b&&i&&m!==g;){const k=_o(b),$=b.getBoundingClientRect(),w=te(b),_=$.left+(b.clientLeft+parseFloat(w.paddingLeft))*k.x,v=$.top+(b.clientTop+parseFloat(w.paddingTop))*k.y;c*=k.x,d*=k.y,u*=k.x,h*=k.y,c+=_,d+=v,g=Vt(b),b=kr(g)}}return ws({width:u,height:h,x:c,y:d})}function ua(e,t){const o=Ms(e).scrollLeft;return t?t.left+o:ro(ge(e)).left+o}function jl(e,t,o){o===void 0&&(o=!1);const i=e.getBoundingClientRect(),s=i.left+t.scrollLeft-(o?0:ua(e,i)),r=i.top+t.scrollTop;return{x:s,y:r}}function gp(e){let{elements:t,rect:o,offsetParent:i,strategy:s}=e;const r=s==="fixed",a=ge(i),n=t?Rs(t.floating):!1;if(i===a||n&&r)return o;let c={scrollLeft:0,scrollTop:0},d=ue(1);const u=ue(0),h=me(i);if((h||!h&&!r)&&((Vo(i)!=="body"||Mi(a))&&(c=Ms(i)),me(i))){const m=ro(i);d=_o(i),u.x=m.x+i.clientLeft,u.y=m.y+i.clientTop}const f=a&&!h&&!r?jl(a,c,!0):ue(0);return{width:o.width*d.x,height:o.height*d.y,x:o.x*d.x-c.scrollLeft*d.x+u.x+f.x,y:o.y*d.y-c.scrollTop*d.y+u.y+f.y}}function bp(e){return Array.from(e.getClientRects())}function vp(e){const t=ge(e),o=Ms(e),i=e.ownerDocument.body,s=It(t.scrollWidth,t.clientWidth,i.scrollWidth,i.clientWidth),r=It(t.scrollHeight,t.clientHeight,i.scrollHeight,i.clientHeight);let a=-o.scrollLeft+ua(e);const n=-o.scrollTop;return te(i).direction==="rtl"&&(a+=It(t.clientWidth,i.clientWidth)-s),{width:s,height:r,x:a,y:n}}function yp(e,t){const o=Vt(e),i=ge(e),s=o.visualViewport;let r=i.clientWidth,a=i.clientHeight,n=0,c=0;if(s){r=s.width,a=s.height;const d=da();(!d||d&&t==="fixed")&&(n=s.offsetLeft,c=s.offsetTop)}return{width:r,height:a,x:n,y:c}}function wp(e,t){const o=ro(e,!0,t==="fixed"),i=o.top+e.clientTop,s=o.left+e.clientLeft,r=me(e)?_o(e):ue(1),a=e.clientWidth*r.x,n=e.clientHeight*r.y,c=s*r.x,d=i*r.y;return{width:a,height:n,x:c,y:d}}function bn(e,t,o){let i;if(t==="viewport")i=yp(e,o);else if(t==="document")i=vp(ge(e));else if(Jt(t))i=wp(t,o);else{const s=Hl(e);i={x:t.x-s.x,y:t.y-s.y,width:t.width,height:t.height}}return ws(i)}function ql(e,t){const o=Ve(e);return o===t||!Jt(o)||To(o)?!1:te(o).position==="fixed"||ql(o,t)}function xp(e,t){const o=t.get(e);if(o)return o;let i=_i(e,[],!1).filter(n=>Jt(n)&&Vo(n)!=="body"),s=null;const r=te(e).position==="fixed";let a=r?Ve(e):e;for(;Jt(a)&&!To(a);){const n=te(a),c=Ds(a);!c&&n.position==="fixed"&&(s=null),(r?!c&&!s:!c&&n.position==="static"&&!!s&&["absolute","fixed"].includes(s.position)||Mi(a)&&!c&&ql(e,a))?i=i.filter(u=>u!==a):s=n,a=Ve(a)}return t.set(e,i),i}function kp(e){let{element:t,boundary:o,rootBoundary:i,strategy:s}=e;const a=[...o==="clippingAncestors"?Rs(t)?[]:xp(t,this._c):[].concat(o),i],n=a[0],c=a.reduce((d,u)=>{const h=bn(t,u,s);return d.top=It(h.top,d.top),d.right=Be(h.right,d.right),d.bottom=Be(h.bottom,d.bottom),d.left=It(h.left,d.left),d},bn(t,n,s));return{width:c.right-c.left,height:c.bottom-c.top,x:c.left,y:c.top}}function _p(e){const{width:t,height:o}=Nl(e);return{width:t,height:o}}function $p(e,t,o){const i=me(t),s=ge(t),r=o==="fixed",a=ro(e,!0,r,t);let n={scrollLeft:0,scrollTop:0};const c=ue(0);if(i||!i&&!r)if((Vo(t)!=="body"||Mi(s))&&(n=Ms(t)),i){const f=ro(t,!0,r,t);c.x=f.x+t.clientLeft,c.y=f.y+t.clientTop}else s&&(c.x=ua(s));const d=s&&!i&&!r?jl(s,n):ue(0),u=a.left+n.scrollLeft-c.x-d.x,h=a.top+n.scrollTop-c.y-d.y;return{x:u,y:h,width:a.width,height:a.height}}function tr(e){return te(e).position==="static"}function vn(e,t){if(!me(e)||te(e).position==="fixed")return null;if(t)return t(e);let o=e.offsetParent;return ge(e)===o&&(o=o.ownerDocument.body),o}function Wl(e,t){const o=Vt(e);if(Rs(e))return o;if(!me(e)){let s=Ve(e);for(;s&&!To(s);){if(Jt(s)&&!tr(s))return s;s=Ve(s)}return o}let i=vn(e,t);for(;i&&up(i)&&tr(i);)i=vn(i,t);return i&&To(i)&&tr(i)&&!Ds(i)?o:i||pp(e)||o}const Cp=async function(e){const t=this.getOffsetParent||Wl,o=this.getDimensions,i=await o(e.floating);return{reference:$p(e.reference,await t(e.floating),e.strategy),floating:{x:0,y:0,width:i.width,height:i.height}}};function Sp(e){return te(e).direction==="rtl"}const ts={convertOffsetParentRelativeRectToViewportRelativeRect:gp,getDocumentElement:ge,getClippingRect:kp,getOffsetParent:Wl,getElementRects:Cp,getClientRects:bp,getDimensions:_p,getScale:_o,isElement:Jt,isRTL:Sp};function Kl(e,t){return e.x===t.x&&e.y===t.y&&e.width===t.width&&e.height===t.height}function Ap(e,t){let o=null,i;const s=ge(e);function r(){var n;clearTimeout(i),(n=o)==null||n.disconnect(),o=null}function a(n,c){n===void 0&&(n=!1),c===void 0&&(c=1),r();const d=e.getBoundingClientRect(),{left:u,top:h,width:f,height:m}=d;if(n||t(),!f||!m)return;const g=qi(h),b=qi(s.clientWidth-(u+f)),k=qi(s.clientHeight-(h+m)),$=qi(u),_={rootMargin:-g+"px "+-b+"px "+-k+"px "+-$+"px",threshold:It(0,Be(1,c))||1};let v=!0;function y(P){const M=P[0].intersectionRatio;if(M!==c){if(!v)return a();M?a(!1,M):i=setTimeout(()=>{a(!1,1e-7)},1e3)}M===1&&!Kl(d,e.getBoundingClientRect())&&a(),v=!1}try{o=new IntersectionObserver(y,{..._,root:s.ownerDocument})}catch{o=new IntersectionObserver(y,_)}o.observe(e)}return a(!0),r}function zp(e,t,o,i){i===void 0&&(i={});const{ancestorScroll:s=!0,ancestorResize:r=!0,elementResize:a=typeof ResizeObserver=="function",layoutShift:n=typeof IntersectionObserver=="function",animationFrame:c=!1}=i,d=ha(e),u=s||r?[...d?_i(d):[],..._i(t)]:[];u.forEach($=>{s&&$.addEventListener("scroll",o,{passive:!0}),r&&$.addEventListener("resize",o)});const h=d&&n?Ap(d,o):null;let f=-1,m=null;a&&(m=new ResizeObserver($=>{let[w]=$;w&&w.target===d&&m&&(m.unobserve(t),cancelAnimationFrame(f),f=requestAnimationFrame(()=>{var _;(_=m)==null||_.observe(t)})),o()}),d&&!c&&m.observe(d),m.observe(t));let g,b=c?ro(e):null;c&&k();function k(){const $=ro(e);b&&!Kl(b,$)&&o(),b=$,g=requestAnimationFrame(k)}return o(),()=>{var $;u.forEach(w=>{s&&w.removeEventListener("scroll",o),r&&w.removeEventListener("resize",o)}),h==null||h(),($=m)==null||$.disconnect(),m=null,c&&cancelAnimationFrame(g)}}const Ep=cp,Tp=dp,Pp=np,yn=hp,Op=ap,Lp=(e,t,o)=>{const i=new Map,s={platform:ts,...o},r={...s.platform,_c:i};return rp(e,t,{...s,platform:r})};function Rp(e){return Dp(e)}function er(e){return e.assignedSlot?e.assignedSlot:e.parentNode instanceof ShadowRoot?e.parentNode.host:e.parentNode}function Dp(e){for(let t=e;t;t=er(t))if(t instanceof Element&&getComputedStyle(t).display==="none")return null;for(let t=er(e);t;t=er(t)){if(!(t instanceof Element))continue;const o=getComputedStyle(t);if(o.display!=="contents"&&(o.position!=="static"||Ds(o)||t.tagName==="BODY"))return t}return null}function Mp(e){return e!==null&&typeof e=="object"&&"getBoundingClientRect"in e&&("contextElement"in e?e.contextElement instanceof Element:!0)}var Y=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.active=!1,this.placement="top",this.strategy="absolute",this.distance=0,this.skidding=0,this.arrow=!1,this.arrowPlacement="anchor",this.arrowPadding=10,this.flip=!1,this.flipFallbackPlacements="",this.flipFallbackStrategy="best-fit",this.flipPadding=0,this.shift=!1,this.shiftPadding=0,this.autoSizePadding=0,this.hoverBridge=!1,this.updateHoverBridge=()=>{if(this.hoverBridge&&this.anchorEl){const e=this.anchorEl.getBoundingClientRect(),t=this.popup.getBoundingClientRect(),o=this.placement.includes("top")||this.placement.includes("bottom");let i=0,s=0,r=0,a=0,n=0,c=0,d=0,u=0;o?e.top<t.top?(i=e.left,s=e.bottom,r=e.right,a=e.bottom,n=t.left,c=t.top,d=t.right,u=t.top):(i=t.left,s=t.bottom,r=t.right,a=t.bottom,n=e.left,c=e.top,d=e.right,u=e.top):e.left<t.left?(i=e.right,s=e.top,r=t.left,a=t.top,n=e.right,c=e.bottom,d=t.left,u=t.bottom):(i=t.right,s=t.top,r=e.left,a=e.top,n=t.right,c=t.bottom,d=e.left,u=e.bottom),this.style.setProperty("--hover-bridge-top-left-x",`${i}px`),this.style.setProperty("--hover-bridge-top-left-y",`${s}px`),this.style.setProperty("--hover-bridge-top-right-x",`${r}px`),this.style.setProperty("--hover-bridge-top-right-y",`${a}px`),this.style.setProperty("--hover-bridge-bottom-left-x",`${n}px`),this.style.setProperty("--hover-bridge-bottom-left-y",`${c}px`),this.style.setProperty("--hover-bridge-bottom-right-x",`${d}px`),this.style.setProperty("--hover-bridge-bottom-right-y",`${u}px`)}}}async connectedCallback(){super.connectedCallback(),await this.updateComplete,this.start()}disconnectedCallback(){super.disconnectedCallback(),this.stop()}async updated(e){super.updated(e),e.has("active")&&(this.active?this.start():this.stop()),e.has("anchor")&&this.handleAnchorChange(),this.active&&(await this.updateComplete,this.reposition())}async handleAnchorChange(){if(await this.stop(),this.anchor&&typeof this.anchor=="string"){const e=this.getRootNode();this.anchorEl=e.getElementById(this.anchor)}else this.anchor instanceof Element||Mp(this.anchor)?this.anchorEl=this.anchor:this.anchorEl=this.querySelector('[slot="anchor"]');this.anchorEl instanceof HTMLSlotElement&&(this.anchorEl=this.anchorEl.assignedElements({flatten:!0})[0]),this.anchorEl&&this.active&&this.start()}start(){!this.anchorEl||!this.active||(this.cleanup=zp(this.anchorEl,this.popup,()=>{this.reposition()}))}async stop(){return new Promise(e=>{this.cleanup?(this.cleanup(),this.cleanup=void 0,this.removeAttribute("data-current-placement"),this.style.removeProperty("--auto-size-available-width"),this.style.removeProperty("--auto-size-available-height"),requestAnimationFrame(()=>e())):e()})}reposition(){if(!this.active||!this.anchorEl)return;const e=[Ep({mainAxis:this.distance,crossAxis:this.skidding})];this.sync?e.push(yn({apply:({rects:o})=>{const i=this.sync==="width"||this.sync==="both",s=this.sync==="height"||this.sync==="both";this.popup.style.width=i?`${o.reference.width}px`:"",this.popup.style.height=s?`${o.reference.height}px`:""}})):(this.popup.style.width="",this.popup.style.height=""),this.flip&&e.push(Pp({boundary:this.flipBoundary,fallbackPlacements:this.flipFallbackPlacements,fallbackStrategy:this.flipFallbackStrategy==="best-fit"?"bestFit":"initialPlacement",padding:this.flipPadding})),this.shift&&e.push(Tp({boundary:this.shiftBoundary,padding:this.shiftPadding})),this.autoSize?e.push(yn({boundary:this.autoSizeBoundary,padding:this.autoSizePadding,apply:({availableWidth:o,availableHeight:i})=>{this.autoSize==="vertical"||this.autoSize==="both"?this.style.setProperty("--auto-size-available-height",`${i}px`):this.style.removeProperty("--auto-size-available-height"),this.autoSize==="horizontal"||this.autoSize==="both"?this.style.setProperty("--auto-size-available-width",`${o}px`):this.style.removeProperty("--auto-size-available-width")}})):(this.style.removeProperty("--auto-size-available-width"),this.style.removeProperty("--auto-size-available-height")),this.arrow&&e.push(Op({element:this.arrowEl,padding:this.arrowPadding}));const t=this.strategy==="absolute"?o=>ts.getOffsetParent(o,Rp):ts.getOffsetParent;Lp(this.anchorEl,this.popup,{placement:this.placement,middleware:e,strategy:this.strategy,platform:Oi(ze({},ts),{getOffsetParent:t})}).then(({x:o,y:i,middlewareData:s,placement:r})=>{const a=this.localize.dir()==="rtl",n={top:"bottom",right:"left",bottom:"top",left:"right"}[r.split("-")[0]];if(this.setAttribute("data-current-placement",r),Object.assign(this.popup.style,{left:`${o}px`,top:`${i}px`}),this.arrow){const c=s.arrow.x,d=s.arrow.y;let u="",h="",f="",m="";if(this.arrowPlacement==="start"){const g=typeof c=="number"?`calc(${this.arrowPadding}px - var(--arrow-padding-offset))`:"";u=typeof d=="number"?`calc(${this.arrowPadding}px - var(--arrow-padding-offset))`:"",h=a?g:"",m=a?"":g}else if(this.arrowPlacement==="end"){const g=typeof c=="number"?`calc(${this.arrowPadding}px - var(--arrow-padding-offset))`:"";h=a?"":g,m=a?g:"",f=typeof d=="number"?`calc(${this.arrowPadding}px - var(--arrow-padding-offset))`:""}else this.arrowPlacement==="center"?(m=typeof c=="number"?"calc(50% - var(--arrow-size-diagonal))":"",u=typeof d=="number"?"calc(50% - var(--arrow-size-diagonal))":""):(m=typeof c=="number"?`${c}px`:"",u=typeof d=="number"?`${d}px`:"");Object.assign(this.arrowEl.style,{top:u,right:h,bottom:f,left:m,[n]:"calc(var(--arrow-size-diagonal) * -1)"})}}),requestAnimationFrame(()=>this.updateHoverBridge()),this.emit("sl-reposition")}render(){return x`
      <slot name="anchor" @slotchange=${this.handleAnchorChange}></slot>

      <span
        part="hover-bridge"
        class=${R({"popup-hover-bridge":!0,"popup-hover-bridge--visible":this.hoverBridge&&this.active})}
      ></span>

      <div
        part="popup"
        class=${R({popup:!0,"popup--active":this.active,"popup--fixed":this.strategy==="fixed","popup--has-arrow":this.arrow})}
      >
        <slot></slot>
        ${this.arrow?x`<div part="arrow" class="popup__arrow" role="presentation"></div>`:""}
      </div>
    `}};Y.styles=[D,Zu];l([S(".popup")],Y.prototype,"popup",2);l([S(".popup__arrow")],Y.prototype,"arrowEl",2);l([p()],Y.prototype,"anchor",2);l([p({type:Boolean,reflect:!0})],Y.prototype,"active",2);l([p({reflect:!0})],Y.prototype,"placement",2);l([p({reflect:!0})],Y.prototype,"strategy",2);l([p({type:Number})],Y.prototype,"distance",2);l([p({type:Number})],Y.prototype,"skidding",2);l([p({type:Boolean})],Y.prototype,"arrow",2);l([p({attribute:"arrow-placement"})],Y.prototype,"arrowPlacement",2);l([p({attribute:"arrow-padding",type:Number})],Y.prototype,"arrowPadding",2);l([p({type:Boolean})],Y.prototype,"flip",2);l([p({attribute:"flip-fallback-placements",converter:{fromAttribute:e=>e.split(" ").map(t=>t.trim()).filter(t=>t!==""),toAttribute:e=>e.join(" ")}})],Y.prototype,"flipFallbackPlacements",2);l([p({attribute:"flip-fallback-strategy"})],Y.prototype,"flipFallbackStrategy",2);l([p({type:Object})],Y.prototype,"flipBoundary",2);l([p({attribute:"flip-padding",type:Number})],Y.prototype,"flipPadding",2);l([p({type:Boolean})],Y.prototype,"shift",2);l([p({type:Object})],Y.prototype,"shiftBoundary",2);l([p({attribute:"shift-padding",type:Number})],Y.prototype,"shiftPadding",2);l([p({attribute:"auto-size"})],Y.prototype,"autoSize",2);l([p()],Y.prototype,"sync",2);l([p({type:Object})],Y.prototype,"autoSizeBoundary",2);l([p({attribute:"auto-size-padding",type:Number})],Y.prototype,"autoSizePadding",2);l([p({attribute:"hover-bridge",type:Boolean})],Y.prototype,"hoverBridge",2);function Pt(e,t){return new Promise(o=>{function i(s){s.target===e&&(e.removeEventListener(t,i),o())}e.addEventListener(t,i)})}var yt=class extends T{constructor(){super(),this.localize=new j(this),this.content="",this.placement="top",this.disabled=!1,this.distance=8,this.open=!1,this.skidding=0,this.trigger="hover focus",this.hoist=!1,this.handleBlur=()=>{this.hasTrigger("focus")&&this.hide()},this.handleClick=()=>{this.hasTrigger("click")&&(this.open?this.hide():this.show())},this.handleFocus=()=>{this.hasTrigger("focus")&&this.show()},this.handleDocumentKeyDown=e=>{e.key==="Escape"&&(e.stopPropagation(),this.hide())},this.handleMouseOver=()=>{if(this.hasTrigger("hover")){const e=pn(getComputedStyle(this).getPropertyValue("--show-delay"));clearTimeout(this.hoverTimeout),this.hoverTimeout=window.setTimeout(()=>this.show(),e)}},this.handleMouseOut=()=>{if(this.hasTrigger("hover")){const e=pn(getComputedStyle(this).getPropertyValue("--hide-delay"));clearTimeout(this.hoverTimeout),this.hoverTimeout=window.setTimeout(()=>this.hide(),e)}},this.addEventListener("blur",this.handleBlur,!0),this.addEventListener("focus",this.handleFocus,!0),this.addEventListener("click",this.handleClick),this.addEventListener("mouseover",this.handleMouseOver),this.addEventListener("mouseout",this.handleMouseOut)}disconnectedCallback(){var e;super.disconnectedCallback(),(e=this.closeWatcher)==null||e.destroy(),document.removeEventListener("keydown",this.handleDocumentKeyDown)}firstUpdated(){this.body.hidden=!this.open,this.open&&(this.popup.active=!0,this.popup.reposition())}hasTrigger(e){return this.trigger.split(" ").includes(e)}async handleOpenChange(){var e,t;if(this.open){if(this.disabled)return;this.emit("sl-show"),"CloseWatcher"in window?((e=this.closeWatcher)==null||e.destroy(),this.closeWatcher=new CloseWatcher,this.closeWatcher.onclose=()=>{this.hide()}):document.addEventListener("keydown",this.handleDocumentKeyDown),await ut(this.body),this.body.hidden=!1,this.popup.active=!0;const{keyframes:o,options:i}=ot(this,"tooltip.show",{dir:this.localize.dir()});await at(this.popup.popup,o,i),this.popup.reposition(),this.emit("sl-after-show")}else{this.emit("sl-hide"),(t=this.closeWatcher)==null||t.destroy(),document.removeEventListener("keydown",this.handleDocumentKeyDown),await ut(this.body);const{keyframes:o,options:i}=ot(this,"tooltip.hide",{dir:this.localize.dir()});await at(this.popup.popup,o,i),this.popup.active=!1,this.body.hidden=!0,this.emit("sl-after-hide")}}async handleOptionsChange(){this.hasUpdated&&(await this.updateComplete,this.popup.reposition())}handleDisabledChange(){this.disabled&&this.open&&this.hide()}async show(){if(!this.open)return this.open=!0,Pt(this,"sl-after-show")}async hide(){if(this.open)return this.open=!1,Pt(this,"sl-after-hide")}render(){return x`
      <sl-popup
        part="base"
        exportparts="
          popup:base__popup,
          arrow:base__arrow
        "
        class=${R({tooltip:!0,"tooltip--open":this.open})}
        placement=${this.placement}
        distance=${this.distance}
        skidding=${this.skidding}
        strategy=${this.hoist?"fixed":"absolute"}
        flip
        shift
        arrow
        hover-bridge
      >
        ${""}
        <slot slot="anchor" aria-describedby="tooltip"></slot>

        ${""}
        <div part="body" id="tooltip" class="tooltip__body" role="tooltip" aria-live=${this.open?"polite":"off"}>
          <slot name="content">${this.content}</slot>
        </div>
      </sl-popup>
    `}};yt.styles=[D,Qu];yt.dependencies={"sl-popup":Y};l([S("slot:not([name])")],yt.prototype,"defaultSlot",2);l([S(".tooltip__body")],yt.prototype,"body",2);l([S("sl-popup")],yt.prototype,"popup",2);l([p()],yt.prototype,"content",2);l([p()],yt.prototype,"placement",2);l([p({type:Boolean,reflect:!0})],yt.prototype,"disabled",2);l([p({type:Number})],yt.prototype,"distance",2);l([p({type:Boolean,reflect:!0})],yt.prototype,"open",2);l([p({type:Number})],yt.prototype,"skidding",2);l([p()],yt.prototype,"trigger",2);l([p({type:Boolean})],yt.prototype,"hoist",2);l([C("open",{waitUntilFirstUpdate:!0})],yt.prototype,"handleOpenChange",1);l([C(["content","distance","hoist","placement","skidding"])],yt.prototype,"handleOptionsChange",1);l([C("disabled")],yt.prototype,"handleDisabledChange",1);K("tooltip.show",{keyframes:[{opacity:0,scale:.8},{opacity:1,scale:1}],options:{duration:150,easing:"ease"}});K("tooltip.hide",{keyframes:[{opacity:1,scale:1},{opacity:0,scale:.8}],options:{duration:150,easing:"ease"}});yt.define("sl-tooltip");var Ip=O`
  :host {
    /*
     * These are actually used by tree item, but we define them here so they can more easily be set and all tree items
     * stay consistent.
     */
    --indent-guide-color: var(--sl-color-neutral-200);
    --indent-guide-offset: 0;
    --indent-guide-style: solid;
    --indent-guide-width: 0;
    --indent-size: var(--sl-spacing-large);

    display: block;

    /*
     * Tree item indentation uses the "em" unit to increment its width on each level, so setting the font size to zero
     * here removes the indentation for all the nodes on the first level.
     */
    font-size: 0;
  }
`;function ht(e,t,o){const i=s=>Object.is(s,-0)?0:s;return e<t?i(t):e>o?i(o):i(e)}function wn(e,t=!1){function o(r){const a=r.getChildrenItems({includeDisabled:!1});if(a.length){const n=a.every(d=>d.selected),c=a.every(d=>!d.selected&&!d.indeterminate);r.selected=n,r.indeterminate=!n&&!c}}function i(r){const a=r.parentElement;li.isTreeItem(a)&&(o(a),i(a))}function s(r){for(const a of r.getChildrenItems())a.selected=t?r.selected||a.selected:!a.disabled&&r.selected,s(a);t&&o(r)}s(e),i(e)}var po=class extends T{constructor(){super(),this.selection="single",this.clickTarget=null,this.localize=new j(this),this.initTreeItem=e=>{e.selectable=this.selection==="multiple",["expand","collapse"].filter(t=>!!this.querySelector(`[slot="${t}-icon"]`)).forEach(t=>{const o=e.querySelector(`[slot="${t}-icon"]`),i=this.getExpandButtonIcon(t);i&&(o===null?e.append(i):o.hasAttribute("data-default")&&o.replaceWith(i))})},this.handleTreeChanged=e=>{for(const t of e){const o=[...t.addedNodes].filter(li.isTreeItem),i=[...t.removedNodes].filter(li.isTreeItem);o.forEach(this.initTreeItem),this.lastFocusedItem&&i.includes(this.lastFocusedItem)&&(this.lastFocusedItem=null)}},this.handleFocusOut=e=>{const t=e.relatedTarget;(!t||!this.contains(t))&&(this.tabIndex=0)},this.handleFocusIn=e=>{const t=e.target;e.target===this&&this.focusItem(this.lastFocusedItem||this.getAllTreeItems()[0]),li.isTreeItem(t)&&!t.disabled&&(this.lastFocusedItem&&(this.lastFocusedItem.tabIndex=-1),this.lastFocusedItem=t,this.tabIndex=-1,t.tabIndex=0)},this.addEventListener("focusin",this.handleFocusIn),this.addEventListener("focusout",this.handleFocusOut),this.addEventListener("sl-lazy-change",this.handleSlotChange)}async connectedCallback(){super.connectedCallback(),this.setAttribute("role","tree"),this.setAttribute("tabindex","0"),await this.updateComplete,this.mutationObserver=new MutationObserver(this.handleTreeChanged),this.mutationObserver.observe(this,{childList:!0,subtree:!0})}disconnectedCallback(){var e;super.disconnectedCallback(),(e=this.mutationObserver)==null||e.disconnect()}getExpandButtonIcon(e){const o=(e==="expand"?this.expandedIconSlot:this.collapsedIconSlot).assignedElements({flatten:!0})[0];if(o){const i=o.cloneNode(!0);return[i,...i.querySelectorAll("[id]")].forEach(s=>s.removeAttribute("id")),i.setAttribute("data-default",""),i.slot=`${e}-icon`,i}return null}selectItem(e){const t=[...this.selectedItems];if(this.selection==="multiple")e.selected=!e.selected,e.lazy&&(e.expanded=!0),wn(e);else if(this.selection==="single"||e.isLeaf){const i=this.getAllTreeItems();for(const s of i)s.selected=s===e}else this.selection==="leaf"&&(e.expanded=!e.expanded);const o=this.selectedItems;(t.length!==o.length||o.some(i=>!t.includes(i)))&&Promise.all(o.map(i=>i.updateComplete)).then(()=>{this.emit("sl-selection-change",{detail:{selection:o}})})}getAllTreeItems(){return[...this.querySelectorAll("sl-tree-item")]}focusItem(e){e==null||e.focus()}handleKeyDown(e){if(!["ArrowDown","ArrowUp","ArrowRight","ArrowLeft","Home","End","Enter"," "].includes(e.key)||e.composedPath().some(s=>{var r;return["input","textarea"].includes((r=s==null?void 0:s.tagName)==null?void 0:r.toLowerCase())}))return;const t=this.getFocusableItems(),o=this.localize.dir()==="ltr",i=this.localize.dir()==="rtl";if(t.length>0){e.preventDefault();const s=t.findIndex(c=>c.matches(":focus")),r=t[s],a=c=>{const d=t[ht(c,0,t.length-1)];this.focusItem(d)},n=c=>{r.expanded=c};e.key==="ArrowDown"?a(s+1):e.key==="ArrowUp"?a(s-1):o&&e.key==="ArrowRight"||i&&e.key==="ArrowLeft"?!r||r.disabled||r.expanded||r.isLeaf&&!r.lazy?a(s+1):n(!0):o&&e.key==="ArrowLeft"||i&&e.key==="ArrowRight"?!r||r.disabled||r.isLeaf||!r.expanded?a(s-1):n(!1):e.key==="Home"?a(0):e.key==="End"?a(t.length-1):(e.key==="Enter"||e.key===" ")&&(r.disabled||this.selectItem(r))}}handleClick(e){const t=e.target,o=t.closest("sl-tree-item"),i=e.composedPath().some(s=>{var r;return(r=s==null?void 0:s.classList)==null?void 0:r.contains("tree-item__expand-button")});!o||o.disabled||t!==this.clickTarget||(i?o.expanded=!o.expanded:this.selectItem(o))}handleMouseDown(e){this.clickTarget=e.target}handleSlotChange(){this.getAllTreeItems().forEach(this.initTreeItem)}async handleSelectionChange(){const e=this.selection==="multiple",t=this.getAllTreeItems();this.setAttribute("aria-multiselectable",e?"true":"false");for(const o of t)o.selectable=e;e&&(await this.updateComplete,[...this.querySelectorAll(":scope > sl-tree-item")].forEach(o=>wn(o,!0)))}get selectedItems(){const e=this.getAllTreeItems(),t=o=>o.selected;return e.filter(t)}getFocusableItems(){const e=this.getAllTreeItems(),t=new Set;return e.filter(o=>{var i;if(o.disabled)return!1;const s=(i=o.parentElement)==null?void 0:i.closest("[role=treeitem]");return s&&(!s.expanded||s.loading||t.has(s))&&t.add(o),!t.has(o)})}render(){return x`
      <div
        part="base"
        class="tree"
        @click=${this.handleClick}
        @keydown=${this.handleKeyDown}
        @mousedown=${this.handleMouseDown}
      >
        <slot @slotchange=${this.handleSlotChange}></slot>
        <span hidden aria-hidden="true"><slot name="expand-icon"></slot></span>
        <span hidden aria-hidden="true"><slot name="collapse-icon"></slot></span>
      </div>
    `}};po.styles=[D,Ip];l([S("slot:not([name])")],po.prototype,"defaultSlot",2);l([S("slot[name=expand-icon]")],po.prototype,"expandedIconSlot",2);l([S("slot[name=collapse-icon]")],po.prototype,"collapsedIconSlot",2);l([p()],po.prototype,"selection",2);l([C("selection")],po.prototype,"handleSelectionChange",1);po.define("sl-tree");var Bp=O`
  :host(:not(:focus-within)) {
    position: absolute !important;
    width: 1px !important;
    height: 1px !important;
    clip: rect(0 0 0 0) !important;
    clip-path: inset(50%) !important;
    border: none !important;
    overflow: hidden !important;
    white-space: nowrap !important;
    padding: 0 !important;
  }
`,pa=class extends T{render(){return x` <slot></slot> `}};pa.styles=[D,Bp];pa.define("sl-visually-hidden");var Fp=O`
  :host {
    --padding: 0;

    display: none;
  }

  :host([active]) {
    display: block;
  }

  .tab-panel {
    display: block;
    padding: var(--padding);
  }
`,Vp=0,Ii=class extends T{constructor(){super(...arguments),this.attrId=++Vp,this.componentId=`sl-tab-panel-${this.attrId}`,this.name="",this.active=!1}connectedCallback(){super.connectedCallback(),this.id=this.id.length>0?this.id:this.componentId,this.setAttribute("role","tabpanel")}handleActiveChange(){this.setAttribute("aria-hidden",this.active?"false":"true")}render(){return x`
      <slot
        part="base"
        class=${R({"tab-panel":!0,"tab-panel--active":this.active})}
      ></slot>
    `}};Ii.styles=[D,Fp];l([p({reflect:!0})],Ii.prototype,"name",2);l([p({type:Boolean,reflect:!0})],Ii.prototype,"active",2);l([C("active")],Ii.prototype,"handleActiveChange",1);Ii.define("sl-tab-panel");var Up=O`
  :host {
    display: inline-block;
  }

  .tag {
    display: flex;
    align-items: center;
    border: solid 1px;
    line-height: 1;
    white-space: nowrap;
    user-select: none;
    -webkit-user-select: none;
  }

  .tag__remove::part(base) {
    color: inherit;
    padding: 0;
  }

  /*
   * Variant modifiers
   */

  .tag--primary {
    background-color: var(--sl-color-primary-50);
    border-color: var(--sl-color-primary-200);
    color: var(--sl-color-primary-800);
  }

  .tag--primary:active > sl-icon-button {
    color: var(--sl-color-primary-600);
  }

  .tag--success {
    background-color: var(--sl-color-success-50);
    border-color: var(--sl-color-success-200);
    color: var(--sl-color-success-800);
  }

  .tag--success:active > sl-icon-button {
    color: var(--sl-color-success-600);
  }

  .tag--neutral {
    background-color: var(--sl-color-neutral-50);
    border-color: var(--sl-color-neutral-200);
    color: var(--sl-color-neutral-800);
  }

  .tag--neutral:active > sl-icon-button {
    color: var(--sl-color-neutral-600);
  }

  .tag--warning {
    background-color: var(--sl-color-warning-50);
    border-color: var(--sl-color-warning-200);
    color: var(--sl-color-warning-800);
  }

  .tag--warning:active > sl-icon-button {
    color: var(--sl-color-warning-600);
  }

  .tag--danger {
    background-color: var(--sl-color-danger-50);
    border-color: var(--sl-color-danger-200);
    color: var(--sl-color-danger-800);
  }

  .tag--danger:active > sl-icon-button {
    color: var(--sl-color-danger-600);
  }

  /*
   * Size modifiers
   */

  .tag--small {
    font-size: var(--sl-button-font-size-small);
    height: calc(var(--sl-input-height-small) * 0.8);
    line-height: calc(var(--sl-input-height-small) - var(--sl-input-border-width) * 2);
    border-radius: var(--sl-input-border-radius-small);
    padding: 0 var(--sl-spacing-x-small);
  }

  .tag--medium {
    font-size: var(--sl-button-font-size-medium);
    height: calc(var(--sl-input-height-medium) * 0.8);
    line-height: calc(var(--sl-input-height-medium) - var(--sl-input-border-width) * 2);
    border-radius: var(--sl-input-border-radius-medium);
    padding: 0 var(--sl-spacing-small);
  }

  .tag--large {
    font-size: var(--sl-button-font-size-large);
    height: calc(var(--sl-input-height-large) * 0.8);
    line-height: calc(var(--sl-input-height-large) - var(--sl-input-border-width) * 2);
    border-radius: var(--sl-input-border-radius-large);
    padding: 0 var(--sl-spacing-medium);
  }

  .tag__remove {
    margin-inline-start: var(--sl-spacing-x-small);
  }

  /*
   * Pill modifier
   */

  .tag--pill {
    border-radius: var(--sl-border-radius-pill);
  }
`,Np=O`
  :host {
    display: inline-block;
    color: var(--sl-color-neutral-600);
  }

  .icon-button {
    flex: 0 0 auto;
    display: flex;
    align-items: center;
    background: none;
    border: none;
    border-radius: var(--sl-border-radius-medium);
    font-size: inherit;
    color: inherit;
    padding: var(--sl-spacing-x-small);
    cursor: pointer;
    transition: var(--sl-transition-x-fast) color;
    -webkit-appearance: none;
  }

  .icon-button:hover:not(.icon-button--disabled),
  .icon-button:focus-visible:not(.icon-button--disabled) {
    color: var(--sl-color-primary-600);
  }

  .icon-button:active:not(.icon-button--disabled) {
    color: var(--sl-color-primary-700);
  }

  .icon-button:focus {
    outline: none;
  }

  .icon-button--disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .icon-button:focus-visible {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  .icon-button__icon {
    pointer-events: none;
  }
`;/**
 * @license
 * Copyright 2020 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Yl=Symbol.for(""),Hp=e=>{if((e==null?void 0:e.r)===Yl)return e==null?void 0:e._$litStatic$},xs=(e,...t)=>({_$litStatic$:t.reduce((o,i,s)=>o+(r=>{if(r._$litStatic$!==void 0)return r._$litStatic$;throw Error(`Value passed to 'literal' function must be a 'literal' result: ${r}. Use 'unsafeStatic' to pass non-literal values, but
            take care to ensure page security.`)})(i)+e[s+1],e[0]),r:Yl}),xn=new Map,jp=e=>(t,...o)=>{const i=o.length;let s,r;const a=[],n=[];let c,d=0,u=!1;for(;d<i;){for(c=t[d];d<i&&(r=o[d],(s=Hp(r))!==void 0);)c+=s+t[++d],u=!0;d!==i&&n.push(r),a.push(c),d++}if(d===i&&a.push(t[i]),u){const h=a.join("$$lit$$");(t=xn.get(h))===void 0&&(a.raw=a,xn.set(h,t=a)),o=n}return e(t,...o)},ci=jp(x);var gt=class extends T{constructor(){super(...arguments),this.hasFocus=!1,this.label="",this.disabled=!1}handleBlur(){this.hasFocus=!1,this.emit("sl-blur")}handleFocus(){this.hasFocus=!0,this.emit("sl-focus")}handleClick(e){this.disabled&&(e.preventDefault(),e.stopPropagation())}click(){this.button.click()}focus(e){this.button.focus(e)}blur(){this.button.blur()}render(){const e=!!this.href,t=e?xs`a`:xs`button`;return ci`
      <${t}
        part="base"
        class=${R({"icon-button":!0,"icon-button--disabled":!e&&this.disabled,"icon-button--focused":this.hasFocus})}
        ?disabled=${E(e?void 0:this.disabled)}
        type=${E(e?void 0:"button")}
        href=${E(e?this.href:void 0)}
        target=${E(e?this.target:void 0)}
        download=${E(e?this.download:void 0)}
        rel=${E(e&&this.target?"noreferrer noopener":void 0)}
        role=${E(e?void 0:"button")}
        aria-disabled=${this.disabled?"true":"false"}
        aria-label="${this.label}"
        tabindex=${this.disabled?"-1":"0"}
        @blur=${this.handleBlur}
        @focus=${this.handleFocus}
        @click=${this.handleClick}
      >
        <sl-icon
          class="icon-button__icon"
          name=${E(this.name)}
          library=${E(this.library)}
          src=${E(this.src)}
          aria-hidden="true"
        ></sl-icon>
      </${t}>
    `}};gt.styles=[D,Np];gt.dependencies={"sl-icon":J};l([S(".icon-button")],gt.prototype,"button",2);l([z()],gt.prototype,"hasFocus",2);l([p()],gt.prototype,"name",2);l([p()],gt.prototype,"library",2);l([p()],gt.prototype,"src",2);l([p()],gt.prototype,"href",2);l([p()],gt.prototype,"target",2);l([p()],gt.prototype,"download",2);l([p()],gt.prototype,"label",2);l([p({type:Boolean,reflect:!0})],gt.prototype,"disabled",2);var Ue=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.variant="neutral",this.size="medium",this.pill=!1,this.removable=!1}handleRemoveClick(){this.emit("sl-remove")}render(){return x`
      <span
        part="base"
        class=${R({tag:!0,"tag--primary":this.variant==="primary","tag--success":this.variant==="success","tag--neutral":this.variant==="neutral","tag--warning":this.variant==="warning","tag--danger":this.variant==="danger","tag--text":this.variant==="text","tag--small":this.size==="small","tag--medium":this.size==="medium","tag--large":this.size==="large","tag--pill":this.pill,"tag--removable":this.removable})}
      >
        <slot part="content" class="tag__content"></slot>

        ${this.removable?x`
              <sl-icon-button
                part="remove-button"
                exportparts="base:remove-button__base"
                name="x-lg"
                library="system"
                label=${this.localize.term("remove")}
                class="tag__remove"
                @click=${this.handleRemoveClick}
                tabindex="-1"
              ></sl-icon-button>
            `:""}
      </span>
    `}};Ue.styles=[D,Up];Ue.dependencies={"sl-icon-button":gt};l([p({reflect:!0})],Ue.prototype,"variant",2);l([p({reflect:!0})],Ue.prototype,"size",2);l([p({type:Boolean,reflect:!0})],Ue.prototype,"pill",2);l([p({type:Boolean})],Ue.prototype,"removable",2);Ue.define("sl-tag");var qp=O`
  :host {
    display: block;
  }

  .textarea {
    display: grid;
    align-items: center;
    position: relative;
    width: 100%;
    font-family: var(--sl-input-font-family);
    font-weight: var(--sl-input-font-weight);
    line-height: var(--sl-line-height-normal);
    letter-spacing: var(--sl-input-letter-spacing);
    vertical-align: middle;
    transition:
      var(--sl-transition-fast) color,
      var(--sl-transition-fast) border,
      var(--sl-transition-fast) box-shadow,
      var(--sl-transition-fast) background-color;
    cursor: text;
  }

  /* Standard textareas */
  .textarea--standard {
    background-color: var(--sl-input-background-color);
    border: solid var(--sl-input-border-width) var(--sl-input-border-color);
  }

  .textarea--standard:hover:not(.textarea--disabled) {
    background-color: var(--sl-input-background-color-hover);
    border-color: var(--sl-input-border-color-hover);
  }
  .textarea--standard:hover:not(.textarea--disabled) .textarea__control {
    color: var(--sl-input-color-hover);
  }

  .textarea--standard.textarea--focused:not(.textarea--disabled) {
    background-color: var(--sl-input-background-color-focus);
    border-color: var(--sl-input-border-color-focus);
    color: var(--sl-input-color-focus);
    box-shadow: 0 0 0 var(--sl-focus-ring-width) var(--sl-input-focus-ring-color);
  }

  .textarea--standard.textarea--focused:not(.textarea--disabled) .textarea__control {
    color: var(--sl-input-color-focus);
  }

  .textarea--standard.textarea--disabled {
    background-color: var(--sl-input-background-color-disabled);
    border-color: var(--sl-input-border-color-disabled);
    opacity: 0.5;
    cursor: not-allowed;
  }

  .textarea__control,
  .textarea__size-adjuster {
    grid-area: 1 / 1 / 2 / 2;
  }

  .textarea__size-adjuster {
    visibility: hidden;
    pointer-events: none;
    opacity: 0;
  }

  .textarea--standard.textarea--disabled .textarea__control {
    color: var(--sl-input-color-disabled);
  }

  .textarea--standard.textarea--disabled .textarea__control::placeholder {
    color: var(--sl-input-placeholder-color-disabled);
  }

  /* Filled textareas */
  .textarea--filled {
    border: none;
    background-color: var(--sl-input-filled-background-color);
    color: var(--sl-input-color);
  }

  .textarea--filled:hover:not(.textarea--disabled) {
    background-color: var(--sl-input-filled-background-color-hover);
  }

  .textarea--filled.textarea--focused:not(.textarea--disabled) {
    background-color: var(--sl-input-filled-background-color-focus);
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  .textarea--filled.textarea--disabled {
    background-color: var(--sl-input-filled-background-color-disabled);
    opacity: 0.5;
    cursor: not-allowed;
  }

  .textarea__control {
    font-family: inherit;
    font-size: inherit;
    font-weight: inherit;
    line-height: 1.4;
    color: var(--sl-input-color);
    border: none;
    background: none;
    box-shadow: none;
    cursor: inherit;
    -webkit-appearance: none;
  }

  .textarea__control::-webkit-search-decoration,
  .textarea__control::-webkit-search-cancel-button,
  .textarea__control::-webkit-search-results-button,
  .textarea__control::-webkit-search-results-decoration {
    -webkit-appearance: none;
  }

  .textarea__control::placeholder {
    color: var(--sl-input-placeholder-color);
    user-select: none;
    -webkit-user-select: none;
  }

  .textarea__control:focus {
    outline: none;
  }

  /*
   * Size modifiers
   */

  .textarea--small {
    border-radius: var(--sl-input-border-radius-small);
    font-size: var(--sl-input-font-size-small);
  }

  .textarea--small .textarea__control {
    padding: 0.5em var(--sl-input-spacing-small);
  }

  .textarea--medium {
    border-radius: var(--sl-input-border-radius-medium);
    font-size: var(--sl-input-font-size-medium);
  }

  .textarea--medium .textarea__control {
    padding: 0.5em var(--sl-input-spacing-medium);
  }

  .textarea--large {
    border-radius: var(--sl-input-border-radius-large);
    font-size: var(--sl-input-font-size-large);
  }

  .textarea--large .textarea__control {
    padding: 0.5em var(--sl-input-spacing-large);
  }

  /*
   * Resize types
   */

  .textarea--resize-none .textarea__control {
    resize: none;
  }

  .textarea--resize-vertical .textarea__control {
    resize: vertical;
  }

  .textarea--resize-auto .textarea__control {
    height: auto;
    resize: none;
    overflow-y: hidden;
  }
`,q=class extends T{constructor(){super(...arguments),this.formControlController=new Ee(this,{assumeInteractionOn:["sl-blur","sl-input"]}),this.hasSlotController=new Ot(this,"help-text","label"),this.hasFocus=!1,this.title="",this.name="",this.value="",this.size="medium",this.filled=!1,this.label="",this.helpText="",this.placeholder="",this.rows=4,this.resize="vertical",this.disabled=!1,this.readonly=!1,this.form="",this.required=!1,this.spellcheck=!0,this.defaultValue=""}get validity(){return this.input.validity}get validationMessage(){return this.input.validationMessage}connectedCallback(){super.connectedCallback(),this.resizeObserver=new ResizeObserver(()=>this.setTextareaHeight()),this.updateComplete.then(()=>{this.setTextareaHeight(),this.resizeObserver.observe(this.input)})}firstUpdated(){this.formControlController.updateValidity()}disconnectedCallback(){var e;super.disconnectedCallback(),this.input&&((e=this.resizeObserver)==null||e.unobserve(this.input))}handleBlur(){this.hasFocus=!1,this.emit("sl-blur")}handleChange(){this.value=this.input.value,this.setTextareaHeight(),this.emit("sl-change")}handleFocus(){this.hasFocus=!0,this.emit("sl-focus")}handleInput(){this.value=this.input.value,this.emit("sl-input")}handleInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}setTextareaHeight(){this.resize==="auto"?(this.sizeAdjuster.style.height=`${this.input.clientHeight}px`,this.input.style.height="auto",this.input.style.height=`${this.input.scrollHeight}px`):this.input.style.height=""}handleDisabledChange(){this.formControlController.setValidity(this.disabled)}handleRowsChange(){this.setTextareaHeight()}async handleValueChange(){await this.updateComplete,this.formControlController.updateValidity(),this.setTextareaHeight()}focus(e){this.input.focus(e)}blur(){this.input.blur()}select(){this.input.select()}scrollPosition(e){if(e){typeof e.top=="number"&&(this.input.scrollTop=e.top),typeof e.left=="number"&&(this.input.scrollLeft=e.left);return}return{top:this.input.scrollTop,left:this.input.scrollTop}}setSelectionRange(e,t,o="none"){this.input.setSelectionRange(e,t,o)}setRangeText(e,t,o,i="preserve"){const s=t??this.input.selectionStart,r=o??this.input.selectionEnd;this.input.setRangeText(e,s,r,i),this.value!==this.input.value&&(this.value=this.input.value,this.setTextareaHeight())}checkValidity(){return this.input.checkValidity()}getForm(){return this.formControlController.getForm()}reportValidity(){return this.input.reportValidity()}setCustomValidity(e){this.input.setCustomValidity(e),this.formControlController.updateValidity()}render(){const e=this.hasSlotController.test("label"),t=this.hasSlotController.test("help-text"),o=this.label?!0:!!e,i=this.helpText?!0:!!t;return x`
      <div
        part="form-control"
        class=${R({"form-control":!0,"form-control--small":this.size==="small","form-control--medium":this.size==="medium","form-control--large":this.size==="large","form-control--has-label":o,"form-control--has-help-text":i})}
      >
        <label
          part="form-control-label"
          class="form-control__label"
          for="input"
          aria-hidden=${o?"false":"true"}
        >
          <slot name="label">${this.label}</slot>
        </label>

        <div part="form-control-input" class="form-control-input">
          <div
            part="base"
            class=${R({textarea:!0,"textarea--small":this.size==="small","textarea--medium":this.size==="medium","textarea--large":this.size==="large","textarea--standard":!this.filled,"textarea--filled":this.filled,"textarea--disabled":this.disabled,"textarea--focused":this.hasFocus,"textarea--empty":!this.value,"textarea--resize-none":this.resize==="none","textarea--resize-vertical":this.resize==="vertical","textarea--resize-auto":this.resize==="auto"})}
          >
            <textarea
              part="textarea"
              id="input"
              class="textarea__control"
              title=${this.title}
              name=${E(this.name)}
              .value=${io(this.value)}
              ?disabled=${this.disabled}
              ?readonly=${this.readonly}
              ?required=${this.required}
              placeholder=${E(this.placeholder)}
              rows=${E(this.rows)}
              minlength=${E(this.minlength)}
              maxlength=${E(this.maxlength)}
              autocapitalize=${E(this.autocapitalize)}
              autocorrect=${E(this.autocorrect)}
              ?autofocus=${this.autofocus}
              spellcheck=${E(this.spellcheck)}
              enterkeyhint=${E(this.enterkeyhint)}
              inputmode=${E(this.inputmode)}
              aria-describedby="help-text"
              @change=${this.handleChange}
              @input=${this.handleInput}
              @invalid=${this.handleInvalid}
              @focus=${this.handleFocus}
              @blur=${this.handleBlur}
            ></textarea>
            <!-- This "adjuster" exists to prevent layout shifting. https://github.com/shoelace-style/shoelace/issues/2180 -->
            <div part="textarea-adjuster" class="textarea__size-adjuster" ?hidden=${this.resize!=="auto"}></div>
          </div>
        </div>

        <div
          part="form-control-help-text"
          id="help-text"
          class="form-control__help-text"
          aria-hidden=${i?"false":"true"}
        >
          <slot name="help-text">${this.helpText}</slot>
        </div>
      </div>
    `}};q.styles=[D,uo,qp];l([S(".textarea__control")],q.prototype,"input",2);l([S(".textarea__size-adjuster")],q.prototype,"sizeAdjuster",2);l([z()],q.prototype,"hasFocus",2);l([p()],q.prototype,"title",2);l([p()],q.prototype,"name",2);l([p()],q.prototype,"value",2);l([p({reflect:!0})],q.prototype,"size",2);l([p({type:Boolean,reflect:!0})],q.prototype,"filled",2);l([p()],q.prototype,"label",2);l([p({attribute:"help-text"})],q.prototype,"helpText",2);l([p()],q.prototype,"placeholder",2);l([p({type:Number})],q.prototype,"rows",2);l([p()],q.prototype,"resize",2);l([p({type:Boolean,reflect:!0})],q.prototype,"disabled",2);l([p({type:Boolean,reflect:!0})],q.prototype,"readonly",2);l([p({reflect:!0})],q.prototype,"form",2);l([p({type:Boolean,reflect:!0})],q.prototype,"required",2);l([p({type:Number})],q.prototype,"minlength",2);l([p({type:Number})],q.prototype,"maxlength",2);l([p()],q.prototype,"autocapitalize",2);l([p()],q.prototype,"autocorrect",2);l([p()],q.prototype,"autocomplete",2);l([p({type:Boolean})],q.prototype,"autofocus",2);l([p()],q.prototype,"enterkeyhint",2);l([p({type:Boolean,converter:{fromAttribute:e=>!(!e||e==="false"),toAttribute:e=>e?"true":"false"}})],q.prototype,"spellcheck",2);l([p()],q.prototype,"inputmode",2);l([Io()],q.prototype,"defaultValue",2);l([C("disabled",{waitUntilFirstUpdate:!0})],q.prototype,"handleDisabledChange",1);l([C("rows",{waitUntilFirstUpdate:!0})],q.prototype,"handleRowsChange",1);l([C("value",{waitUntilFirstUpdate:!0})],q.prototype,"handleValueChange",1);q.define("sl-textarea");var Wp=O`
  :host {
    display: inline-block;
  }

  .tab {
    display: inline-flex;
    align-items: center;
    font-family: var(--sl-font-sans);
    font-size: var(--sl-font-size-small);
    font-weight: var(--sl-font-weight-semibold);
    border-radius: var(--sl-border-radius-medium);
    color: var(--sl-color-neutral-600);
    padding: var(--sl-spacing-medium) var(--sl-spacing-large);
    white-space: nowrap;
    user-select: none;
    -webkit-user-select: none;
    cursor: pointer;
    transition:
      var(--transition-speed) box-shadow,
      var(--transition-speed) color;
  }

  .tab:hover:not(.tab--disabled) {
    color: var(--sl-color-primary-600);
  }

  :host(:focus) {
    outline: transparent;
  }

  :host(:focus-visible) {
    color: var(--sl-color-primary-600);
    outline: var(--sl-focus-ring);
    outline-offset: calc(-1 * var(--sl-focus-ring-width) - var(--sl-focus-ring-offset));
  }

  .tab.tab--active:not(.tab--disabled) {
    color: var(--sl-color-primary-600);
  }

  .tab.tab--closable {
    padding-inline-end: var(--sl-spacing-small);
  }

  .tab.tab--disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .tab__close-button {
    font-size: var(--sl-font-size-small);
    margin-inline-start: var(--sl-spacing-small);
  }

  .tab__close-button::part(base) {
    padding: var(--sl-spacing-3x-small);
  }

  @media (forced-colors: active) {
    .tab.tab--active:not(.tab--disabled) {
      outline: solid 1px transparent;
      outline-offset: -3px;
    }
  }
`,Kp=0,ee=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.attrId=++Kp,this.componentId=`sl-tab-${this.attrId}`,this.panel="",this.active=!1,this.closable=!1,this.disabled=!1,this.tabIndex=0}connectedCallback(){super.connectedCallback(),this.setAttribute("role","tab")}handleCloseClick(e){e.stopPropagation(),this.emit("sl-close")}handleActiveChange(){this.setAttribute("aria-selected",this.active?"true":"false")}handleDisabledChange(){this.setAttribute("aria-disabled",this.disabled?"true":"false"),this.disabled&&!this.active?this.tabIndex=-1:this.tabIndex=0}render(){return this.id=this.id.length>0?this.id:this.componentId,x`
      <div
        part="base"
        class=${R({tab:!0,"tab--active":this.active,"tab--closable":this.closable,"tab--disabled":this.disabled})}
      >
        <slot></slot>
        ${this.closable?x`
              <sl-icon-button
                part="close-button"
                exportparts="base:close-button__base"
                name="x-lg"
                library="system"
                label=${this.localize.term("close")}
                class="tab__close-button"
                @click=${this.handleCloseClick}
                tabindex="-1"
              ></sl-icon-button>
            `:""}
      </div>
    `}};ee.styles=[D,Wp];ee.dependencies={"sl-icon-button":gt};l([S(".tab")],ee.prototype,"tab",2);l([p({reflect:!0})],ee.prototype,"panel",2);l([p({type:Boolean,reflect:!0})],ee.prototype,"active",2);l([p({type:Boolean,reflect:!0})],ee.prototype,"closable",2);l([p({type:Boolean,reflect:!0})],ee.prototype,"disabled",2);l([p({type:Number,reflect:!0})],ee.prototype,"tabIndex",2);l([C("active")],ee.prototype,"handleActiveChange",1);l([C("disabled")],ee.prototype,"handleDisabledChange",1);ee.define("sl-tab");var Yp=O`
  :host {
    --indicator-color: var(--sl-color-primary-600);
    --track-color: var(--sl-color-neutral-200);
    --track-width: 2px;

    display: block;
  }

  .tab-group {
    display: flex;
    border-radius: 0;
  }

  .tab-group__tabs {
    display: flex;
    position: relative;
  }

  .tab-group__indicator {
    position: absolute;
    transition:
      var(--sl-transition-fast) translate ease,
      var(--sl-transition-fast) width ease;
  }

  .tab-group--has-scroll-controls .tab-group__nav-container {
    position: relative;
    padding: 0 var(--sl-spacing-x-large);
  }

  .tab-group--has-scroll-controls .tab-group__scroll-button--start--hidden,
  .tab-group--has-scroll-controls .tab-group__scroll-button--end--hidden {
    visibility: hidden;
  }

  .tab-group__body {
    display: block;
    overflow: auto;
  }

  .tab-group__scroll-button {
    display: flex;
    align-items: center;
    justify-content: center;
    position: absolute;
    top: 0;
    bottom: 0;
    width: var(--sl-spacing-x-large);
  }

  .tab-group__scroll-button--start {
    left: 0;
  }

  .tab-group__scroll-button--end {
    right: 0;
  }

  .tab-group--rtl .tab-group__scroll-button--start {
    left: auto;
    right: 0;
  }

  .tab-group--rtl .tab-group__scroll-button--end {
    left: 0;
    right: auto;
  }

  /*
   * Top
   */

  .tab-group--top {
    flex-direction: column;
  }

  .tab-group--top .tab-group__nav-container {
    order: 1;
  }

  .tab-group--top .tab-group__nav {
    display: flex;
    overflow-x: auto;

    /* Hide scrollbar in Firefox */
    scrollbar-width: none;
  }

  /* Hide scrollbar in Chrome/Safari */
  .tab-group--top .tab-group__nav::-webkit-scrollbar {
    width: 0;
    height: 0;
  }

  .tab-group--top .tab-group__tabs {
    flex: 1 1 auto;
    position: relative;
    flex-direction: row;
    border-bottom: solid var(--track-width) var(--track-color);
  }

  .tab-group--top .tab-group__indicator {
    bottom: calc(-1 * var(--track-width));
    border-bottom: solid var(--track-width) var(--indicator-color);
  }

  .tab-group--top .tab-group__body {
    order: 2;
  }

  .tab-group--top ::slotted(sl-tab-panel) {
    --padding: var(--sl-spacing-medium) 0;
  }

  /*
   * Bottom
   */

  .tab-group--bottom {
    flex-direction: column;
  }

  .tab-group--bottom .tab-group__nav-container {
    order: 2;
  }

  .tab-group--bottom .tab-group__nav {
    display: flex;
    overflow-x: auto;

    /* Hide scrollbar in Firefox */
    scrollbar-width: none;
  }

  /* Hide scrollbar in Chrome/Safari */
  .tab-group--bottom .tab-group__nav::-webkit-scrollbar {
    width: 0;
    height: 0;
  }

  .tab-group--bottom .tab-group__tabs {
    flex: 1 1 auto;
    position: relative;
    flex-direction: row;
    border-top: solid var(--track-width) var(--track-color);
  }

  .tab-group--bottom .tab-group__indicator {
    top: calc(-1 * var(--track-width));
    border-top: solid var(--track-width) var(--indicator-color);
  }

  .tab-group--bottom .tab-group__body {
    order: 1;
  }

  .tab-group--bottom ::slotted(sl-tab-panel) {
    --padding: var(--sl-spacing-medium) 0;
  }

  /*
   * Start
   */

  .tab-group--start {
    flex-direction: row;
  }

  .tab-group--start .tab-group__nav-container {
    order: 1;
  }

  .tab-group--start .tab-group__tabs {
    flex: 0 0 auto;
    flex-direction: column;
    border-inline-end: solid var(--track-width) var(--track-color);
  }

  .tab-group--start .tab-group__indicator {
    right: calc(-1 * var(--track-width));
    border-right: solid var(--track-width) var(--indicator-color);
  }

  .tab-group--start.tab-group--rtl .tab-group__indicator {
    right: auto;
    left: calc(-1 * var(--track-width));
  }

  .tab-group--start .tab-group__body {
    flex: 1 1 auto;
    order: 2;
  }

  .tab-group--start ::slotted(sl-tab-panel) {
    --padding: 0 var(--sl-spacing-medium);
  }

  /*
   * End
   */

  .tab-group--end {
    flex-direction: row;
  }

  .tab-group--end .tab-group__nav-container {
    order: 2;
  }

  .tab-group--end .tab-group__tabs {
    flex: 0 0 auto;
    flex-direction: column;
    border-left: solid var(--track-width) var(--track-color);
  }

  .tab-group--end .tab-group__indicator {
    left: calc(-1 * var(--track-width));
    border-inline-start: solid var(--track-width) var(--indicator-color);
  }

  .tab-group--end.tab-group--rtl .tab-group__indicator {
    right: calc(-1 * var(--track-width));
    left: auto;
  }

  .tab-group--end .tab-group__body {
    flex: 1 1 auto;
    order: 1;
  }

  .tab-group--end ::slotted(sl-tab-panel) {
    --padding: 0 var(--sl-spacing-medium);
  }
`,Xp=O`
  :host {
    display: contents;
  }
`,Bi=class extends T{constructor(){super(...arguments),this.observedElements=[],this.disabled=!1}connectedCallback(){super.connectedCallback(),this.resizeObserver=new ResizeObserver(e=>{this.emit("sl-resize",{detail:{entries:e}})}),this.disabled||this.startObserver()}disconnectedCallback(){super.disconnectedCallback(),this.stopObserver()}handleSlotChange(){this.disabled||this.startObserver()}startObserver(){const e=this.shadowRoot.querySelector("slot");if(e!==null){const t=e.assignedElements({flatten:!0});this.observedElements.forEach(o=>this.resizeObserver.unobserve(o)),this.observedElements=[],t.forEach(o=>{this.resizeObserver.observe(o),this.observedElements.push(o)})}}stopObserver(){this.resizeObserver.disconnect()}handleDisabledChange(){this.disabled?this.stopObserver():this.startObserver()}render(){return x` <slot @slotchange=${this.handleSlotChange}></slot> `}};Bi.styles=[D,Xp];l([p({type:Boolean,reflect:!0})],Bi.prototype,"disabled",2);l([C("disabled",{waitUntilFirstUpdate:!0})],Bi.prototype,"handleDisabledChange",1);function Qp(e,t){return{top:Math.round(e.getBoundingClientRect().top-t.getBoundingClientRect().top),left:Math.round(e.getBoundingClientRect().left-t.getBoundingClientRect().left)}}var _r=new Set;function Zp(){const e=document.documentElement.clientWidth;return Math.abs(window.innerWidth-e)}function Gp(){const e=Number(getComputedStyle(document.body).paddingRight.replace(/px/,""));return isNaN(e)||!e?0:e}function di(e){if(_r.add(e),!document.documentElement.classList.contains("sl-scroll-lock")){const t=Zp()+Gp();let o=getComputedStyle(document.documentElement).scrollbarGutter;(!o||o==="auto")&&(o="stable"),t<2&&(o=""),document.documentElement.style.setProperty("--sl-scroll-lock-gutter",o),document.documentElement.classList.add("sl-scroll-lock"),document.documentElement.style.setProperty("--sl-scroll-lock-size",`${t}px`)}}function hi(e){_r.delete(e),_r.size===0&&(document.documentElement.classList.remove("sl-scroll-lock"),document.documentElement.style.removeProperty("--sl-scroll-lock-size"))}function $r(e,t,o="vertical",i="smooth"){const s=Qp(e,t),r=s.top+t.scrollTop,a=s.left+t.scrollLeft,n=t.scrollLeft,c=t.scrollLeft+t.offsetWidth,d=t.scrollTop,u=t.scrollTop+t.offsetHeight;(o==="horizontal"||o==="both")&&(a<n?t.scrollTo({left:a,behavior:i}):a+e.clientWidth>c&&t.scrollTo({left:a-t.offsetWidth+e.clientWidth,behavior:i})),(o==="vertical"||o==="both")&&(r<d?t.scrollTo({top:r,behavior:i}):r+e.clientHeight>u&&t.scrollTo({top:r-t.offsetHeight+e.clientHeight,behavior:i}))}var $t=class extends T{constructor(){super(...arguments),this.tabs=[],this.focusableTabs=[],this.panels=[],this.localize=new j(this),this.hasScrollControls=!1,this.shouldHideScrollStartButton=!1,this.shouldHideScrollEndButton=!1,this.placement="top",this.activation="auto",this.noScrollControls=!1,this.fixedScrollControls=!1,this.scrollOffset=1}connectedCallback(){const e=Promise.all([customElements.whenDefined("sl-tab"),customElements.whenDefined("sl-tab-panel")]);super.connectedCallback(),this.resizeObserver=new ResizeObserver(()=>{this.repositionIndicator(),this.updateScrollControls()}),this.mutationObserver=new MutationObserver(t=>{const o=t.filter(({target:i})=>{if(i===this)return!0;if(i.closest("sl-tab-group")!==this)return!1;const s=i.tagName.toLowerCase();return s==="sl-tab"||s==="sl-tab-panel"});if(o.length!==0){if(o.some(i=>!["aria-labelledby","aria-controls"].includes(i.attributeName))&&setTimeout(()=>this.setAriaLabels()),o.some(i=>i.attributeName==="disabled"))this.syncTabsAndPanels();else if(o.some(i=>i.attributeName==="active")){const s=o.filter(r=>r.attributeName==="active"&&r.target.tagName.toLowerCase()==="sl-tab").map(r=>r.target).find(r=>r.active);s&&this.setActiveTab(s)}}}),this.updateComplete.then(()=>{this.syncTabsAndPanels(),this.mutationObserver.observe(this,{attributes:!0,attributeFilter:["active","disabled","name","panel"],childList:!0,subtree:!0}),this.resizeObserver.observe(this.nav),e.then(()=>{new IntersectionObserver((o,i)=>{var s;o[0].intersectionRatio>0&&(this.setAriaLabels(),this.setActiveTab((s=this.getActiveTab())!=null?s:this.tabs[0],{emitEvents:!1}),i.unobserve(o[0].target))}).observe(this.tabGroup)})})}disconnectedCallback(){var e,t;super.disconnectedCallback(),(e=this.mutationObserver)==null||e.disconnect(),this.nav&&((t=this.resizeObserver)==null||t.unobserve(this.nav))}getAllTabs(){return this.shadowRoot.querySelector('slot[name="nav"]').assignedElements()}getAllPanels(){return[...this.body.assignedElements()].filter(e=>e.tagName.toLowerCase()==="sl-tab-panel")}getActiveTab(){return this.tabs.find(e=>e.active)}handleClick(e){const o=e.target.closest("sl-tab");(o==null?void 0:o.closest("sl-tab-group"))===this&&o!==null&&this.setActiveTab(o,{scrollBehavior:"smooth"})}handleKeyDown(e){const o=e.target.closest("sl-tab");if((o==null?void 0:o.closest("sl-tab-group"))===this&&(["Enter"," "].includes(e.key)&&o!==null&&(this.setActiveTab(o,{scrollBehavior:"smooth"}),e.preventDefault()),["ArrowLeft","ArrowRight","ArrowUp","ArrowDown","Home","End"].includes(e.key))){const s=this.tabs.find(n=>n.matches(":focus")),r=this.localize.dir()==="rtl";let a=null;if((s==null?void 0:s.tagName.toLowerCase())==="sl-tab"){if(e.key==="Home")a=this.focusableTabs[0];else if(e.key==="End")a=this.focusableTabs[this.focusableTabs.length-1];else if(["top","bottom"].includes(this.placement)&&e.key===(r?"ArrowRight":"ArrowLeft")||["start","end"].includes(this.placement)&&e.key==="ArrowUp"){const n=this.tabs.findIndex(c=>c===s);a=this.findNextFocusableTab(n,"backward")}else if(["top","bottom"].includes(this.placement)&&e.key===(r?"ArrowLeft":"ArrowRight")||["start","end"].includes(this.placement)&&e.key==="ArrowDown"){const n=this.tabs.findIndex(c=>c===s);a=this.findNextFocusableTab(n,"forward")}if(!a)return;a.tabIndex=0,a.focus({preventScroll:!0}),this.activation==="auto"?this.setActiveTab(a,{scrollBehavior:"smooth"}):this.tabs.forEach(n=>{n.tabIndex=n===a?0:-1}),["top","bottom"].includes(this.placement)&&$r(a,this.nav,"horizontal"),e.preventDefault()}}}handleScrollToStart(){this.nav.scroll({left:this.localize.dir()==="rtl"?this.nav.scrollLeft+this.nav.clientWidth:this.nav.scrollLeft-this.nav.clientWidth,behavior:"smooth"})}handleScrollToEnd(){this.nav.scroll({left:this.localize.dir()==="rtl"?this.nav.scrollLeft-this.nav.clientWidth:this.nav.scrollLeft+this.nav.clientWidth,behavior:"smooth"})}setActiveTab(e,t){if(t=ze({emitEvents:!0,scrollBehavior:"auto"},t),e!==this.activeTab&&!e.disabled){const o=this.activeTab;this.activeTab=e,this.tabs.forEach(i=>{i.active=i===this.activeTab,i.tabIndex=i===this.activeTab?0:-1}),this.panels.forEach(i=>{var s;return i.active=i.name===((s=this.activeTab)==null?void 0:s.panel)}),this.syncIndicator(),["top","bottom"].includes(this.placement)&&$r(this.activeTab,this.nav,"horizontal",t.scrollBehavior),t.emitEvents&&(o&&this.emit("sl-tab-hide",{detail:{name:o.panel}}),this.emit("sl-tab-show",{detail:{name:this.activeTab.panel}}))}}setAriaLabels(){this.tabs.forEach(e=>{const t=this.panels.find(o=>o.name===e.panel);t&&(e.setAttribute("aria-controls",t.getAttribute("id")),t.setAttribute("aria-labelledby",e.getAttribute("id")))})}repositionIndicator(){const e=this.getActiveTab();if(!e)return;const t=e.clientWidth,o=e.clientHeight,i=this.localize.dir()==="rtl",s=this.getAllTabs(),a=s.slice(0,s.indexOf(e)).reduce((n,c)=>({left:n.left+c.clientWidth,top:n.top+c.clientHeight}),{left:0,top:0});switch(this.placement){case"top":case"bottom":this.indicator.style.width=`${t}px`,this.indicator.style.height="auto",this.indicator.style.translate=i?`${-1*a.left}px`:`${a.left}px`;break;case"start":case"end":this.indicator.style.width="auto",this.indicator.style.height=`${o}px`,this.indicator.style.translate=`0 ${a.top}px`;break}}syncTabsAndPanels(){this.tabs=this.getAllTabs(),this.focusableTabs=this.tabs.filter(e=>!e.disabled),this.panels=this.getAllPanels(),this.syncIndicator(),this.updateComplete.then(()=>this.updateScrollControls())}findNextFocusableTab(e,t){let o=null;const i=t==="forward"?1:-1;let s=e+i;for(;e<this.tabs.length;){if(o=this.tabs[s]||null,o===null){t==="forward"?o=this.focusableTabs[0]:o=this.focusableTabs[this.focusableTabs.length-1];break}if(!o.disabled)break;s+=i}return o}updateScrollButtons(){this.hasScrollControls&&!this.fixedScrollControls&&(this.shouldHideScrollStartButton=this.scrollFromStart()<=this.scrollOffset,this.shouldHideScrollEndButton=this.isScrolledToEnd())}isScrolledToEnd(){return this.scrollFromStart()+this.nav.clientWidth>=this.nav.scrollWidth-this.scrollOffset}scrollFromStart(){return this.localize.dir()==="rtl"?-this.nav.scrollLeft:this.nav.scrollLeft}updateScrollControls(){this.noScrollControls?this.hasScrollControls=!1:this.hasScrollControls=["top","bottom"].includes(this.placement)&&this.nav.scrollWidth>this.nav.clientWidth+1,this.updateScrollButtons()}syncIndicator(){this.getActiveTab()?(this.indicator.style.display="block",this.repositionIndicator()):this.indicator.style.display="none"}show(e){const t=this.tabs.find(o=>o.panel===e);t&&this.setActiveTab(t,{scrollBehavior:"smooth"})}render(){const e=this.localize.dir()==="rtl";return x`
      <div
        part="base"
        class=${R({"tab-group":!0,"tab-group--top":this.placement==="top","tab-group--bottom":this.placement==="bottom","tab-group--start":this.placement==="start","tab-group--end":this.placement==="end","tab-group--rtl":this.localize.dir()==="rtl","tab-group--has-scroll-controls":this.hasScrollControls})}
        @click=${this.handleClick}
        @keydown=${this.handleKeyDown}
      >
        <div class="tab-group__nav-container" part="nav">
          ${this.hasScrollControls?x`
                <sl-icon-button
                  part="scroll-button scroll-button--start"
                  exportparts="base:scroll-button__base"
                  class=${R({"tab-group__scroll-button":!0,"tab-group__scroll-button--start":!0,"tab-group__scroll-button--start--hidden":this.shouldHideScrollStartButton})}
                  name=${e?"chevron-right":"chevron-left"}
                  library="system"
                  tabindex="-1"
                  aria-hidden="true"
                  label=${this.localize.term("scrollToStart")}
                  @click=${this.handleScrollToStart}
                ></sl-icon-button>
              `:""}

          <div class="tab-group__nav" @scrollend=${this.updateScrollButtons}>
            <div part="tabs" class="tab-group__tabs" role="tablist">
              <div part="active-tab-indicator" class="tab-group__indicator"></div>
              <sl-resize-observer @sl-resize=${this.syncIndicator}>
                <slot name="nav" @slotchange=${this.syncTabsAndPanels}></slot>
              </sl-resize-observer>
            </div>
          </div>

          ${this.hasScrollControls?x`
                <sl-icon-button
                  part="scroll-button scroll-button--end"
                  exportparts="base:scroll-button__base"
                  class=${R({"tab-group__scroll-button":!0,"tab-group__scroll-button--end":!0,"tab-group__scroll-button--end--hidden":this.shouldHideScrollEndButton})}
                  name=${e?"chevron-left":"chevron-right"}
                  library="system"
                  tabindex="-1"
                  aria-hidden="true"
                  label=${this.localize.term("scrollToEnd")}
                  @click=${this.handleScrollToEnd}
                ></sl-icon-button>
              `:""}
        </div>

        <slot part="body" class="tab-group__body" @slotchange=${this.syncTabsAndPanels}></slot>
      </div>
    `}};$t.styles=[D,Yp];$t.dependencies={"sl-icon-button":gt,"sl-resize-observer":Bi};l([S(".tab-group")],$t.prototype,"tabGroup",2);l([S(".tab-group__body")],$t.prototype,"body",2);l([S(".tab-group__nav")],$t.prototype,"nav",2);l([S(".tab-group__indicator")],$t.prototype,"indicator",2);l([z()],$t.prototype,"hasScrollControls",2);l([z()],$t.prototype,"shouldHideScrollStartButton",2);l([z()],$t.prototype,"shouldHideScrollEndButton",2);l([p()],$t.prototype,"placement",2);l([p()],$t.prototype,"activation",2);l([p({attribute:"no-scroll-controls",type:Boolean})],$t.prototype,"noScrollControls",2);l([p({attribute:"fixed-scroll-controls",type:Boolean})],$t.prototype,"fixedScrollControls",2);l([Ai({passive:!0})],$t.prototype,"updateScrollButtons",1);l([C("noScrollControls",{waitUntilFirstUpdate:!0})],$t.prototype,"updateScrollControls",1);l([C("placement",{waitUntilFirstUpdate:!0})],$t.prototype,"syncIndicator",1);$t.define("sl-tab-group");var Jp=O`
  :host {
    --border-radius: var(--sl-border-radius-pill);
    --color: var(--sl-color-neutral-200);
    --sheen-color: var(--sl-color-neutral-300);

    display: block;
    position: relative;
  }

  .skeleton {
    display: flex;
    width: 100%;
    height: 100%;
    min-height: 1rem;
  }

  .skeleton__indicator {
    flex: 1 1 auto;
    background: var(--color);
    border-radius: var(--border-radius);
  }

  .skeleton--sheen .skeleton__indicator {
    background: linear-gradient(270deg, var(--sheen-color), var(--color), var(--color), var(--sheen-color));
    background-size: 400% 100%;
    animation: sheen 8s ease-in-out infinite;
  }

  .skeleton--pulse .skeleton__indicator {
    animation: pulse 2s ease-in-out 0.5s infinite;
  }

  /* Forced colors mode */
  @media (forced-colors: active) {
    :host {
      --color: GrayText;
    }
  }

  @keyframes sheen {
    0% {
      background-position: 200% 0;
    }
    to {
      background-position: -200% 0;
    }
  }

  @keyframes pulse {
    0% {
      opacity: 1;
    }
    50% {
      opacity: 0.4;
    }
    100% {
      opacity: 1;
    }
  }
`,fa=class extends T{constructor(){super(...arguments),this.effect="none"}render(){return x`
      <div
        part="base"
        class=${R({skeleton:!0,"skeleton--pulse":this.effect==="pulse","skeleton--sheen":this.effect==="sheen"})}
      >
        <div part="indicator" class="skeleton__indicator"></div>
      </div>
    `}};fa.styles=[D,Jp];l([p()],fa.prototype,"effect",2);fa.define("sl-skeleton");var tf=O`
  :host {
    --divider-width: 4px;
    --divider-hit-area: 12px;
    --min: 0%;
    --max: 100%;

    display: grid;
  }

  .start,
  .end {
    overflow: hidden;
  }

  .divider {
    flex: 0 0 var(--divider-width);
    display: flex;
    position: relative;
    align-items: center;
    justify-content: center;
    background-color: var(--sl-color-neutral-200);
    color: var(--sl-color-neutral-900);
    z-index: 1;
  }

  .divider:focus {
    outline: none;
  }

  :host(:not([disabled])) .divider:focus-visible {
    background-color: var(--sl-color-primary-600);
    color: var(--sl-color-neutral-0);
  }

  :host([disabled]) .divider {
    cursor: not-allowed;
  }

  /* Horizontal */
  :host(:not([vertical], [disabled])) .divider {
    cursor: col-resize;
  }

  :host(:not([vertical])) .divider::after {
    display: flex;
    content: '';
    position: absolute;
    height: 100%;
    left: calc(var(--divider-hit-area) / -2 + var(--divider-width) / 2);
    width: var(--divider-hit-area);
  }

  /* Vertical */
  :host([vertical]) {
    flex-direction: column;
  }

  :host([vertical]:not([disabled])) .divider {
    cursor: row-resize;
  }

  :host([vertical]) .divider::after {
    content: '';
    position: absolute;
    width: 100%;
    top: calc(var(--divider-hit-area) / -2 + var(--divider-width) / 2);
    height: var(--divider-hit-area);
  }

  @media (forced-colors: active) {
    .divider {
      outline: solid 1px transparent;
    }
  }
`;function ui(e,t){function o(s){const r=e.getBoundingClientRect(),a=e.ownerDocument.defaultView,n=r.left+a.scrollX,c=r.top+a.scrollY,d=s.pageX-n,u=s.pageY-c;t!=null&&t.onMove&&t.onMove(d,u)}function i(){document.removeEventListener("pointermove",o),document.removeEventListener("pointerup",i),t!=null&&t.onStop&&t.onStop()}document.addEventListener("pointermove",o,{passive:!0}),document.addEventListener("pointerup",i),(t==null?void 0:t.initialEvent)instanceof PointerEvent&&o(t.initialEvent)}var kn=()=>null,Ut=class extends T{constructor(){super(...arguments),this.isCollapsed=!1,this.localize=new j(this),this.positionBeforeCollapsing=0,this.position=50,this.vertical=!1,this.disabled=!1,this.snapValue="",this.snapFunction=kn,this.snapThreshold=12}toSnapFunction(e){const t=e.split(" ");return({pos:o,size:i,snapThreshold:s,isRtl:r,vertical:a})=>{let n=o,c=Number.POSITIVE_INFINITY;return t.forEach(d=>{let u;if(d.startsWith("repeat(")){const f=e.substring(7,e.length-1),m=f.endsWith("%"),g=Number.parseFloat(f),b=m?i*(g/100):g;u=Math.round((r&&!a?i-o:o)/b)*b}else d.endsWith("%")?u=i*(Number.parseFloat(d)/100):u=Number.parseFloat(d);r&&!a&&(u=i-u);const h=Math.abs(o-u);h<=s&&h<c&&(n=u,c=h)}),n}}set snap(e){this.snapValue=e??"",e?this.snapFunction=typeof e=="string"?this.toSnapFunction(e):e:this.snapFunction=kn}get snap(){return this.snapValue}connectedCallback(){super.connectedCallback(),this.resizeObserver=new ResizeObserver(e=>this.handleResize(e)),this.updateComplete.then(()=>this.resizeObserver.observe(this)),this.detectSize(),this.cachedPositionInPixels=this.percentageToPixels(this.position)}disconnectedCallback(){var e;super.disconnectedCallback(),(e=this.resizeObserver)==null||e.unobserve(this)}detectSize(){const{width:e,height:t}=this.getBoundingClientRect();this.size=this.vertical?t:e}percentageToPixels(e){return this.size*(e/100)}pixelsToPercentage(e){return e/this.size*100}handleDrag(e){const t=this.localize.dir()==="rtl";this.disabled||(e.cancelable&&e.preventDefault(),ui(this,{onMove:(o,i)=>{var s;let r=this.vertical?i:o;this.primary==="end"&&(r=this.size-r),r=(s=this.snapFunction({pos:r,size:this.size,snapThreshold:this.snapThreshold,isRtl:t,vertical:this.vertical}))!=null?s:r,this.position=ht(this.pixelsToPercentage(r),0,100)},initialEvent:e}))}handleKeyDown(e){if(!this.disabled&&["ArrowLeft","ArrowRight","ArrowUp","ArrowDown","Home","End","Enter"].includes(e.key)){let t=this.position;const o=(e.shiftKey?10:1)*(this.primary==="end"?-1:1);if(e.preventDefault(),(e.key==="ArrowLeft"&&!this.vertical||e.key==="ArrowUp"&&this.vertical)&&(t-=o),(e.key==="ArrowRight"&&!this.vertical||e.key==="ArrowDown"&&this.vertical)&&(t+=o),e.key==="Home"&&(t=this.primary==="end"?100:0),e.key==="End"&&(t=this.primary==="end"?0:100),e.key==="Enter")if(this.isCollapsed)t=this.positionBeforeCollapsing,this.isCollapsed=!1;else{const i=this.position;t=0,requestAnimationFrame(()=>{this.isCollapsed=!0,this.positionBeforeCollapsing=i})}this.position=ht(t,0,100)}}handleResize(e){const{width:t,height:o}=e[0].contentRect;this.size=this.vertical?o:t,(isNaN(this.cachedPositionInPixels)||this.position===1/0)&&(this.cachedPositionInPixels=Number(this.getAttribute("position-in-pixels")),this.positionInPixels=Number(this.getAttribute("position-in-pixels")),this.position=this.pixelsToPercentage(this.positionInPixels)),this.primary&&(this.position=this.pixelsToPercentage(this.cachedPositionInPixels))}handlePositionChange(){this.cachedPositionInPixels=this.percentageToPixels(this.position),this.isCollapsed=!1,this.positionBeforeCollapsing=0,this.positionInPixels=this.percentageToPixels(this.position),this.emit("sl-reposition")}handlePositionInPixelsChange(){this.position=this.pixelsToPercentage(this.positionInPixels)}handleVerticalChange(){this.detectSize()}render(){const e=this.vertical?"gridTemplateRows":"gridTemplateColumns",t=this.vertical?"gridTemplateColumns":"gridTemplateRows",o=this.localize.dir()==="rtl",i=`
      clamp(
        0%,
        clamp(
          var(--min),
          ${this.position}% - var(--divider-width) / 2,
          var(--max)
        ),
        calc(100% - var(--divider-width))
      )
    `,s="auto";return this.primary==="end"?o&&!this.vertical?this.style[e]=`${i} var(--divider-width) ${s}`:this.style[e]=`${s} var(--divider-width) ${i}`:o&&!this.vertical?this.style[e]=`${s} var(--divider-width) ${i}`:this.style[e]=`${i} var(--divider-width) ${s}`,this.style[t]="",x`
      <slot name="start" part="panel start" class="start"></slot>

      <div
        part="divider"
        class="divider"
        tabindex=${E(this.disabled?void 0:"0")}
        role="separator"
        aria-valuenow=${this.position}
        aria-valuemin="0"
        aria-valuemax="100"
        aria-label=${this.localize.term("resize")}
        @keydown=${this.handleKeyDown}
        @mousedown=${this.handleDrag}
        @touchstart=${this.handleDrag}
      >
        <slot name="divider"></slot>
      </div>

      <slot name="end" part="panel end" class="end"></slot>
    `}};Ut.styles=[D,tf];l([S(".divider")],Ut.prototype,"divider",2);l([p({type:Number,reflect:!0})],Ut.prototype,"position",2);l([p({attribute:"position-in-pixels",type:Number})],Ut.prototype,"positionInPixels",2);l([p({type:Boolean,reflect:!0})],Ut.prototype,"vertical",2);l([p({type:Boolean,reflect:!0})],Ut.prototype,"disabled",2);l([p()],Ut.prototype,"primary",2);l([p({reflect:!0})],Ut.prototype,"snap",1);l([p({type:Number,attribute:"snap-threshold"})],Ut.prototype,"snapThreshold",2);l([C("position")],Ut.prototype,"handlePositionChange",1);l([C("positionInPixels")],Ut.prototype,"handlePositionInPixelsChange",1);l([C("vertical")],Ut.prototype,"handleVerticalChange",1);Ut.define("sl-split-panel");var ef=O`
  :host {
    display: inline-block;
  }

  :host([size='small']) {
    --height: var(--sl-toggle-size-small);
    --thumb-size: calc(var(--sl-toggle-size-small) + 4px);
    --width: calc(var(--height) * 2);

    font-size: var(--sl-input-font-size-small);
  }

  :host([size='medium']) {
    --height: var(--sl-toggle-size-medium);
    --thumb-size: calc(var(--sl-toggle-size-medium) + 4px);
    --width: calc(var(--height) * 2);

    font-size: var(--sl-input-font-size-medium);
  }

  :host([size='large']) {
    --height: var(--sl-toggle-size-large);
    --thumb-size: calc(var(--sl-toggle-size-large) + 4px);
    --width: calc(var(--height) * 2);

    font-size: var(--sl-input-font-size-large);
  }

  .switch {
    position: relative;
    display: inline-flex;
    align-items: center;
    font-family: var(--sl-input-font-family);
    font-size: inherit;
    font-weight: var(--sl-input-font-weight);
    color: var(--sl-input-label-color);
    vertical-align: middle;
    cursor: pointer;
  }

  .switch__control {
    flex: 0 0 auto;
    position: relative;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: var(--width);
    height: var(--height);
    background-color: var(--sl-color-neutral-400);
    border: solid var(--sl-input-border-width) var(--sl-color-neutral-400);
    border-radius: var(--height);
    transition:
      var(--sl-transition-fast) border-color,
      var(--sl-transition-fast) background-color;
  }

  .switch__control .switch__thumb {
    width: var(--thumb-size);
    height: var(--thumb-size);
    background-color: var(--sl-color-neutral-0);
    border-radius: 50%;
    border: solid var(--sl-input-border-width) var(--sl-color-neutral-400);
    translate: calc((var(--width) - var(--height)) / -2);
    transition:
      var(--sl-transition-fast) translate ease,
      var(--sl-transition-fast) background-color,
      var(--sl-transition-fast) border-color,
      var(--sl-transition-fast) box-shadow;
  }

  .switch__input {
    position: absolute;
    opacity: 0;
    padding: 0;
    margin: 0;
    pointer-events: none;
  }

  /* Hover */
  .switch:not(.switch--checked):not(.switch--disabled) .switch__control:hover {
    background-color: var(--sl-color-neutral-400);
    border-color: var(--sl-color-neutral-400);
  }

  .switch:not(.switch--checked):not(.switch--disabled) .switch__control:hover .switch__thumb {
    background-color: var(--sl-color-neutral-0);
    border-color: var(--sl-color-neutral-400);
  }

  /* Focus */
  .switch:not(.switch--checked):not(.switch--disabled) .switch__input:focus-visible ~ .switch__control {
    background-color: var(--sl-color-neutral-400);
    border-color: var(--sl-color-neutral-400);
  }

  .switch:not(.switch--checked):not(.switch--disabled) .switch__input:focus-visible ~ .switch__control .switch__thumb {
    background-color: var(--sl-color-neutral-0);
    border-color: var(--sl-color-primary-600);
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  /* Checked */
  .switch--checked .switch__control {
    background-color: var(--sl-color-primary-600);
    border-color: var(--sl-color-primary-600);
  }

  .switch--checked .switch__control .switch__thumb {
    background-color: var(--sl-color-neutral-0);
    border-color: var(--sl-color-primary-600);
    translate: calc((var(--width) - var(--height)) / 2);
  }

  /* Checked + hover */
  .switch.switch--checked:not(.switch--disabled) .switch__control:hover {
    background-color: var(--sl-color-primary-600);
    border-color: var(--sl-color-primary-600);
  }

  .switch.switch--checked:not(.switch--disabled) .switch__control:hover .switch__thumb {
    background-color: var(--sl-color-neutral-0);
    border-color: var(--sl-color-primary-600);
  }

  /* Checked + focus */
  .switch.switch--checked:not(.switch--disabled) .switch__input:focus-visible ~ .switch__control {
    background-color: var(--sl-color-primary-600);
    border-color: var(--sl-color-primary-600);
  }

  .switch.switch--checked:not(.switch--disabled) .switch__input:focus-visible ~ .switch__control .switch__thumb {
    background-color: var(--sl-color-neutral-0);
    border-color: var(--sl-color-primary-600);
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  /* Disabled */
  .switch--disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .switch__label {
    display: inline-block;
    line-height: var(--height);
    margin-inline-start: 0.5em;
    user-select: none;
    -webkit-user-select: none;
  }

  :host([required]) .switch__label::after {
    content: var(--sl-input-required-content);
    color: var(--sl-input-required-content-color);
    margin-inline-start: var(--sl-input-required-content-offset);
  }

  @media (forced-colors: active) {
    .switch.switch--checked:not(.switch--disabled) .switch__control:hover .switch__thumb,
    .switch--checked .switch__control .switch__thumb {
      background-color: ButtonText;
    }
  }
`,zt=class extends T{constructor(){super(...arguments),this.formControlController=new Ee(this,{value:e=>e.checked?e.value||"on":void 0,defaultValue:e=>e.defaultChecked,setValue:(e,t)=>e.checked=t}),this.hasSlotController=new Ot(this,"help-text"),this.hasFocus=!1,this.title="",this.name="",this.size="medium",this.disabled=!1,this.checked=!1,this.defaultChecked=!1,this.form="",this.required=!1,this.helpText=""}get validity(){return this.input.validity}get validationMessage(){return this.input.validationMessage}firstUpdated(){this.formControlController.updateValidity()}handleBlur(){this.hasFocus=!1,this.emit("sl-blur")}handleInput(){this.emit("sl-input")}handleInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}handleClick(){this.checked=!this.checked,this.emit("sl-change")}handleFocus(){this.hasFocus=!0,this.emit("sl-focus")}handleKeyDown(e){e.key==="ArrowLeft"&&(e.preventDefault(),this.checked=!1,this.emit("sl-change"),this.emit("sl-input")),e.key==="ArrowRight"&&(e.preventDefault(),this.checked=!0,this.emit("sl-change"),this.emit("sl-input"))}handleCheckedChange(){this.input.checked=this.checked,this.formControlController.updateValidity()}handleDisabledChange(){this.formControlController.setValidity(!0)}click(){this.input.click()}focus(e){this.input.focus(e)}blur(){this.input.blur()}checkValidity(){return this.input.checkValidity()}getForm(){return this.formControlController.getForm()}reportValidity(){return this.input.reportValidity()}setCustomValidity(e){this.input.setCustomValidity(e),this.formControlController.updateValidity()}render(){const e=this.hasSlotController.test("help-text"),t=this.helpText?!0:!!e;return x`
      <div
        class=${R({"form-control":!0,"form-control--small":this.size==="small","form-control--medium":this.size==="medium","form-control--large":this.size==="large","form-control--has-help-text":t})}
      >
        <label
          part="base"
          class=${R({switch:!0,"switch--checked":this.checked,"switch--disabled":this.disabled,"switch--focused":this.hasFocus,"switch--small":this.size==="small","switch--medium":this.size==="medium","switch--large":this.size==="large"})}
        >
          <input
            class="switch__input"
            type="checkbox"
            title=${this.title}
            name=${this.name}
            value=${E(this.value)}
            .checked=${io(this.checked)}
            .disabled=${this.disabled}
            .required=${this.required}
            role="switch"
            aria-checked=${this.checked?"true":"false"}
            aria-describedby="help-text"
            @click=${this.handleClick}
            @input=${this.handleInput}
            @invalid=${this.handleInvalid}
            @blur=${this.handleBlur}
            @focus=${this.handleFocus}
            @keydown=${this.handleKeyDown}
          />

          <span part="control" class="switch__control">
            <span part="thumb" class="switch__thumb"></span>
          </span>

          <div part="label" class="switch__label">
            <slot></slot>
          </div>
        </label>

        <div
          aria-hidden=${t?"false":"true"}
          class="form-control__help-text"
          id="help-text"
          part="form-control-help-text"
        >
          <slot name="help-text">${this.helpText}</slot>
        </div>
      </div>
    `}};zt.styles=[D,uo,ef];l([S('input[type="checkbox"]')],zt.prototype,"input",2);l([z()],zt.prototype,"hasFocus",2);l([p()],zt.prototype,"title",2);l([p()],zt.prototype,"name",2);l([p()],zt.prototype,"value",2);l([p({reflect:!0})],zt.prototype,"size",2);l([p({type:Boolean,reflect:!0})],zt.prototype,"disabled",2);l([p({type:Boolean,reflect:!0})],zt.prototype,"checked",2);l([Io("checked")],zt.prototype,"defaultChecked",2);l([p({reflect:!0})],zt.prototype,"form",2);l([p({type:Boolean,reflect:!0})],zt.prototype,"required",2);l([p({attribute:"help-text"})],zt.prototype,"helpText",2);l([C("checked",{waitUntilFirstUpdate:!0})],zt.prototype,"handleCheckedChange",1);l([C("disabled",{waitUntilFirstUpdate:!0})],zt.prototype,"handleDisabledChange",1);zt.define("sl-switch");Bi.define("sl-resize-observer");var of=O`
  :host {
    display: block;
  }

  /** The popup */
  .select {
    flex: 1 1 auto;
    display: inline-flex;
    width: 100%;
    position: relative;
    vertical-align: middle;
  }

  .select::part(popup) {
    z-index: var(--sl-z-index-dropdown);
  }

  .select[data-current-placement^='top']::part(popup) {
    transform-origin: bottom;
  }

  .select[data-current-placement^='bottom']::part(popup) {
    transform-origin: top;
  }

  /* Combobox */
  .select__combobox {
    flex: 1;
    display: flex;
    width: 100%;
    min-width: 0;
    position: relative;
    align-items: center;
    justify-content: start;
    font-family: var(--sl-input-font-family);
    font-weight: var(--sl-input-font-weight);
    letter-spacing: var(--sl-input-letter-spacing);
    vertical-align: middle;
    overflow: hidden;
    cursor: pointer;
    transition:
      var(--sl-transition-fast) color,
      var(--sl-transition-fast) border,
      var(--sl-transition-fast) box-shadow,
      var(--sl-transition-fast) background-color;
  }

  .select__display-input {
    position: relative;
    width: 100%;
    font: inherit;
    border: none;
    background: none;
    color: var(--sl-input-color);
    cursor: inherit;
    overflow: hidden;
    padding: 0;
    margin: 0;
    -webkit-appearance: none;
  }

  .select__display-input::placeholder {
    color: var(--sl-input-placeholder-color);
  }

  .select:not(.select--disabled):hover .select__display-input {
    color: var(--sl-input-color-hover);
  }

  .select__display-input:focus {
    outline: none;
  }

  /* Visually hide the display input when multiple is enabled */
  .select--multiple:not(.select--placeholder-visible) .select__display-input {
    position: absolute;
    z-index: -1;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    opacity: 0;
  }

  .select__value-input {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    padding: 0;
    margin: 0;
    opacity: 0;
    z-index: -1;
  }

  .select__tags {
    display: flex;
    flex: 1;
    align-items: center;
    flex-wrap: wrap;
    margin-inline-start: var(--sl-spacing-2x-small);
  }

  .select__tags::slotted(sl-tag) {
    cursor: pointer !important;
  }

  .select--disabled .select__tags,
  .select--disabled .select__tags::slotted(sl-tag) {
    cursor: not-allowed !important;
  }

  /* Standard selects */
  .select--standard .select__combobox {
    background-color: var(--sl-input-background-color);
    border: solid var(--sl-input-border-width) var(--sl-input-border-color);
  }

  .select--standard.select--disabled .select__combobox {
    background-color: var(--sl-input-background-color-disabled);
    border-color: var(--sl-input-border-color-disabled);
    color: var(--sl-input-color-disabled);
    opacity: 0.5;
    cursor: not-allowed;
    outline: none;
  }

  .select--standard:not(.select--disabled).select--open .select__combobox,
  .select--standard:not(.select--disabled).select--focused .select__combobox {
    background-color: var(--sl-input-background-color-focus);
    border-color: var(--sl-input-border-color-focus);
    box-shadow: 0 0 0 var(--sl-focus-ring-width) var(--sl-input-focus-ring-color);
  }

  /* Filled selects */
  .select--filled .select__combobox {
    border: none;
    background-color: var(--sl-input-filled-background-color);
    color: var(--sl-input-color);
  }

  .select--filled:hover:not(.select--disabled) .select__combobox {
    background-color: var(--sl-input-filled-background-color-hover);
  }

  .select--filled.select--disabled .select__combobox {
    background-color: var(--sl-input-filled-background-color-disabled);
    opacity: 0.5;
    cursor: not-allowed;
  }

  .select--filled:not(.select--disabled).select--open .select__combobox,
  .select--filled:not(.select--disabled).select--focused .select__combobox {
    background-color: var(--sl-input-filled-background-color-focus);
    outline: var(--sl-focus-ring);
  }

  /* Sizes */
  .select--small .select__combobox {
    border-radius: var(--sl-input-border-radius-small);
    font-size: var(--sl-input-font-size-small);
    min-height: var(--sl-input-height-small);
    padding-block: 0;
    padding-inline: var(--sl-input-spacing-small);
  }

  .select--small .select__clear {
    margin-inline-start: var(--sl-input-spacing-small);
  }

  .select--small .select__prefix::slotted(*) {
    margin-inline-end: var(--sl-input-spacing-small);
  }

  .select--small.select--multiple:not(.select--placeholder-visible) .select__prefix::slotted(*) {
    margin-inline-start: var(--sl-input-spacing-small);
  }

  .select--small.select--multiple:not(.select--placeholder-visible) .select__combobox {
    padding-block: 2px;
    padding-inline-start: 0;
  }

  .select--small .select__tags {
    gap: 2px;
  }

  .select--medium .select__combobox {
    border-radius: var(--sl-input-border-radius-medium);
    font-size: var(--sl-input-font-size-medium);
    min-height: var(--sl-input-height-medium);
    padding-block: 0;
    padding-inline: var(--sl-input-spacing-medium);
  }

  .select--medium .select__clear {
    margin-inline-start: var(--sl-input-spacing-medium);
  }

  .select--medium .select__prefix::slotted(*) {
    margin-inline-end: var(--sl-input-spacing-medium);
  }

  .select--medium.select--multiple:not(.select--placeholder-visible) .select__prefix::slotted(*) {
    margin-inline-start: var(--sl-input-spacing-medium);
  }

  .select--medium.select--multiple:not(.select--placeholder-visible) .select__combobox {
    padding-inline-start: 0;
    padding-block: 3px;
  }

  .select--medium .select__tags {
    gap: 3px;
  }

  .select--large .select__combobox {
    border-radius: var(--sl-input-border-radius-large);
    font-size: var(--sl-input-font-size-large);
    min-height: var(--sl-input-height-large);
    padding-block: 0;
    padding-inline: var(--sl-input-spacing-large);
  }

  .select--large .select__clear {
    margin-inline-start: var(--sl-input-spacing-large);
  }

  .select--large .select__prefix::slotted(*) {
    margin-inline-end: var(--sl-input-spacing-large);
  }

  .select--large.select--multiple:not(.select--placeholder-visible) .select__prefix::slotted(*) {
    margin-inline-start: var(--sl-input-spacing-large);
  }

  .select--large.select--multiple:not(.select--placeholder-visible) .select__combobox {
    padding-inline-start: 0;
    padding-block: 4px;
  }

  .select--large .select__tags {
    gap: 4px;
  }

  /* Pills */
  .select--pill.select--small .select__combobox {
    border-radius: var(--sl-input-height-small);
  }

  .select--pill.select--medium .select__combobox {
    border-radius: var(--sl-input-height-medium);
  }

  .select--pill.select--large .select__combobox {
    border-radius: var(--sl-input-height-large);
  }

  /* Prefix and Suffix */
  .select__prefix,
  .select__suffix {
    flex: 0;
    display: inline-flex;
    align-items: center;
    color: var(--sl-input-placeholder-color);
  }

  .select__suffix::slotted(*) {
    margin-inline-start: var(--sl-spacing-small);
  }

  /* Clear button */
  .select__clear {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-size: inherit;
    color: var(--sl-input-icon-color);
    border: none;
    background: none;
    padding: 0;
    transition: var(--sl-transition-fast) color;
    cursor: pointer;
  }

  .select__clear:hover {
    color: var(--sl-input-icon-color-hover);
  }

  .select__clear:focus {
    outline: none;
  }

  /* Expand icon */
  .select__expand-icon {
    flex: 0 0 auto;
    display: flex;
    align-items: center;
    transition: var(--sl-transition-medium) rotate ease;
    rotate: 0;
    margin-inline-start: var(--sl-spacing-small);
  }

  .select--open .select__expand-icon {
    rotate: -180deg;
  }

  /* Listbox */
  .select__listbox {
    display: block;
    position: relative;
    font-family: var(--sl-font-sans);
    font-size: var(--sl-font-size-medium);
    font-weight: var(--sl-font-weight-normal);
    box-shadow: var(--sl-shadow-large);
    background: var(--sl-panel-background-color);
    border: solid var(--sl-panel-border-width) var(--sl-panel-border-color);
    border-radius: var(--sl-border-radius-medium);
    padding-block: var(--sl-spacing-x-small);
    padding-inline: 0;
    overflow: auto;
    overscroll-behavior: none;

    /* Make sure it adheres to the popup's auto size */
    max-width: var(--auto-size-available-width);
    max-height: var(--auto-size-available-height);
  }

  .select__listbox ::slotted(sl-divider) {
    --spacing: var(--sl-spacing-x-small);
  }

  .select__listbox ::slotted(small) {
    display: block;
    font-size: var(--sl-font-size-small);
    font-weight: var(--sl-font-weight-semibold);
    color: var(--sl-color-neutral-500);
    padding-block: var(--sl-spacing-2x-small);
    padding-inline: var(--sl-spacing-x-large);
  }
`;/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */let Cr=class extends Ri{constructor(t){if(super(t),this.it=et,t.type!==ce.CHILD)throw Error(this.constructor.directiveName+"() can only be used in child bindings")}render(t){if(t===et||t==null)return this._t=void 0,this.it=t;if(t===Bt)return t;if(typeof t!="string")throw Error(this.constructor.directiveName+"() called with a non-string value");if(t===this.it)return this._t;this.it=t;const o=[t];return o.raw=o,this._t={_$litType$:this.constructor.resultType,strings:o,values:[]}}};Cr.directiveName="unsafeHTML",Cr.resultType=1;const es=Li(Cr);var N=class extends T{constructor(){super(...arguments),this.formControlController=new Ee(this,{assumeInteractionOn:["sl-blur","sl-input"]}),this.hasSlotController=new Ot(this,"help-text","label"),this.localize=new j(this),this.typeToSelectString="",this.hasFocus=!1,this.displayLabel="",this.selectedOptions=[],this.valueHasChanged=!1,this.name="",this._value="",this.defaultValue="",this.size="medium",this.placeholder="",this.multiple=!1,this.maxOptionsVisible=3,this.disabled=!1,this.clearable=!1,this.open=!1,this.hoist=!1,this.filled=!1,this.pill=!1,this.label="",this.placement="bottom",this.helpText="",this.form="",this.required=!1,this.getTag=e=>x`
      <sl-tag
        part="tag"
        exportparts="
              base:tag__base,
              content:tag__content,
              remove-button:tag__remove-button,
              remove-button__base:tag__remove-button__base
            "
        ?pill=${this.pill}
        size=${this.size}
        removable
        @sl-remove=${t=>this.handleTagRemove(t,e)}
      >
        ${e.getTextLabel()}
      </sl-tag>
    `,this.handleDocumentFocusIn=e=>{const t=e.composedPath();this&&!t.includes(this)&&this.hide()},this.handleDocumentKeyDown=e=>{const t=e.target,o=t.closest(".select__clear")!==null,i=t.closest("sl-icon-button")!==null;if(!(o||i)){if(e.key==="Escape"&&this.open&&!this.closeWatcher&&(e.preventDefault(),e.stopPropagation(),this.hide(),this.displayInput.focus({preventScroll:!0})),e.key==="Enter"||e.key===" "&&this.typeToSelectString===""){if(e.preventDefault(),e.stopImmediatePropagation(),!this.open){this.show();return}this.currentOption&&!this.currentOption.disabled&&(this.valueHasChanged=!0,this.multiple?this.toggleOptionSelection(this.currentOption):this.setSelectedOptions(this.currentOption),this.updateComplete.then(()=>{this.emit("sl-input"),this.emit("sl-change")}),this.multiple||(this.hide(),this.displayInput.focus({preventScroll:!0})));return}if(["ArrowUp","ArrowDown","Home","End"].includes(e.key)){const s=this.getAllOptions(),r=s.indexOf(this.currentOption);let a=Math.max(0,r);if(e.preventDefault(),!this.open&&(this.show(),this.currentOption))return;e.key==="ArrowDown"?(a=r+1,a>s.length-1&&(a=0)):e.key==="ArrowUp"?(a=r-1,a<0&&(a=s.length-1)):e.key==="Home"?a=0:e.key==="End"&&(a=s.length-1),this.setCurrentOption(s[a])}if(e.key&&e.key.length===1||e.key==="Backspace"){const s=this.getAllOptions();if(e.metaKey||e.ctrlKey||e.altKey)return;if(!this.open){if(e.key==="Backspace")return;this.show()}e.stopPropagation(),e.preventDefault(),clearTimeout(this.typeToSelectTimeout),this.typeToSelectTimeout=window.setTimeout(()=>this.typeToSelectString="",1e3),e.key==="Backspace"?this.typeToSelectString=this.typeToSelectString.slice(0,-1):this.typeToSelectString+=e.key.toLowerCase();for(const r of s)if(r.getTextLabel().toLowerCase().startsWith(this.typeToSelectString)){this.setCurrentOption(r);break}}}},this.handleDocumentMouseDown=e=>{const t=e.composedPath();this&&!t.includes(this)&&this.hide()}}get value(){return this._value}set value(e){this.multiple?e=Array.isArray(e)?e:e.split(" "):e=Array.isArray(e)?e.join(" "):e,this._value!==e&&(this.valueHasChanged=!0,this._value=e)}get validity(){return this.valueInput.validity}get validationMessage(){return this.valueInput.validationMessage}connectedCallback(){super.connectedCallback(),setTimeout(()=>{this.handleDefaultSlotChange()}),this.open=!1}addOpenListeners(){var e;document.addEventListener("focusin",this.handleDocumentFocusIn),document.addEventListener("keydown",this.handleDocumentKeyDown),document.addEventListener("mousedown",this.handleDocumentMouseDown),this.getRootNode()!==document&&this.getRootNode().addEventListener("focusin",this.handleDocumentFocusIn),"CloseWatcher"in window&&((e=this.closeWatcher)==null||e.destroy(),this.closeWatcher=new CloseWatcher,this.closeWatcher.onclose=()=>{this.open&&(this.hide(),this.displayInput.focus({preventScroll:!0}))})}removeOpenListeners(){var e;document.removeEventListener("focusin",this.handleDocumentFocusIn),document.removeEventListener("keydown",this.handleDocumentKeyDown),document.removeEventListener("mousedown",this.handleDocumentMouseDown),this.getRootNode()!==document&&this.getRootNode().removeEventListener("focusin",this.handleDocumentFocusIn),(e=this.closeWatcher)==null||e.destroy()}handleFocus(){this.hasFocus=!0,this.displayInput.setSelectionRange(0,0),this.emit("sl-focus")}handleBlur(){this.hasFocus=!1,this.emit("sl-blur")}handleLabelClick(){this.displayInput.focus()}handleComboboxMouseDown(e){const o=e.composedPath().some(i=>i instanceof Element&&i.tagName.toLowerCase()==="sl-icon-button");this.disabled||o||(e.preventDefault(),this.displayInput.focus({preventScroll:!0}),this.open=!this.open)}handleComboboxKeyDown(e){e.key!=="Tab"&&(e.stopPropagation(),this.handleDocumentKeyDown(e))}handleClearClick(e){e.stopPropagation(),this.valueHasChanged=!0,this.value!==""&&(this.setSelectedOptions([]),this.displayInput.focus({preventScroll:!0}),this.updateComplete.then(()=>{this.emit("sl-clear"),this.emit("sl-input"),this.emit("sl-change")}))}handleClearMouseDown(e){e.stopPropagation(),e.preventDefault()}handleOptionClick(e){const o=e.target.closest("sl-option"),i=this.value;o&&!o.disabled&&(this.valueHasChanged=!0,this.multiple?this.toggleOptionSelection(o):this.setSelectedOptions(o),this.updateComplete.then(()=>this.displayInput.focus({preventScroll:!0})),this.value!==i&&this.updateComplete.then(()=>{this.emit("sl-input"),this.emit("sl-change")}),this.multiple||(this.hide(),this.displayInput.focus({preventScroll:!0})))}handleDefaultSlotChange(){customElements.get("sl-option")||customElements.whenDefined("sl-option").then(()=>this.handleDefaultSlotChange());const e=this.getAllOptions(),t=this.valueHasChanged?this.value:this.defaultValue,o=Array.isArray(t)?t:[t],i=[];e.forEach(s=>i.push(s.value)),this.setSelectedOptions(e.filter(s=>o.includes(s.value)))}handleTagRemove(e,t){e.stopPropagation(),this.valueHasChanged=!0,this.disabled||(this.toggleOptionSelection(t,!1),this.updateComplete.then(()=>{this.emit("sl-input"),this.emit("sl-change")}))}getAllOptions(){return[...this.querySelectorAll("sl-option")]}getFirstOption(){return this.querySelector("sl-option")}setCurrentOption(e){this.getAllOptions().forEach(o=>{o.current=!1,o.tabIndex=-1}),e&&(this.currentOption=e,e.current=!0,e.tabIndex=0,e.focus())}setSelectedOptions(e){const t=this.getAllOptions(),o=Array.isArray(e)?e:[e];t.forEach(i=>i.selected=!1),o.length&&o.forEach(i=>i.selected=!0),this.selectionChanged()}toggleOptionSelection(e,t){t===!0||t===!1?e.selected=t:e.selected=!e.selected,this.selectionChanged()}selectionChanged(){var e,t,o;const i=this.getAllOptions();this.selectedOptions=i.filter(r=>r.selected);const s=this.valueHasChanged;if(this.multiple)this.value=this.selectedOptions.map(r=>r.value),this.placeholder&&this.value.length===0?this.displayLabel="":this.displayLabel=this.localize.term("numOptionsSelected",this.selectedOptions.length);else{const r=this.selectedOptions[0];this.value=(e=r==null?void 0:r.value)!=null?e:"",this.displayLabel=(o=(t=r==null?void 0:r.getTextLabel)==null?void 0:t.call(r))!=null?o:""}this.valueHasChanged=s,this.updateComplete.then(()=>{this.formControlController.updateValidity()})}get tags(){return this.selectedOptions.map((e,t)=>{if(t<this.maxOptionsVisible||this.maxOptionsVisible<=0){const o=this.getTag(e,t);return x`<div @sl-remove=${i=>this.handleTagRemove(i,e)}>
          ${typeof o=="string"?es(o):o}
        </div>`}else if(t===this.maxOptionsVisible)return x`<sl-tag size=${this.size}>+${this.selectedOptions.length-t}</sl-tag>`;return x``})}handleInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}handleDisabledChange(){this.disabled&&(this.open=!1,this.handleOpenChange())}attributeChangedCallback(e,t,o){if(super.attributeChangedCallback(e,t,o),e==="value"){const i=this.valueHasChanged;this.value=this.defaultValue,this.valueHasChanged=i}}handleValueChange(){if(!this.valueHasChanged){const o=this.valueHasChanged;this.value=this.defaultValue,this.valueHasChanged=o}const e=this.getAllOptions(),t=Array.isArray(this.value)?this.value:[this.value];this.setSelectedOptions(e.filter(o=>t.includes(o.value)))}async handleOpenChange(){if(this.open&&!this.disabled){this.setCurrentOption(this.selectedOptions[0]||this.getFirstOption()),this.emit("sl-show"),this.addOpenListeners(),await ut(this),this.listbox.hidden=!1,this.popup.active=!0,requestAnimationFrame(()=>{this.setCurrentOption(this.currentOption)});const{keyframes:e,options:t}=ot(this,"select.show",{dir:this.localize.dir()});await at(this.popup.popup,e,t),this.currentOption&&$r(this.currentOption,this.listbox,"vertical","auto"),this.emit("sl-after-show")}else{this.emit("sl-hide"),this.removeOpenListeners(),await ut(this);const{keyframes:e,options:t}=ot(this,"select.hide",{dir:this.localize.dir()});await at(this.popup.popup,e,t),this.listbox.hidden=!0,this.popup.active=!1,this.emit("sl-after-hide")}}async show(){if(this.open||this.disabled){this.open=!1;return}return this.open=!0,Pt(this,"sl-after-show")}async hide(){if(!this.open||this.disabled){this.open=!1;return}return this.open=!1,Pt(this,"sl-after-hide")}checkValidity(){return this.valueInput.checkValidity()}getForm(){return this.formControlController.getForm()}reportValidity(){return this.valueInput.reportValidity()}setCustomValidity(e){this.valueInput.setCustomValidity(e),this.formControlController.updateValidity()}focus(e){this.displayInput.focus(e)}blur(){this.displayInput.blur()}render(){const e=this.hasSlotController.test("label"),t=this.hasSlotController.test("help-text"),o=this.label?!0:!!e,i=this.helpText?!0:!!t,s=this.clearable&&!this.disabled&&this.value.length>0,r=this.placeholder&&this.value&&this.value.length<=0;return x`
      <div
        part="form-control"
        class=${R({"form-control":!0,"form-control--small":this.size==="small","form-control--medium":this.size==="medium","form-control--large":this.size==="large","form-control--has-label":o,"form-control--has-help-text":i})}
      >
        <label
          id="label"
          part="form-control-label"
          class="form-control__label"
          aria-hidden=${o?"false":"true"}
          @click=${this.handleLabelClick}
        >
          <slot name="label">${this.label}</slot>
        </label>

        <div part="form-control-input" class="form-control-input">
          <sl-popup
            class=${R({select:!0,"select--standard":!0,"select--filled":this.filled,"select--pill":this.pill,"select--open":this.open,"select--disabled":this.disabled,"select--multiple":this.multiple,"select--focused":this.hasFocus,"select--placeholder-visible":r,"select--top":this.placement==="top","select--bottom":this.placement==="bottom","select--small":this.size==="small","select--medium":this.size==="medium","select--large":this.size==="large"})}
            placement=${this.placement}
            strategy=${this.hoist?"fixed":"absolute"}
            flip
            shift
            sync="width"
            auto-size="vertical"
            auto-size-padding="10"
          >
            <div
              part="combobox"
              class="select__combobox"
              slot="anchor"
              @keydown=${this.handleComboboxKeyDown}
              @mousedown=${this.handleComboboxMouseDown}
            >
              <slot part="prefix" name="prefix" class="select__prefix"></slot>

              <input
                part="display-input"
                class="select__display-input"
                type="text"
                placeholder=${this.placeholder}
                .disabled=${this.disabled}
                .value=${this.displayLabel}
                autocomplete="off"
                spellcheck="false"
                autocapitalize="off"
                readonly
                aria-controls="listbox"
                aria-expanded=${this.open?"true":"false"}
                aria-haspopup="listbox"
                aria-labelledby="label"
                aria-disabled=${this.disabled?"true":"false"}
                aria-describedby="help-text"
                role="combobox"
                tabindex="0"
                @focus=${this.handleFocus}
                @blur=${this.handleBlur}
              />

              ${this.multiple?x`<div part="tags" class="select__tags">${this.tags}</div>`:""}

              <input
                class="select__value-input"
                type="text"
                ?disabled=${this.disabled}
                ?required=${this.required}
                .value=${Array.isArray(this.value)?this.value.join(", "):this.value}
                tabindex="-1"
                aria-hidden="true"
                @focus=${()=>this.focus()}
                @invalid=${this.handleInvalid}
              />

              ${s?x`
                    <button
                      part="clear-button"
                      class="select__clear"
                      type="button"
                      aria-label=${this.localize.term("clearEntry")}
                      @mousedown=${this.handleClearMouseDown}
                      @click=${this.handleClearClick}
                      tabindex="-1"
                    >
                      <slot name="clear-icon">
                        <sl-icon name="x-circle-fill" library="system"></sl-icon>
                      </slot>
                    </button>
                  `:""}

              <slot name="suffix" part="suffix" class="select__suffix"></slot>

              <slot name="expand-icon" part="expand-icon" class="select__expand-icon">
                <sl-icon library="system" name="chevron-down"></sl-icon>
              </slot>
            </div>

            <div
              id="listbox"
              role="listbox"
              aria-expanded=${this.open?"true":"false"}
              aria-multiselectable=${this.multiple?"true":"false"}
              aria-labelledby="label"
              part="listbox"
              class="select__listbox"
              tabindex="-1"
              @mouseup=${this.handleOptionClick}
              @slotchange=${this.handleDefaultSlotChange}
            >
              <slot></slot>
            </div>
          </sl-popup>
        </div>

        <div
          part="form-control-help-text"
          id="help-text"
          class="form-control__help-text"
          aria-hidden=${i?"false":"true"}
        >
          <slot name="help-text">${this.helpText}</slot>
        </div>
      </div>
    `}};N.styles=[D,uo,of];N.dependencies={"sl-icon":J,"sl-popup":Y,"sl-tag":Ue};l([S(".select")],N.prototype,"popup",2);l([S(".select__combobox")],N.prototype,"combobox",2);l([S(".select__display-input")],N.prototype,"displayInput",2);l([S(".select__value-input")],N.prototype,"valueInput",2);l([S(".select__listbox")],N.prototype,"listbox",2);l([z()],N.prototype,"hasFocus",2);l([z()],N.prototype,"displayLabel",2);l([z()],N.prototype,"currentOption",2);l([z()],N.prototype,"selectedOptions",2);l([z()],N.prototype,"valueHasChanged",2);l([p()],N.prototype,"name",2);l([z()],N.prototype,"value",1);l([p({attribute:"value"})],N.prototype,"defaultValue",2);l([p({reflect:!0})],N.prototype,"size",2);l([p()],N.prototype,"placeholder",2);l([p({type:Boolean,reflect:!0})],N.prototype,"multiple",2);l([p({attribute:"max-options-visible",type:Number})],N.prototype,"maxOptionsVisible",2);l([p({type:Boolean,reflect:!0})],N.prototype,"disabled",2);l([p({type:Boolean})],N.prototype,"clearable",2);l([p({type:Boolean,reflect:!0})],N.prototype,"open",2);l([p({type:Boolean})],N.prototype,"hoist",2);l([p({type:Boolean,reflect:!0})],N.prototype,"filled",2);l([p({type:Boolean,reflect:!0})],N.prototype,"pill",2);l([p()],N.prototype,"label",2);l([p({reflect:!0})],N.prototype,"placement",2);l([p({attribute:"help-text"})],N.prototype,"helpText",2);l([p({reflect:!0})],N.prototype,"form",2);l([p({type:Boolean,reflect:!0})],N.prototype,"required",2);l([p()],N.prototype,"getTag",2);l([C("disabled",{waitUntilFirstUpdate:!0})],N.prototype,"handleDisabledChange",1);l([C(["defaultValue","value"],{waitUntilFirstUpdate:!0})],N.prototype,"handleValueChange",1);l([C("open",{waitUntilFirstUpdate:!0})],N.prototype,"handleOpenChange",1);K("select.show",{keyframes:[{opacity:0,scale:.9},{opacity:1,scale:1}],options:{duration:100,easing:"ease"}});K("select.hide",{keyframes:[{opacity:1,scale:1},{opacity:0,scale:.9}],options:{duration:100,easing:"ease"}});N.define("sl-select");Di.define("sl-spinner");var sf=O`
  :host {
    --thumb-size: 20px;
    --tooltip-offset: 10px;
    --track-color-active: var(--sl-color-neutral-200);
    --track-color-inactive: var(--sl-color-neutral-200);
    --track-active-offset: 0%;
    --track-height: 6px;

    display: block;
  }

  .range {
    position: relative;
  }

  .range__control {
    --percent: 0%;
    -webkit-appearance: none;
    border-radius: 3px;
    width: 100%;
    height: var(--track-height);
    background: transparent;
    line-height: var(--sl-input-height-medium);
    vertical-align: middle;
    margin: 0;

    background-image: linear-gradient(
      to right,
      var(--track-color-inactive) 0%,
      var(--track-color-inactive) min(var(--percent), var(--track-active-offset)),
      var(--track-color-active) min(var(--percent), var(--track-active-offset)),
      var(--track-color-active) max(var(--percent), var(--track-active-offset)),
      var(--track-color-inactive) max(var(--percent), var(--track-active-offset)),
      var(--track-color-inactive) 100%
    );
  }

  .range--rtl .range__control {
    background-image: linear-gradient(
      to left,
      var(--track-color-inactive) 0%,
      var(--track-color-inactive) min(var(--percent), var(--track-active-offset)),
      var(--track-color-active) min(var(--percent), var(--track-active-offset)),
      var(--track-color-active) max(var(--percent), var(--track-active-offset)),
      var(--track-color-inactive) max(var(--percent), var(--track-active-offset)),
      var(--track-color-inactive) 100%
    );
  }

  /* Webkit */
  .range__control::-webkit-slider-runnable-track {
    width: 100%;
    height: var(--track-height);
    border-radius: 3px;
    border: none;
  }

  .range__control::-webkit-slider-thumb {
    border: none;
    width: var(--thumb-size);
    height: var(--thumb-size);
    border-radius: 50%;
    background-color: var(--sl-color-primary-600);
    border: solid var(--sl-input-border-width) var(--sl-color-primary-600);
    -webkit-appearance: none;
    margin-top: calc(var(--thumb-size) / -2 + var(--track-height) / 2);
    cursor: pointer;
  }

  .range__control:enabled::-webkit-slider-thumb:hover {
    background-color: var(--sl-color-primary-500);
    border-color: var(--sl-color-primary-500);
  }

  .range__control:enabled:focus-visible::-webkit-slider-thumb {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  .range__control:enabled::-webkit-slider-thumb:active {
    background-color: var(--sl-color-primary-500);
    border-color: var(--sl-color-primary-500);
    cursor: grabbing;
  }

  /* Firefox */
  .range__control::-moz-focus-outer {
    border: 0;
  }

  .range__control::-moz-range-progress {
    background-color: var(--track-color-active);
    border-radius: 3px;
    height: var(--track-height);
  }

  .range__control::-moz-range-track {
    width: 100%;
    height: var(--track-height);
    background-color: var(--track-color-inactive);
    border-radius: 3px;
    border: none;
  }

  .range__control::-moz-range-thumb {
    border: none;
    height: var(--thumb-size);
    width: var(--thumb-size);
    border-radius: 50%;
    background-color: var(--sl-color-primary-600);
    border-color: var(--sl-color-primary-600);
    transition:
      var(--sl-transition-fast) border-color,
      var(--sl-transition-fast) background-color,
      var(--sl-transition-fast) color,
      var(--sl-transition-fast) box-shadow;
    cursor: pointer;
  }

  .range__control:enabled::-moz-range-thumb:hover {
    background-color: var(--sl-color-primary-500);
    border-color: var(--sl-color-primary-500);
  }

  .range__control:enabled:focus-visible::-moz-range-thumb {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  .range__control:enabled::-moz-range-thumb:active {
    background-color: var(--sl-color-primary-500);
    border-color: var(--sl-color-primary-500);
    cursor: grabbing;
  }

  /* States */
  .range__control:focus-visible {
    outline: none;
  }

  .range__control:disabled {
    opacity: 0.5;
  }

  .range__control:disabled::-webkit-slider-thumb {
    cursor: not-allowed;
  }

  .range__control:disabled::-moz-range-thumb {
    cursor: not-allowed;
  }

  /* Tooltip output */
  .range__tooltip {
    position: absolute;
    z-index: var(--sl-z-index-tooltip);
    left: 0;
    border-radius: var(--sl-tooltip-border-radius);
    background-color: var(--sl-tooltip-background-color);
    font-family: var(--sl-tooltip-font-family);
    font-size: var(--sl-tooltip-font-size);
    font-weight: var(--sl-tooltip-font-weight);
    line-height: var(--sl-tooltip-line-height);
    color: var(--sl-tooltip-color);
    opacity: 0;
    padding: var(--sl-tooltip-padding);
    transition: var(--sl-transition-fast) opacity;
    pointer-events: none;
  }

  .range__tooltip:after {
    content: '';
    position: absolute;
    width: 0;
    height: 0;
    left: 50%;
    translate: calc(-1 * var(--sl-tooltip-arrow-size));
  }

  .range--tooltip-visible .range__tooltip {
    opacity: 1;
  }

  /* Tooltip on top */
  .range--tooltip-top .range__tooltip {
    top: calc(-1 * var(--thumb-size) - var(--tooltip-offset));
  }

  .range--tooltip-top .range__tooltip:after {
    border-top: var(--sl-tooltip-arrow-size) solid var(--sl-tooltip-background-color);
    border-left: var(--sl-tooltip-arrow-size) solid transparent;
    border-right: var(--sl-tooltip-arrow-size) solid transparent;
    top: 100%;
  }

  /* Tooltip on bottom */
  .range--tooltip-bottom .range__tooltip {
    bottom: calc(-1 * var(--thumb-size) - var(--tooltip-offset));
  }

  .range--tooltip-bottom .range__tooltip:after {
    border-bottom: var(--sl-tooltip-arrow-size) solid var(--sl-tooltip-background-color);
    border-left: var(--sl-tooltip-arrow-size) solid transparent;
    border-right: var(--sl-tooltip-arrow-size) solid transparent;
    bottom: 100%;
  }

  @media (forced-colors: active) {
    .range__control,
    .range__tooltip {
      border: solid 1px transparent;
    }

    .range__control::-webkit-slider-thumb {
      border: solid 1px transparent;
    }

    .range__control::-moz-range-thumb {
      border: solid 1px transparent;
    }

    .range__tooltip:after {
      display: none;
    }
  }
`,st=class extends T{constructor(){super(...arguments),this.formControlController=new Ee(this),this.hasSlotController=new Ot(this,"help-text","label"),this.localize=new j(this),this.hasFocus=!1,this.hasTooltip=!1,this.title="",this.name="",this.value=0,this.label="",this.helpText="",this.disabled=!1,this.min=0,this.max=100,this.step=1,this.tooltip="top",this.tooltipFormatter=e=>e.toString(),this.form="",this.defaultValue=0}get validity(){return this.input.validity}get validationMessage(){return this.input.validationMessage}connectedCallback(){super.connectedCallback(),this.resizeObserver=new ResizeObserver(()=>this.syncRange()),this.value<this.min&&(this.value=this.min),this.value>this.max&&(this.value=this.max),this.updateComplete.then(()=>{this.syncRange(),this.resizeObserver.observe(this.input)})}disconnectedCallback(){var e;super.disconnectedCallback(),(e=this.resizeObserver)==null||e.unobserve(this.input)}handleChange(){this.emit("sl-change")}handleInput(){this.value=parseFloat(this.input.value),this.emit("sl-input"),this.syncRange()}handleBlur(){this.hasFocus=!1,this.hasTooltip=!1,this.emit("sl-blur")}handleFocus(){this.hasFocus=!0,this.hasTooltip=!0,this.emit("sl-focus")}handleThumbDragStart(){this.hasTooltip=!0}handleThumbDragEnd(){this.hasTooltip=!1}syncProgress(e){this.input.style.setProperty("--percent",`${e*100}%`)}syncTooltip(e){if(this.output!==null){const t=this.input.offsetWidth,o=this.output.offsetWidth,i=getComputedStyle(this.input).getPropertyValue("--thumb-size"),s=this.localize.dir()==="rtl",r=t*e;if(s){const a=`${t-r}px + ${e} * ${i}`;this.output.style.translate=`calc((${a} - ${o/2}px - ${i} / 2))`}else{const a=`${r}px - ${e} * ${i}`;this.output.style.translate=`calc(${a} - ${o/2}px + ${i} / 2)`}}}handleValueChange(){this.formControlController.updateValidity(),this.input.value=this.value.toString(),this.value=parseFloat(this.input.value),this.syncRange()}handleDisabledChange(){this.formControlController.setValidity(this.disabled)}syncRange(){const e=Math.max(0,(this.value-this.min)/(this.max-this.min));this.syncProgress(e),this.tooltip!=="none"&&this.hasTooltip&&this.updateComplete.then(()=>this.syncTooltip(e))}handleInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}focus(e){this.input.focus(e)}blur(){this.input.blur()}stepUp(){this.input.stepUp(),this.value!==Number(this.input.value)&&(this.value=Number(this.input.value))}stepDown(){this.input.stepDown(),this.value!==Number(this.input.value)&&(this.value=Number(this.input.value))}checkValidity(){return this.input.checkValidity()}getForm(){return this.formControlController.getForm()}reportValidity(){return this.input.reportValidity()}setCustomValidity(e){this.input.setCustomValidity(e),this.formControlController.updateValidity()}render(){const e=this.hasSlotController.test("label"),t=this.hasSlotController.test("help-text"),o=this.label?!0:!!e,i=this.helpText?!0:!!t;return x`
      <div
        part="form-control"
        class=${R({"form-control":!0,"form-control--medium":!0,"form-control--has-label":o,"form-control--has-help-text":i})}
      >
        <label
          part="form-control-label"
          class="form-control__label"
          for="input"
          aria-hidden=${o?"false":"true"}
        >
          <slot name="label">${this.label}</slot>
        </label>

        <div part="form-control-input" class="form-control-input">
          <div
            part="base"
            class=${R({range:!0,"range--disabled":this.disabled,"range--focused":this.hasFocus,"range--rtl":this.localize.dir()==="rtl","range--tooltip-visible":this.hasTooltip,"range--tooltip-top":this.tooltip==="top","range--tooltip-bottom":this.tooltip==="bottom"})}
            @mousedown=${this.handleThumbDragStart}
            @mouseup=${this.handleThumbDragEnd}
            @touchstart=${this.handleThumbDragStart}
            @touchend=${this.handleThumbDragEnd}
          >
            <input
              part="input"
              id="input"
              class="range__control"
              title=${this.title}
              type="range"
              name=${E(this.name)}
              ?disabled=${this.disabled}
              min=${E(this.min)}
              max=${E(this.max)}
              step=${E(this.step)}
              .value=${io(this.value.toString())}
              aria-describedby="help-text"
              @change=${this.handleChange}
              @focus=${this.handleFocus}
              @input=${this.handleInput}
              @invalid=${this.handleInvalid}
              @blur=${this.handleBlur}
            />
            ${this.tooltip!=="none"&&!this.disabled?x`
                  <output part="tooltip" class="range__tooltip">
                    ${typeof this.tooltipFormatter=="function"?this.tooltipFormatter(this.value):this.value}
                  </output>
                `:""}
          </div>
        </div>

        <div
          part="form-control-help-text"
          id="help-text"
          class="form-control__help-text"
          aria-hidden=${i?"false":"true"}
        >
          <slot name="help-text">${this.helpText}</slot>
        </div>
      </div>
    `}};st.styles=[D,uo,sf];l([S(".range__control")],st.prototype,"input",2);l([S(".range__tooltip")],st.prototype,"output",2);l([z()],st.prototype,"hasFocus",2);l([z()],st.prototype,"hasTooltip",2);l([p()],st.prototype,"title",2);l([p()],st.prototype,"name",2);l([p({type:Number})],st.prototype,"value",2);l([p()],st.prototype,"label",2);l([p({attribute:"help-text"})],st.prototype,"helpText",2);l([p({type:Boolean,reflect:!0})],st.prototype,"disabled",2);l([p({type:Number})],st.prototype,"min",2);l([p({type:Number})],st.prototype,"max",2);l([p({type:Number})],st.prototype,"step",2);l([p()],st.prototype,"tooltip",2);l([p({attribute:!1})],st.prototype,"tooltipFormatter",2);l([p({reflect:!0})],st.prototype,"form",2);l([Io()],st.prototype,"defaultValue",2);l([Ai({passive:!0})],st.prototype,"handleThumbDragStart",1);l([C("value",{waitUntilFirstUpdate:!0})],st.prototype,"handleValueChange",1);l([C("disabled",{waitUntilFirstUpdate:!0})],st.prototype,"handleDisabledChange",1);l([C("hasTooltip",{waitUntilFirstUpdate:!0})],st.prototype,"syncRange",1);st.define("sl-range");var rf=O`
  :host {
    --symbol-color: var(--sl-color-neutral-300);
    --symbol-color-active: var(--sl-color-amber-500);
    --symbol-size: 1.2rem;
    --symbol-spacing: var(--sl-spacing-3x-small);

    display: inline-flex;
  }

  .rating {
    position: relative;
    display: inline-flex;
    border-radius: var(--sl-border-radius-medium);
    vertical-align: middle;
  }

  .rating:focus {
    outline: none;
  }

  .rating:focus-visible {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  .rating__symbols {
    display: inline-flex;
    position: relative;
    font-size: var(--symbol-size);
    line-height: 0;
    color: var(--symbol-color);
    white-space: nowrap;
    cursor: pointer;
  }

  .rating__symbols > * {
    padding: var(--symbol-spacing);
  }

  .rating__symbol--active,
  .rating__partial--filled {
    color: var(--symbol-color-active);
  }

  .rating__partial-symbol-container {
    position: relative;
  }

  .rating__partial--filled {
    position: absolute;
    top: var(--symbol-spacing);
    left: var(--symbol-spacing);
  }

  .rating__symbol {
    transition: var(--sl-transition-fast) scale;
    pointer-events: none;
  }

  .rating__symbol--hover {
    scale: 1.2;
  }

  .rating--disabled .rating__symbols,
  .rating--readonly .rating__symbols {
    cursor: default;
  }

  .rating--disabled .rating__symbol--hover,
  .rating--readonly .rating__symbol--hover {
    scale: none;
  }

  .rating--disabled {
    opacity: 0.5;
  }

  .rating--disabled .rating__symbols {
    cursor: not-allowed;
  }

  /* Forced colors mode */
  @media (forced-colors: active) {
    .rating__symbol--active {
      color: SelectedItem;
    }
  }
`;/**
 * @license
 * Copyright 2018 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Xl="important",af=" !"+Xl,Rt=Li(class extends Ri{constructor(e){var t;if(super(e),e.type!==ce.ATTRIBUTE||e.name!=="style"||((t=e.strings)==null?void 0:t.length)>2)throw Error("The `styleMap` directive must be used in the `style` attribute and must be the only part in the attribute.")}render(e){return Object.keys(e).reduce((t,o)=>{const i=e[o];return i==null?t:t+`${o=o.includes("-")?o:o.replace(/(?:^(webkit|moz|ms|o)|)(?=[A-Z])/g,"-$&").toLowerCase()}:${i};`},"")}update(e,[t]){const{style:o}=e.element;if(this.ft===void 0)return this.ft=new Set(Object.keys(t)),this.render(t);for(const i of this.ft)t[i]==null&&(this.ft.delete(i),i.includes("-")?o.removeProperty(i):o[i]=null);for(const i in t){const s=t[i];if(s!=null){this.ft.add(i);const r=typeof s=="string"&&s.endsWith(af);i.includes("-")||r?o.setProperty(i,r?s.slice(0,-11):s,r?Xl:""):o[i]=s}}return Bt}});var Et=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.hoverValue=0,this.isHovering=!1,this.label="",this.value=0,this.max=5,this.precision=1,this.readonly=!1,this.disabled=!1,this.getSymbol=()=>'<sl-icon name="star-fill" library="system"></sl-icon>'}getValueFromMousePosition(e){return this.getValueFromXCoordinate(e.clientX)}getValueFromTouchPosition(e){return this.getValueFromXCoordinate(e.touches[0].clientX)}getValueFromXCoordinate(e){const t=this.localize.dir()==="rtl",{left:o,right:i,width:s}=this.rating.getBoundingClientRect(),r=t?this.roundToPrecision((i-e)/s*this.max,this.precision):this.roundToPrecision((e-o)/s*this.max,this.precision);return ht(r,0,this.max)}handleClick(e){this.disabled||(this.setValue(this.getValueFromMousePosition(e)),this.emit("sl-change"))}setValue(e){this.disabled||this.readonly||(this.value=e===this.value?0:e,this.isHovering=!1)}handleKeyDown(e){const t=this.localize.dir()==="ltr",o=this.localize.dir()==="rtl",i=this.value;if(!(this.disabled||this.readonly)){if(e.key==="ArrowDown"||t&&e.key==="ArrowLeft"||o&&e.key==="ArrowRight"){const s=e.shiftKey?1:this.precision;this.value=Math.max(0,this.value-s),e.preventDefault()}if(e.key==="ArrowUp"||t&&e.key==="ArrowRight"||o&&e.key==="ArrowLeft"){const s=e.shiftKey?1:this.precision;this.value=Math.min(this.max,this.value+s),e.preventDefault()}e.key==="Home"&&(this.value=0,e.preventDefault()),e.key==="End"&&(this.value=this.max,e.preventDefault()),this.value!==i&&this.emit("sl-change")}}handleMouseEnter(e){this.isHovering=!0,this.hoverValue=this.getValueFromMousePosition(e)}handleMouseMove(e){this.hoverValue=this.getValueFromMousePosition(e)}handleMouseLeave(){this.isHovering=!1}handleTouchStart(e){this.isHovering=!0,this.hoverValue=this.getValueFromTouchPosition(e),e.preventDefault()}handleTouchMove(e){this.hoverValue=this.getValueFromTouchPosition(e)}handleTouchEnd(e){this.isHovering=!1,this.setValue(this.hoverValue),this.emit("sl-change"),e.preventDefault()}roundToPrecision(e,t=.5){const o=1/t;return Math.ceil(e*o)/o}handleHoverValueChange(){this.emit("sl-hover",{detail:{phase:"move",value:this.hoverValue}})}handleIsHoveringChange(){this.emit("sl-hover",{detail:{phase:this.isHovering?"start":"end",value:this.hoverValue}})}focus(e){this.rating.focus(e)}blur(){this.rating.blur()}render(){const e=this.localize.dir()==="rtl",t=Array.from(Array(this.max).keys());let o=0;return this.disabled||this.readonly?o=this.value:o=this.isHovering?this.hoverValue:this.value,x`
      <div
        part="base"
        class=${R({rating:!0,"rating--readonly":this.readonly,"rating--disabled":this.disabled,"rating--rtl":e})}
        role="slider"
        aria-label=${this.label}
        aria-disabled=${this.disabled?"true":"false"}
        aria-readonly=${this.readonly?"true":"false"}
        aria-valuenow=${this.value}
        aria-valuemin=${0}
        aria-valuemax=${this.max}
        tabindex=${this.disabled||this.readonly?"-1":"0"}
        @click=${this.handleClick}
        @keydown=${this.handleKeyDown}
        @mouseenter=${this.handleMouseEnter}
        @touchstart=${this.handleTouchStart}
        @mouseleave=${this.handleMouseLeave}
        @touchend=${this.handleTouchEnd}
        @mousemove=${this.handleMouseMove}
        @touchmove=${this.handleTouchMove}
      >
        <span class="rating__symbols">
          ${t.map(i=>o>i&&o<i+1?x`
                <span
                  class=${R({rating__symbol:!0,"rating__partial-symbol-container":!0,"rating__symbol--hover":this.isHovering&&Math.ceil(o)===i+1})}
                  role="presentation"
                >
                  <div
                    style=${Rt({clipPath:e?`inset(0 ${(o-i)*100}% 0 0)`:`inset(0 0 0 ${(o-i)*100}%)`})}
                  >
                    ${es(this.getSymbol(i+1))}
                  </div>
                  <div
                    class="rating__partial--filled"
                    style=${Rt({clipPath:e?`inset(0 0 0 ${100-(o-i)*100}%)`:`inset(0 ${100-(o-i)*100}% 0 0)`})}
                  >
                    ${es(this.getSymbol(i+1))}
                  </div>
                </span>
              `:x`
              <span
                class=${R({rating__symbol:!0,"rating__symbol--hover":this.isHovering&&Math.ceil(o)===i+1,"rating__symbol--active":o>=i+1})}
                role="presentation"
              >
                ${es(this.getSymbol(i+1))}
              </span>
            `)}
        </span>
      </div>
    `}};Et.styles=[D,rf];Et.dependencies={"sl-icon":J};l([S(".rating")],Et.prototype,"rating",2);l([z()],Et.prototype,"hoverValue",2);l([z()],Et.prototype,"isHovering",2);l([p()],Et.prototype,"label",2);l([p({type:Number})],Et.prototype,"value",2);l([p({type:Number})],Et.prototype,"max",2);l([p({type:Number})],Et.prototype,"precision",2);l([p({type:Boolean,reflect:!0})],Et.prototype,"readonly",2);l([p({type:Boolean,reflect:!0})],Et.prototype,"disabled",2);l([p()],Et.prototype,"getSymbol",2);l([Ai({passive:!0})],Et.prototype,"handleTouchMove",1);l([C("hoverValue")],Et.prototype,"handleHoverValueChange",1);l([C("isHovering")],Et.prototype,"handleIsHoveringChange",1);Et.define("sl-rating");var nf=[{max:276e4,value:6e4,unit:"minute"},{max:72e6,value:36e5,unit:"hour"},{max:5184e5,value:864e5,unit:"day"},{max:24192e5,value:6048e5,unit:"week"},{max:28512e6,value:2592e6,unit:"month"},{max:1/0,value:31536e6,unit:"year"}],fo=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.isoTime="",this.relativeTime="",this.date=new Date,this.format="long",this.numeric="auto",this.sync=!1}disconnectedCallback(){super.disconnectedCallback(),clearTimeout(this.updateTimeout)}render(){const e=new Date,t=new Date(this.date);if(isNaN(t.getMilliseconds()))return this.relativeTime="",this.isoTime="","";const o=t.getTime()-e.getTime(),{unit:i,value:s}=nf.find(r=>Math.abs(o)<r.max);if(this.isoTime=t.toISOString(),this.relativeTime=this.localize.relativeTime(Math.round(o/s),i,{numeric:this.numeric,style:this.format}),clearTimeout(this.updateTimeout),this.sync){let r;i==="minute"?r=Wi("second"):i==="hour"?r=Wi("minute"):i==="day"?r=Wi("hour"):r=Wi("day"),this.updateTimeout=window.setTimeout(()=>this.requestUpdate(),r)}return x` <time datetime=${this.isoTime}>${this.relativeTime}</time> `}};l([z()],fo.prototype,"isoTime",2);l([z()],fo.prototype,"relativeTime",2);l([p()],fo.prototype,"date",2);l([p()],fo.prototype,"format",2);l([p()],fo.prototype,"numeric",2);l([p({type:Boolean})],fo.prototype,"sync",2);function Wi(e){const o={second:1e3,minute:6e4,hour:36e5,day:864e5}[e];return o-Date.now()%o}fo.define("sl-relative-time");var Ql=O`
  :host {
    display: inline-block;
    position: relative;
    width: auto;
    cursor: pointer;
  }

  .button {
    display: inline-flex;
    align-items: stretch;
    justify-content: center;
    width: 100%;
    border-style: solid;
    border-width: var(--sl-input-border-width);
    font-family: var(--sl-input-font-family);
    font-weight: var(--sl-font-weight-semibold);
    text-decoration: none;
    user-select: none;
    -webkit-user-select: none;
    white-space: nowrap;
    vertical-align: middle;
    padding: 0;
    transition:
      var(--sl-transition-x-fast) background-color,
      var(--sl-transition-x-fast) color,
      var(--sl-transition-x-fast) border,
      var(--sl-transition-x-fast) box-shadow;
    cursor: inherit;
  }

  .button::-moz-focus-inner {
    border: 0;
  }

  .button:focus {
    outline: none;
  }

  .button:focus-visible {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  .button--disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  /* When disabled, prevent mouse events from bubbling up from children */
  .button--disabled * {
    pointer-events: none;
  }

  .button__prefix,
  .button__suffix {
    flex: 0 0 auto;
    display: flex;
    align-items: center;
    pointer-events: none;
  }

  .button__label {
    display: inline-block;
  }

  .button__label::slotted(sl-icon) {
    vertical-align: -2px;
  }

  /*
   * Standard buttons
   */

  /* Default */
  .button--standard.button--default {
    background-color: var(--sl-color-neutral-0);
    border-color: var(--sl-input-border-color);
    color: var(--sl-color-neutral-700);
  }

  .button--standard.button--default:hover:not(.button--disabled) {
    background-color: var(--sl-color-primary-50);
    border-color: var(--sl-color-primary-300);
    color: var(--sl-color-primary-700);
  }

  .button--standard.button--default:active:not(.button--disabled) {
    background-color: var(--sl-color-primary-100);
    border-color: var(--sl-color-primary-400);
    color: var(--sl-color-primary-700);
  }

  /* Primary */
  .button--standard.button--primary {
    background-color: var(--sl-color-primary-600);
    border-color: var(--sl-color-primary-600);
    color: var(--sl-color-neutral-0);
  }

  .button--standard.button--primary:hover:not(.button--disabled) {
    background-color: var(--sl-color-primary-500);
    border-color: var(--sl-color-primary-500);
    color: var(--sl-color-neutral-0);
  }

  .button--standard.button--primary:active:not(.button--disabled) {
    background-color: var(--sl-color-primary-600);
    border-color: var(--sl-color-primary-600);
    color: var(--sl-color-neutral-0);
  }

  /* Success */
  .button--standard.button--success {
    background-color: var(--sl-color-success-600);
    border-color: var(--sl-color-success-600);
    color: var(--sl-color-neutral-0);
  }

  .button--standard.button--success:hover:not(.button--disabled) {
    background-color: var(--sl-color-success-500);
    border-color: var(--sl-color-success-500);
    color: var(--sl-color-neutral-0);
  }

  .button--standard.button--success:active:not(.button--disabled) {
    background-color: var(--sl-color-success-600);
    border-color: var(--sl-color-success-600);
    color: var(--sl-color-neutral-0);
  }

  /* Neutral */
  .button--standard.button--neutral {
    background-color: var(--sl-color-neutral-600);
    border-color: var(--sl-color-neutral-600);
    color: var(--sl-color-neutral-0);
  }

  .button--standard.button--neutral:hover:not(.button--disabled) {
    background-color: var(--sl-color-neutral-500);
    border-color: var(--sl-color-neutral-500);
    color: var(--sl-color-neutral-0);
  }

  .button--standard.button--neutral:active:not(.button--disabled) {
    background-color: var(--sl-color-neutral-600);
    border-color: var(--sl-color-neutral-600);
    color: var(--sl-color-neutral-0);
  }

  /* Warning */
  .button--standard.button--warning {
    background-color: var(--sl-color-warning-600);
    border-color: var(--sl-color-warning-600);
    color: var(--sl-color-neutral-0);
  }
  .button--standard.button--warning:hover:not(.button--disabled) {
    background-color: var(--sl-color-warning-500);
    border-color: var(--sl-color-warning-500);
    color: var(--sl-color-neutral-0);
  }

  .button--standard.button--warning:active:not(.button--disabled) {
    background-color: var(--sl-color-warning-600);
    border-color: var(--sl-color-warning-600);
    color: var(--sl-color-neutral-0);
  }

  /* Danger */
  .button--standard.button--danger {
    background-color: var(--sl-color-danger-600);
    border-color: var(--sl-color-danger-600);
    color: var(--sl-color-neutral-0);
  }

  .button--standard.button--danger:hover:not(.button--disabled) {
    background-color: var(--sl-color-danger-500);
    border-color: var(--sl-color-danger-500);
    color: var(--sl-color-neutral-0);
  }

  .button--standard.button--danger:active:not(.button--disabled) {
    background-color: var(--sl-color-danger-600);
    border-color: var(--sl-color-danger-600);
    color: var(--sl-color-neutral-0);
  }

  /*
   * Outline buttons
   */

  .button--outline {
    background: none;
    border: solid 1px;
  }

  /* Default */
  .button--outline.button--default {
    border-color: var(--sl-input-border-color);
    color: var(--sl-color-neutral-700);
  }

  .button--outline.button--default:hover:not(.button--disabled),
  .button--outline.button--default.button--checked:not(.button--disabled) {
    border-color: var(--sl-color-primary-600);
    background-color: var(--sl-color-primary-600);
    color: var(--sl-color-neutral-0);
  }

  .button--outline.button--default:active:not(.button--disabled) {
    border-color: var(--sl-color-primary-700);
    background-color: var(--sl-color-primary-700);
    color: var(--sl-color-neutral-0);
  }

  /* Primary */
  .button--outline.button--primary {
    border-color: var(--sl-color-primary-600);
    color: var(--sl-color-primary-600);
  }

  .button--outline.button--primary:hover:not(.button--disabled),
  .button--outline.button--primary.button--checked:not(.button--disabled) {
    background-color: var(--sl-color-primary-600);
    color: var(--sl-color-neutral-0);
  }

  .button--outline.button--primary:active:not(.button--disabled) {
    border-color: var(--sl-color-primary-700);
    background-color: var(--sl-color-primary-700);
    color: var(--sl-color-neutral-0);
  }

  /* Success */
  .button--outline.button--success {
    border-color: var(--sl-color-success-600);
    color: var(--sl-color-success-600);
  }

  .button--outline.button--success:hover:not(.button--disabled),
  .button--outline.button--success.button--checked:not(.button--disabled) {
    background-color: var(--sl-color-success-600);
    color: var(--sl-color-neutral-0);
  }

  .button--outline.button--success:active:not(.button--disabled) {
    border-color: var(--sl-color-success-700);
    background-color: var(--sl-color-success-700);
    color: var(--sl-color-neutral-0);
  }

  /* Neutral */
  .button--outline.button--neutral {
    border-color: var(--sl-color-neutral-600);
    color: var(--sl-color-neutral-600);
  }

  .button--outline.button--neutral:hover:not(.button--disabled),
  .button--outline.button--neutral.button--checked:not(.button--disabled) {
    background-color: var(--sl-color-neutral-600);
    color: var(--sl-color-neutral-0);
  }

  .button--outline.button--neutral:active:not(.button--disabled) {
    border-color: var(--sl-color-neutral-700);
    background-color: var(--sl-color-neutral-700);
    color: var(--sl-color-neutral-0);
  }

  /* Warning */
  .button--outline.button--warning {
    border-color: var(--sl-color-warning-600);
    color: var(--sl-color-warning-600);
  }

  .button--outline.button--warning:hover:not(.button--disabled),
  .button--outline.button--warning.button--checked:not(.button--disabled) {
    background-color: var(--sl-color-warning-600);
    color: var(--sl-color-neutral-0);
  }

  .button--outline.button--warning:active:not(.button--disabled) {
    border-color: var(--sl-color-warning-700);
    background-color: var(--sl-color-warning-700);
    color: var(--sl-color-neutral-0);
  }

  /* Danger */
  .button--outline.button--danger {
    border-color: var(--sl-color-danger-600);
    color: var(--sl-color-danger-600);
  }

  .button--outline.button--danger:hover:not(.button--disabled),
  .button--outline.button--danger.button--checked:not(.button--disabled) {
    background-color: var(--sl-color-danger-600);
    color: var(--sl-color-neutral-0);
  }

  .button--outline.button--danger:active:not(.button--disabled) {
    border-color: var(--sl-color-danger-700);
    background-color: var(--sl-color-danger-700);
    color: var(--sl-color-neutral-0);
  }

  @media (forced-colors: active) {
    .button.button--outline.button--checked:not(.button--disabled) {
      outline: solid 2px transparent;
    }
  }

  /*
   * Text buttons
   */

  .button--text {
    background-color: transparent;
    border-color: transparent;
    color: var(--sl-color-primary-600);
  }

  .button--text:hover:not(.button--disabled) {
    background-color: transparent;
    border-color: transparent;
    color: var(--sl-color-primary-500);
  }

  .button--text:focus-visible:not(.button--disabled) {
    background-color: transparent;
    border-color: transparent;
    color: var(--sl-color-primary-500);
  }

  .button--text:active:not(.button--disabled) {
    background-color: transparent;
    border-color: transparent;
    color: var(--sl-color-primary-700);
  }

  /*
   * Size modifiers
   */

  .button--small {
    height: auto;
    min-height: var(--sl-input-height-small);
    font-size: var(--sl-button-font-size-small);
    line-height: calc(var(--sl-input-height-small) - var(--sl-input-border-width) * 2);
    border-radius: var(--sl-input-border-radius-small);
  }

  .button--medium {
    height: auto;
    min-height: var(--sl-input-height-medium);
    font-size: var(--sl-button-font-size-medium);
    line-height: calc(var(--sl-input-height-medium) - var(--sl-input-border-width) * 2);
    border-radius: var(--sl-input-border-radius-medium);
  }

  .button--large {
    height: auto;
    min-height: var(--sl-input-height-large);
    font-size: var(--sl-button-font-size-large);
    line-height: calc(var(--sl-input-height-large) - var(--sl-input-border-width) * 2);
    border-radius: var(--sl-input-border-radius-large);
  }

  /*
   * Pill modifier
   */

  .button--pill.button--small {
    border-radius: var(--sl-input-height-small);
  }

  .button--pill.button--medium {
    border-radius: var(--sl-input-height-medium);
  }

  .button--pill.button--large {
    border-radius: var(--sl-input-height-large);
  }

  /*
   * Circle modifier
   */

  .button--circle {
    padding-left: 0;
    padding-right: 0;
  }

  .button--circle.button--small {
    width: var(--sl-input-height-small);
    border-radius: 50%;
  }

  .button--circle.button--medium {
    width: var(--sl-input-height-medium);
    border-radius: 50%;
  }

  .button--circle.button--large {
    width: var(--sl-input-height-large);
    border-radius: 50%;
  }

  .button--circle .button__prefix,
  .button--circle .button__suffix,
  .button--circle .button__caret {
    display: none;
  }

  /*
   * Caret modifier
   */

  .button--caret .button__suffix {
    display: none;
  }

  .button--caret .button__caret {
    height: auto;
  }

  /*
   * Loading modifier
   */

  .button--loading {
    position: relative;
    cursor: wait;
  }

  .button--loading .button__prefix,
  .button--loading .button__label,
  .button--loading .button__suffix,
  .button--loading .button__caret {
    visibility: hidden;
  }

  .button--loading sl-spinner {
    --indicator-color: currentColor;
    position: absolute;
    font-size: 1em;
    height: 1em;
    width: 1em;
    top: calc(50% - 0.5em);
    left: calc(50% - 0.5em);
  }

  /*
   * Badges
   */

  .button ::slotted(sl-badge) {
    position: absolute;
    top: 0;
    right: 0;
    translate: 50% -50%;
    pointer-events: none;
  }

  .button--rtl ::slotted(sl-badge) {
    right: auto;
    left: 0;
    translate: -50% -50%;
  }

  /*
   * Button spacing
   */

  .button--has-label.button--small .button__label {
    padding: 0 var(--sl-spacing-small);
  }

  .button--has-label.button--medium .button__label {
    padding: 0 var(--sl-spacing-medium);
  }

  .button--has-label.button--large .button__label {
    padding: 0 var(--sl-spacing-large);
  }

  .button--has-prefix.button--small {
    padding-inline-start: var(--sl-spacing-x-small);
  }

  .button--has-prefix.button--small .button__label {
    padding-inline-start: var(--sl-spacing-x-small);
  }

  .button--has-prefix.button--medium {
    padding-inline-start: var(--sl-spacing-small);
  }

  .button--has-prefix.button--medium .button__label {
    padding-inline-start: var(--sl-spacing-small);
  }

  .button--has-prefix.button--large {
    padding-inline-start: var(--sl-spacing-small);
  }

  .button--has-prefix.button--large .button__label {
    padding-inline-start: var(--sl-spacing-small);
  }

  .button--has-suffix.button--small,
  .button--caret.button--small {
    padding-inline-end: var(--sl-spacing-x-small);
  }

  .button--has-suffix.button--small .button__label,
  .button--caret.button--small .button__label {
    padding-inline-end: var(--sl-spacing-x-small);
  }

  .button--has-suffix.button--medium,
  .button--caret.button--medium {
    padding-inline-end: var(--sl-spacing-small);
  }

  .button--has-suffix.button--medium .button__label,
  .button--caret.button--medium .button__label {
    padding-inline-end: var(--sl-spacing-small);
  }

  .button--has-suffix.button--large,
  .button--caret.button--large {
    padding-inline-end: var(--sl-spacing-small);
  }

  .button--has-suffix.button--large .button__label,
  .button--caret.button--large .button__label {
    padding-inline-end: var(--sl-spacing-small);
  }

  /*
   * Button groups support a variety of button types (e.g. buttons with tooltips, buttons as dropdown triggers, etc.).
   * This means buttons aren't always direct descendants of the button group, thus we can't target them with the
   * ::slotted selector. To work around this, the button group component does some magic to add these special classes to
   * buttons and we style them here instead.
   */

  :host([data-sl-button-group__button--first]:not([data-sl-button-group__button--last])) .button {
    border-start-end-radius: 0;
    border-end-end-radius: 0;
  }

  :host([data-sl-button-group__button--inner]) .button {
    border-radius: 0;
  }

  :host([data-sl-button-group__button--last]:not([data-sl-button-group__button--first])) .button {
    border-start-start-radius: 0;
    border-end-start-radius: 0;
  }

  /* All except the first */
  :host([data-sl-button-group__button]:not([data-sl-button-group__button--first])) {
    margin-inline-start: calc(-1 * var(--sl-input-border-width));
  }

  /* Add a visual separator between solid buttons */
  :host(
      [data-sl-button-group__button]:not(
          [data-sl-button-group__button--first],
          [data-sl-button-group__button--radio],
          [variant='default']
        ):not(:hover)
    )
    .button:after {
    content: '';
    position: absolute;
    top: 0;
    inset-inline-start: 0;
    bottom: 0;
    border-left: solid 1px rgb(128 128 128 / 33%);
    mix-blend-mode: multiply;
  }

  /* Bump hovered, focused, and checked buttons up so their focus ring isn't clipped */
  :host([data-sl-button-group__button--hover]) {
    z-index: 1;
  }

  /* Focus and checked are always on top */
  :host([data-sl-button-group__button--focus]),
  :host([data-sl-button-group__button][checked]) {
    z-index: 2;
  }
`,lf=O`
  ${Ql}

  .button__prefix,
  .button__suffix,
  .button__label {
    display: inline-flex;
    position: relative;
    align-items: center;
  }

  /* We use a hidden input so constraint validation errors work, since they don't appear to show when used with buttons.
    We can't actually hide it, though, otherwise the messages will be suppressed by the browser. */
  .hidden-input {
    all: unset;
    position: absolute;
    top: 0;
    left: 0;
    bottom: 0;
    right: 0;
    outline: dotted 1px red;
    opacity: 0;
    z-index: -1;
  }
`,oe=class extends T{constructor(){super(...arguments),this.hasSlotController=new Ot(this,"[default]","prefix","suffix"),this.hasFocus=!1,this.checked=!1,this.disabled=!1,this.size="medium",this.pill=!1}connectedCallback(){super.connectedCallback(),this.setAttribute("role","presentation")}handleBlur(){this.hasFocus=!1,this.emit("sl-blur")}handleClick(e){if(this.disabled){e.preventDefault(),e.stopPropagation();return}this.checked=!0}handleFocus(){this.hasFocus=!0,this.emit("sl-focus")}handleDisabledChange(){this.setAttribute("aria-disabled",this.disabled?"true":"false")}focus(e){this.input.focus(e)}blur(){this.input.blur()}render(){return ci`
      <div part="base" role="presentation">
        <button
          part="${`button${this.checked?" button--checked":""}`}"
          role="radio"
          aria-checked="${this.checked}"
          class=${R({button:!0,"button--default":!0,"button--small":this.size==="small","button--medium":this.size==="medium","button--large":this.size==="large","button--checked":this.checked,"button--disabled":this.disabled,"button--focused":this.hasFocus,"button--outline":!0,"button--pill":this.pill,"button--has-label":this.hasSlotController.test("[default]"),"button--has-prefix":this.hasSlotController.test("prefix"),"button--has-suffix":this.hasSlotController.test("suffix")})}
          aria-disabled=${this.disabled}
          type="button"
          value=${E(this.value)}
          @blur=${this.handleBlur}
          @focus=${this.handleFocus}
          @click=${this.handleClick}
        >
          <slot name="prefix" part="prefix" class="button__prefix"></slot>
          <slot part="label" class="button__label"></slot>
          <slot name="suffix" part="suffix" class="button__suffix"></slot>
        </button>
      </div>
    `}};oe.styles=[D,lf];l([S(".button")],oe.prototype,"input",2);l([S(".hidden-input")],oe.prototype,"hiddenInput",2);l([z()],oe.prototype,"hasFocus",2);l([p({type:Boolean,reflect:!0})],oe.prototype,"checked",2);l([p()],oe.prototype,"value",2);l([p({type:Boolean,reflect:!0})],oe.prototype,"disabled",2);l([p({reflect:!0})],oe.prototype,"size",2);l([p({type:Boolean,reflect:!0})],oe.prototype,"pill",2);l([C("disabled",{waitUntilFirstUpdate:!0})],oe.prototype,"handleDisabledChange",1);oe.define("sl-radio-button");var cf=O`
  :host {
    display: block;
  }

  .form-control {
    position: relative;
    border: none;
    padding: 0;
    margin: 0;
  }

  .form-control__label {
    padding: 0;
  }

  .radio-group--required .radio-group__label::after {
    content: var(--sl-input-required-content);
    margin-inline-start: var(--sl-input-required-content-offset);
  }

  .visually-hidden {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
    white-space: nowrap;
    border: 0;
  }
`,df=O`
  :host {
    display: inline-block;
  }

  .button-group {
    display: flex;
    flex-wrap: nowrap;
  }
`,mo=class extends T{constructor(){super(...arguments),this.disableRole=!1,this.label=""}handleFocus(e){const t=Jo(e.target);t==null||t.toggleAttribute("data-sl-button-group__button--focus",!0)}handleBlur(e){const t=Jo(e.target);t==null||t.toggleAttribute("data-sl-button-group__button--focus",!1)}handleMouseOver(e){const t=Jo(e.target);t==null||t.toggleAttribute("data-sl-button-group__button--hover",!0)}handleMouseOut(e){const t=Jo(e.target);t==null||t.toggleAttribute("data-sl-button-group__button--hover",!1)}handleSlotChange(){const e=[...this.defaultSlot.assignedElements({flatten:!0})];e.forEach(t=>{const o=e.indexOf(t),i=Jo(t);i&&(i.toggleAttribute("data-sl-button-group__button",!0),i.toggleAttribute("data-sl-button-group__button--first",o===0),i.toggleAttribute("data-sl-button-group__button--inner",o>0&&o<e.length-1),i.toggleAttribute("data-sl-button-group__button--last",o===e.length-1),i.toggleAttribute("data-sl-button-group__button--radio",i.tagName.toLowerCase()==="sl-radio-button"))})}render(){return x`
      <div
        part="base"
        class="button-group"
        role="${this.disableRole?"presentation":"group"}"
        aria-label=${this.label}
        @focusout=${this.handleBlur}
        @focusin=${this.handleFocus}
        @mouseover=${this.handleMouseOver}
        @mouseout=${this.handleMouseOut}
      >
        <slot @slotchange=${this.handleSlotChange}></slot>
      </div>
    `}};mo.styles=[D,df];l([S("slot")],mo.prototype,"defaultSlot",2);l([z()],mo.prototype,"disableRole",2);l([p()],mo.prototype,"label",2);function Jo(e){var t;const o="sl-button, sl-radio-button";return(t=e.closest(o))!=null?t:e.querySelector(o)}var Ct=class extends T{constructor(){super(...arguments),this.formControlController=new Ee(this),this.hasSlotController=new Ot(this,"help-text","label"),this.customValidityMessage="",this.hasButtonGroup=!1,this.errorMessage="",this.defaultValue="",this.label="",this.helpText="",this.name="option",this.value="",this.size="medium",this.form="",this.required=!1}get validity(){const e=this.required&&!this.value;return this.customValidityMessage!==""?Eu:e?zu:Os}get validationMessage(){const e=this.required&&!this.value;return this.customValidityMessage!==""?this.customValidityMessage:e?this.validationInput.validationMessage:""}connectedCallback(){super.connectedCallback(),this.defaultValue=this.value}firstUpdated(){this.formControlController.updateValidity()}getAllRadios(){return[...this.querySelectorAll("sl-radio, sl-radio-button")]}handleRadioClick(e){const t=e.target.closest("sl-radio, sl-radio-button"),o=this.getAllRadios(),i=this.value;!t||t.disabled||(this.value=t.value,o.forEach(s=>s.checked=s===t),this.value!==i&&(this.emit("sl-change"),this.emit("sl-input")))}handleKeyDown(e){var t;if(!["ArrowUp","ArrowDown","ArrowLeft","ArrowRight"," "].includes(e.key))return;const o=this.getAllRadios().filter(n=>!n.disabled),i=(t=o.find(n=>n.checked))!=null?t:o[0],s=e.key===" "?0:["ArrowUp","ArrowLeft"].includes(e.key)?-1:1,r=this.value;let a=o.indexOf(i)+s;a<0&&(a=o.length-1),a>o.length-1&&(a=0),this.getAllRadios().forEach(n=>{n.checked=!1,this.hasButtonGroup||n.setAttribute("tabindex","-1")}),this.value=o[a].value,o[a].checked=!0,this.hasButtonGroup?o[a].shadowRoot.querySelector("button").focus():(o[a].setAttribute("tabindex","0"),o[a].focus()),this.value!==r&&(this.emit("sl-change"),this.emit("sl-input")),e.preventDefault()}handleLabelClick(){this.focus()}handleInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}async syncRadioElements(){var e,t;const o=this.getAllRadios();if(await Promise.all(o.map(async i=>{await i.updateComplete,i.checked=i.value===this.value,i.size=this.size})),this.hasButtonGroup=o.some(i=>i.tagName.toLowerCase()==="sl-radio-button"),o.length>0&&!o.some(i=>i.checked))if(this.hasButtonGroup){const i=(e=o[0].shadowRoot)==null?void 0:e.querySelector("button");i&&i.setAttribute("tabindex","0")}else o[0].setAttribute("tabindex","0");if(this.hasButtonGroup){const i=(t=this.shadowRoot)==null?void 0:t.querySelector("sl-button-group");i&&(i.disableRole=!0)}}syncRadios(){if(customElements.get("sl-radio")&&customElements.get("sl-radio-button")){this.syncRadioElements();return}customElements.get("sl-radio")?this.syncRadioElements():customElements.whenDefined("sl-radio").then(()=>this.syncRadios()),customElements.get("sl-radio-button")?this.syncRadioElements():customElements.whenDefined("sl-radio-button").then(()=>this.syncRadios())}updateCheckedRadio(){this.getAllRadios().forEach(t=>t.checked=t.value===this.value),this.formControlController.setValidity(this.validity.valid)}handleSizeChange(){this.syncRadios()}handleValueChange(){this.hasUpdated&&this.updateCheckedRadio()}checkValidity(){const e=this.required&&!this.value,t=this.customValidityMessage!=="";return e||t?(this.formControlController.emitInvalidEvent(),!1):!0}getForm(){return this.formControlController.getForm()}reportValidity(){const e=this.validity.valid;return this.errorMessage=this.customValidityMessage||e?"":this.validationInput.validationMessage,this.formControlController.setValidity(e),this.validationInput.hidden=!0,clearTimeout(this.validationTimeout),e||(this.validationInput.hidden=!1,this.validationInput.reportValidity(),this.validationTimeout=setTimeout(()=>this.validationInput.hidden=!0,1e4)),e}setCustomValidity(e=""){this.customValidityMessage=e,this.errorMessage=e,this.validationInput.setCustomValidity(e),this.formControlController.updateValidity()}focus(e){const t=this.getAllRadios(),o=t.find(r=>r.checked),i=t.find(r=>!r.disabled),s=o||i;s&&s.focus(e)}render(){const e=this.hasSlotController.test("label"),t=this.hasSlotController.test("help-text"),o=this.label?!0:!!e,i=this.helpText?!0:!!t,s=x`
      <slot @slotchange=${this.syncRadios} @click=${this.handleRadioClick} @keydown=${this.handleKeyDown}></slot>
    `;return x`
      <fieldset
        part="form-control"
        class=${R({"form-control":!0,"form-control--small":this.size==="small","form-control--medium":this.size==="medium","form-control--large":this.size==="large","form-control--radio-group":!0,"form-control--has-label":o,"form-control--has-help-text":i})}
        role="radiogroup"
        aria-labelledby="label"
        aria-describedby="help-text"
        aria-errormessage="error-message"
      >
        <label
          part="form-control-label"
          id="label"
          class="form-control__label"
          aria-hidden=${o?"false":"true"}
          @click=${this.handleLabelClick}
        >
          <slot name="label">${this.label}</slot>
        </label>

        <div part="form-control-input" class="form-control-input">
          <div class="visually-hidden">
            <div id="error-message" aria-live="assertive">${this.errorMessage}</div>
            <label class="radio-group__validation">
              <input
                type="text"
                class="radio-group__validation-input"
                ?required=${this.required}
                tabindex="-1"
                hidden
                @invalid=${this.handleInvalid}
              />
            </label>
          </div>

          ${this.hasButtonGroup?x`
                <sl-button-group part="button-group" exportparts="base:button-group__base" role="presentation">
                  ${s}
                </sl-button-group>
              `:s}
        </div>

        <div
          part="form-control-help-text"
          id="help-text"
          class="form-control__help-text"
          aria-hidden=${i?"false":"true"}
        >
          <slot name="help-text">${this.helpText}</slot>
        </div>
      </fieldset>
    `}};Ct.styles=[D,uo,cf];Ct.dependencies={"sl-button-group":mo};l([S("slot:not([name])")],Ct.prototype,"defaultSlot",2);l([S(".radio-group__validation-input")],Ct.prototype,"validationInput",2);l([z()],Ct.prototype,"hasButtonGroup",2);l([z()],Ct.prototype,"errorMessage",2);l([z()],Ct.prototype,"defaultValue",2);l([p()],Ct.prototype,"label",2);l([p({attribute:"help-text"})],Ct.prototype,"helpText",2);l([p()],Ct.prototype,"name",2);l([p({reflect:!0})],Ct.prototype,"value",2);l([p({reflect:!0})],Ct.prototype,"size",2);l([p({reflect:!0})],Ct.prototype,"form",2);l([p({type:Boolean,reflect:!0})],Ct.prototype,"required",2);l([C("size",{waitUntilFirstUpdate:!0})],Ct.prototype,"handleSizeChange",1);l([C("value")],Ct.prototype,"handleValueChange",1);Ct.define("sl-radio-group");var hf=O`
  :host {
    --size: 128px;
    --track-width: 4px;
    --track-color: var(--sl-color-neutral-200);
    --indicator-width: var(--track-width);
    --indicator-color: var(--sl-color-primary-600);
    --indicator-transition-duration: 0.35s;

    display: inline-flex;
  }

  .progress-ring {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    position: relative;
  }

  .progress-ring__image {
    width: var(--size);
    height: var(--size);
    rotate: -90deg;
    transform-origin: 50% 50%;
  }

  .progress-ring__track,
  .progress-ring__indicator {
    --radius: calc(var(--size) / 2 - max(var(--track-width), var(--indicator-width)) * 0.5);
    --circumference: calc(var(--radius) * 2 * 3.141592654);

    fill: none;
    r: var(--radius);
    cx: calc(var(--size) / 2);
    cy: calc(var(--size) / 2);
  }

  .progress-ring__track {
    stroke: var(--track-color);
    stroke-width: var(--track-width);
  }

  .progress-ring__indicator {
    stroke: var(--indicator-color);
    stroke-width: var(--indicator-width);
    stroke-linecap: round;
    transition-property: stroke-dashoffset;
    transition-duration: var(--indicator-transition-duration);
    stroke-dasharray: var(--circumference) var(--circumference);
    stroke-dashoffset: calc(var(--circumference) - var(--percentage) * var(--circumference));
  }

  .progress-ring__label {
    display: flex;
    align-items: center;
    justify-content: center;
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    text-align: center;
    user-select: none;
    -webkit-user-select: none;
  }
`,Uo=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.value=0,this.label=""}updated(e){if(super.updated(e),e.has("value")){const t=parseFloat(getComputedStyle(this.indicator).getPropertyValue("r")),o=2*Math.PI*t,i=o-this.value/100*o;this.indicatorOffset=`${i}px`}}render(){return x`
      <div
        part="base"
        class="progress-ring"
        role="progressbar"
        aria-label=${this.label.length>0?this.label:this.localize.term("progress")}
        aria-describedby="label"
        aria-valuemin="0"
        aria-valuemax="100"
        aria-valuenow="${this.value}"
        style="--percentage: ${this.value/100}"
      >
        <svg class="progress-ring__image">
          <circle class="progress-ring__track"></circle>
          <circle class="progress-ring__indicator" style="stroke-dashoffset: ${this.indicatorOffset}"></circle>
        </svg>

        <slot id="label" part="label" class="progress-ring__label"></slot>
      </div>
    `}};Uo.styles=[D,hf];l([S(".progress-ring__indicator")],Uo.prototype,"indicator",2);l([z()],Uo.prototype,"indicatorOffset",2);l([p({type:Number,reflect:!0})],Uo.prototype,"value",2);l([p()],Uo.prototype,"label",2);Uo.define("sl-progress-ring");var uf=O`
  :host {
    display: inline-block;
  }
`;let Zl=null;class Gl{}Gl.render=function(e,t){Zl(e,t)};self.QrCreator=Gl;(function(e){function t(n,c,d,u){var h={},f=e(d,c);f.u(n),f.J(),u=u||0;var m=f.h(),g=f.h()+2*u;return h.text=n,h.level=c,h.version=d,h.O=g,h.a=function(b,k){return b-=u,k-=u,0>b||b>=m||0>k||k>=m?!1:f.a(b,k)},h}function o(n,c,d,u,h,f,m,g,b,k){function $(w,_,v,y,P,M,I){w?(n.lineTo(_+M,v+I),n.arcTo(_,v,y,P,f)):n.lineTo(_,v)}m?n.moveTo(c+f,d):n.moveTo(c,d),$(g,u,d,u,h,-f,0),$(b,u,h,c,h,0,-f),$(k,c,h,c,d,f,0),$(m,c,d,u,d,0,f)}function i(n,c,d,u,h,f,m,g,b,k){function $(w,_,v,y){n.moveTo(w+v,_),n.lineTo(w,_),n.lineTo(w,_+y),n.arcTo(w,_,w+v,_,f)}m&&$(c,d,f,f),g&&$(u,d,-f,f),b&&$(u,h,-f,-f),k&&$(c,h,f,-f)}function s(n,c){var d=c.fill;if(typeof d=="string")n.fillStyle=d;else{var u=d.type,h=d.colorStops;if(d=d.position.map(m=>Math.round(m*c.size)),u==="linear-gradient")var f=n.createLinearGradient.apply(n,d);else if(u==="radial-gradient")f=n.createRadialGradient.apply(n,d);else throw Error("Unsupported fill");h.forEach(([m,g])=>{f.addColorStop(m,g)}),n.fillStyle=f}}function r(n,c){t:{var d=c.text,u=c.v,h=c.N,f=c.K,m=c.P;for(h=Math.max(1,h||1),f=Math.min(40,f||40);h<=f;h+=1)try{var g=t(d,u,h,m);break t}catch{}g=void 0}if(!g)return null;for(d=n.getContext("2d"),c.background&&(d.fillStyle=c.background,d.fillRect(c.left,c.top,c.size,c.size)),u=g.O,f=c.size/u,d.beginPath(),m=0;m<u;m+=1)for(h=0;h<u;h+=1){var b=d,k=c.left+h*f,$=c.top+m*f,w=m,_=h,v=g.a,y=k+f,P=$+f,M=w-1,I=w+1,L=_-1,A=_+1,Z=Math.floor(Math.min(.5,Math.max(0,c.R))*f),tt=v(w,_),dt=v(M,L),it=v(M,_);M=v(M,A);var pt=v(w,A);A=v(I,A),_=v(I,_),I=v(I,L),w=v(w,L),k=Math.round(k),$=Math.round($),y=Math.round(y),P=Math.round(P),tt?o(b,k,$,y,P,Z,!it&&!w,!it&&!pt,!_&&!pt,!_&&!w):i(b,k,$,y,P,Z,it&&w&&dt,it&&pt&&M,_&&pt&&A,_&&w&&I)}return s(d,c),d.fill(),n}var a={minVersion:1,maxVersion:40,ecLevel:"L",left:0,top:0,size:200,fill:"#000",background:null,text:"no text",radius:.5,quiet:0};Zl=function(n,c){var d={};Object.assign(d,a,n),d.N=d.minVersion,d.K=d.maxVersion,d.v=d.ecLevel,d.left=d.left,d.top=d.top,d.size=d.size,d.fill=d.fill,d.background=d.background,d.text=d.text,d.R=d.radius,d.P=d.quiet,c instanceof HTMLCanvasElement?((c.width!==d.size||c.height!==d.size)&&(c.width=d.size,c.height=d.size),c.getContext("2d").clearRect(0,0,c.width,c.height),r(c,d)):(n=document.createElement("canvas"),n.width=d.size,n.height=d.size,d=r(n,d),c.appendChild(d))}})(function(){function e(c){var d=o.s(c);return{S:function(){return 4},b:function(){return d.length},write:function(u){for(var h=0;h<d.length;h+=1)u.put(d[h],8)}}}function t(){var c=[],d=0,u={B:function(){return c},c:function(h){return(c[Math.floor(h/8)]>>>7-h%8&1)==1},put:function(h,f){for(var m=0;m<f;m+=1)u.m((h>>>f-m-1&1)==1)},f:function(){return d},m:function(h){var f=Math.floor(d/8);c.length<=f&&c.push(0),h&&(c[f]|=128>>>d%8),d+=1}};return u}function o(c,d){function u(w,_){for(var v=-1;7>=v;v+=1)if(!(-1>=w+v||g<=w+v))for(var y=-1;7>=y;y+=1)-1>=_+y||g<=_+y||(m[w+v][_+y]=0<=v&&6>=v&&(y==0||y==6)||0<=y&&6>=y&&(v==0||v==6)||2<=v&&4>=v&&2<=y&&4>=y)}function h(w,_){for(var v=g=4*c+17,y=Array(v),P=0;P<v;P+=1){y[P]=Array(v);for(var M=0;M<v;M+=1)y[P][M]=null}for(m=y,u(0,0),u(g-7,0),u(0,g-7),v=r.G(c),y=0;y<v.length;y+=1)for(P=0;P<v.length;P+=1){M=v[y];var I=v[P];if(m[M][I]==null)for(var L=-2;2>=L;L+=1)for(var A=-2;2>=A;A+=1)m[M+L][I+A]=L==-2||L==2||A==-2||A==2||L==0&&A==0}for(v=8;v<g-8;v+=1)m[v][6]==null&&(m[v][6]=v%2==0);for(v=8;v<g-8;v+=1)m[6][v]==null&&(m[6][v]=v%2==0);for(v=r.w(f<<3|_),y=0;15>y;y+=1)P=!w&&(v>>y&1)==1,m[6>y?y:8>y?y+1:g-15+y][8]=P,m[8][8>y?g-y-1:9>y?15-y:14-y]=P;if(m[g-8][8]=!w,7<=c){for(v=r.A(c),y=0;18>y;y+=1)P=!w&&(v>>y&1)==1,m[Math.floor(y/3)][y%3+g-8-3]=P;for(y=0;18>y;y+=1)P=!w&&(v>>y&1)==1,m[y%3+g-8-3][Math.floor(y/3)]=P}if(b==null){for(w=n.I(c,f),v=t(),y=0;y<k.length;y+=1)P=k[y],v.put(4,4),v.put(P.b(),r.f(4,c)),P.write(v);for(y=P=0;y<w.length;y+=1)P+=w[y].j;if(v.f()>8*P)throw Error("code length overflow. ("+v.f()+">"+8*P+")");for(v.f()+4<=8*P&&v.put(0,4);v.f()%8!=0;)v.m(!1);for(;!(v.f()>=8*P)&&(v.put(236,8),!(v.f()>=8*P));)v.put(17,8);var Z=0;for(P=y=0,M=Array(w.length),I=Array(w.length),L=0;L<w.length;L+=1){var tt=w[L].j,dt=w[L].o-tt;for(y=Math.max(y,tt),P=Math.max(P,dt),M[L]=Array(tt),A=0;A<M[L].length;A+=1)M[L][A]=255&v.B()[A+Z];for(Z+=tt,A=r.C(dt),tt=i(M[L],A.b()-1).l(A),I[L]=Array(A.b()-1),A=0;A<I[L].length;A+=1)dt=A+tt.b()-I[L].length,I[L][A]=0<=dt?tt.c(dt):0}for(A=v=0;A<w.length;A+=1)v+=w[A].o;for(v=Array(v),A=Z=0;A<y;A+=1)for(L=0;L<w.length;L+=1)A<M[L].length&&(v[Z]=M[L][A],Z+=1);for(A=0;A<P;A+=1)for(L=0;L<w.length;L+=1)A<I[L].length&&(v[Z]=I[L][A],Z+=1);b=v}for(w=b,v=-1,y=g-1,P=7,M=0,_=r.F(_),I=g-1;0<I;I-=2)for(I==6&&--I;;){for(L=0;2>L;L+=1)m[y][I-L]==null&&(A=!1,M<w.length&&(A=(w[M]>>>P&1)==1),_(y,I-L)&&(A=!A),m[y][I-L]=A,--P,P==-1&&(M+=1,P=7));if(y+=v,0>y||g<=y){y-=v,v=-v;break}}}var f=s[d],m=null,g=0,b=null,k=[],$={u:function(w){w=e(w),k.push(w),b=null},a:function(w,_){if(0>w||g<=w||0>_||g<=_)throw Error(w+","+_);return m[w][_]},h:function(){return g},J:function(){for(var w=0,_=0,v=0;8>v;v+=1){h(!0,v);var y=r.D($);(v==0||w>y)&&(w=y,_=v)}h(!1,_)}};return $}function i(c,d){if(typeof c.length>"u")throw Error(c.length+"/"+d);var u=function(){for(var f=0;f<c.length&&c[f]==0;)f+=1;for(var m=Array(c.length-f+d),g=0;g<c.length-f;g+=1)m[g]=c[g+f];return m}(),h={c:function(f){return u[f]},b:function(){return u.length},multiply:function(f){for(var m=Array(h.b()+f.b()-1),g=0;g<h.b();g+=1)for(var b=0;b<f.b();b+=1)m[g+b]^=a.i(a.g(h.c(g))+a.g(f.c(b)));return i(m,0)},l:function(f){if(0>h.b()-f.b())return h;for(var m=a.g(h.c(0))-a.g(f.c(0)),g=Array(h.b()),b=0;b<h.b();b+=1)g[b]=h.c(b);for(b=0;b<f.b();b+=1)g[b]^=a.i(a.g(f.c(b))+m);return i(g,0).l(f)}};return h}o.s=function(c){for(var d=[],u=0;u<c.length;u++){var h=c.charCodeAt(u);128>h?d.push(h):2048>h?d.push(192|h>>6,128|h&63):55296>h||57344<=h?d.push(224|h>>12,128|h>>6&63,128|h&63):(u++,h=65536+((h&1023)<<10|c.charCodeAt(u)&1023),d.push(240|h>>18,128|h>>12&63,128|h>>6&63,128|h&63))}return d};var s={L:1,M:0,Q:3,H:2},r=function(){function c(h){for(var f=0;h!=0;)f+=1,h>>>=1;return f}var d=[[],[6,18],[6,22],[6,26],[6,30],[6,34],[6,22,38],[6,24,42],[6,26,46],[6,28,50],[6,30,54],[6,32,58],[6,34,62],[6,26,46,66],[6,26,48,70],[6,26,50,74],[6,30,54,78],[6,30,56,82],[6,30,58,86],[6,34,62,90],[6,28,50,72,94],[6,26,50,74,98],[6,30,54,78,102],[6,28,54,80,106],[6,32,58,84,110],[6,30,58,86,114],[6,34,62,90,118],[6,26,50,74,98,122],[6,30,54,78,102,126],[6,26,52,78,104,130],[6,30,56,82,108,134],[6,34,60,86,112,138],[6,30,58,86,114,142],[6,34,62,90,118,146],[6,30,54,78,102,126,150],[6,24,50,76,102,128,154],[6,28,54,80,106,132,158],[6,32,58,84,110,136,162],[6,26,54,82,110,138,166],[6,30,58,86,114,142,170]],u={w:function(h){for(var f=h<<10;0<=c(f)-c(1335);)f^=1335<<c(f)-c(1335);return(h<<10|f)^21522},A:function(h){for(var f=h<<12;0<=c(f)-c(7973);)f^=7973<<c(f)-c(7973);return h<<12|f},G:function(h){return d[h-1]},F:function(h){switch(h){case 0:return function(f,m){return(f+m)%2==0};case 1:return function(f){return f%2==0};case 2:return function(f,m){return m%3==0};case 3:return function(f,m){return(f+m)%3==0};case 4:return function(f,m){return(Math.floor(f/2)+Math.floor(m/3))%2==0};case 5:return function(f,m){return f*m%2+f*m%3==0};case 6:return function(f,m){return(f*m%2+f*m%3)%2==0};case 7:return function(f,m){return(f*m%3+(f+m)%2)%2==0};default:throw Error("bad maskPattern:"+h)}},C:function(h){for(var f=i([1],0),m=0;m<h;m+=1)f=f.multiply(i([1,a.i(m)],0));return f},f:function(h,f){if(h!=4||1>f||40<f)throw Error("mode: "+h+"; type: "+f);return 10>f?8:16},D:function(h){for(var f=h.h(),m=0,g=0;g<f;g+=1)for(var b=0;b<f;b+=1){for(var k=0,$=h.a(g,b),w=-1;1>=w;w+=1)if(!(0>g+w||f<=g+w))for(var _=-1;1>=_;_+=1)0>b+_||f<=b+_||(w!=0||_!=0)&&$==h.a(g+w,b+_)&&(k+=1);5<k&&(m+=3+k-5)}for(g=0;g<f-1;g+=1)for(b=0;b<f-1;b+=1)k=0,h.a(g,b)&&(k+=1),h.a(g+1,b)&&(k+=1),h.a(g,b+1)&&(k+=1),h.a(g+1,b+1)&&(k+=1),(k==0||k==4)&&(m+=3);for(g=0;g<f;g+=1)for(b=0;b<f-6;b+=1)h.a(g,b)&&!h.a(g,b+1)&&h.a(g,b+2)&&h.a(g,b+3)&&h.a(g,b+4)&&!h.a(g,b+5)&&h.a(g,b+6)&&(m+=40);for(b=0;b<f;b+=1)for(g=0;g<f-6;g+=1)h.a(g,b)&&!h.a(g+1,b)&&h.a(g+2,b)&&h.a(g+3,b)&&h.a(g+4,b)&&!h.a(g+5,b)&&h.a(g+6,b)&&(m+=40);for(b=k=0;b<f;b+=1)for(g=0;g<f;g+=1)h.a(g,b)&&(k+=1);return m+=Math.abs(100*k/f/f-50)/5*10}};return u}(),a=function(){for(var c=Array(256),d=Array(256),u=0;8>u;u+=1)c[u]=1<<u;for(u=8;256>u;u+=1)c[u]=c[u-4]^c[u-5]^c[u-6]^c[u-8];for(u=0;255>u;u+=1)d[c[u]]=u;return{g:function(h){if(1>h)throw Error("glog("+h+")");return d[h]},i:function(h){for(;0>h;)h+=255;for(;256<=h;)h-=255;return c[h]}}}(),n=function(){function c(h,f){switch(f){case s.L:return d[4*(h-1)];case s.M:return d[4*(h-1)+1];case s.Q:return d[4*(h-1)+2];case s.H:return d[4*(h-1)+3]}}var d=[[1,26,19],[1,26,16],[1,26,13],[1,26,9],[1,44,34],[1,44,28],[1,44,22],[1,44,16],[1,70,55],[1,70,44],[2,35,17],[2,35,13],[1,100,80],[2,50,32],[2,50,24],[4,25,9],[1,134,108],[2,67,43],[2,33,15,2,34,16],[2,33,11,2,34,12],[2,86,68],[4,43,27],[4,43,19],[4,43,15],[2,98,78],[4,49,31],[2,32,14,4,33,15],[4,39,13,1,40,14],[2,121,97],[2,60,38,2,61,39],[4,40,18,2,41,19],[4,40,14,2,41,15],[2,146,116],[3,58,36,2,59,37],[4,36,16,4,37,17],[4,36,12,4,37,13],[2,86,68,2,87,69],[4,69,43,1,70,44],[6,43,19,2,44,20],[6,43,15,2,44,16],[4,101,81],[1,80,50,4,81,51],[4,50,22,4,51,23],[3,36,12,8,37,13],[2,116,92,2,117,93],[6,58,36,2,59,37],[4,46,20,6,47,21],[7,42,14,4,43,15],[4,133,107],[8,59,37,1,60,38],[8,44,20,4,45,21],[12,33,11,4,34,12],[3,145,115,1,146,116],[4,64,40,5,65,41],[11,36,16,5,37,17],[11,36,12,5,37,13],[5,109,87,1,110,88],[5,65,41,5,66,42],[5,54,24,7,55,25],[11,36,12,7,37,13],[5,122,98,1,123,99],[7,73,45,3,74,46],[15,43,19,2,44,20],[3,45,15,13,46,16],[1,135,107,5,136,108],[10,74,46,1,75,47],[1,50,22,15,51,23],[2,42,14,17,43,15],[5,150,120,1,151,121],[9,69,43,4,70,44],[17,50,22,1,51,23],[2,42,14,19,43,15],[3,141,113,4,142,114],[3,70,44,11,71,45],[17,47,21,4,48,22],[9,39,13,16,40,14],[3,135,107,5,136,108],[3,67,41,13,68,42],[15,54,24,5,55,25],[15,43,15,10,44,16],[4,144,116,4,145,117],[17,68,42],[17,50,22,6,51,23],[19,46,16,6,47,17],[2,139,111,7,140,112],[17,74,46],[7,54,24,16,55,25],[34,37,13],[4,151,121,5,152,122],[4,75,47,14,76,48],[11,54,24,14,55,25],[16,45,15,14,46,16],[6,147,117,4,148,118],[6,73,45,14,74,46],[11,54,24,16,55,25],[30,46,16,2,47,17],[8,132,106,4,133,107],[8,75,47,13,76,48],[7,54,24,22,55,25],[22,45,15,13,46,16],[10,142,114,2,143,115],[19,74,46,4,75,47],[28,50,22,6,51,23],[33,46,16,4,47,17],[8,152,122,4,153,123],[22,73,45,3,74,46],[8,53,23,26,54,24],[12,45,15,28,46,16],[3,147,117,10,148,118],[3,73,45,23,74,46],[4,54,24,31,55,25],[11,45,15,31,46,16],[7,146,116,7,147,117],[21,73,45,7,74,46],[1,53,23,37,54,24],[19,45,15,26,46,16],[5,145,115,10,146,116],[19,75,47,10,76,48],[15,54,24,25,55,25],[23,45,15,25,46,16],[13,145,115,3,146,116],[2,74,46,29,75,47],[42,54,24,1,55,25],[23,45,15,28,46,16],[17,145,115],[10,74,46,23,75,47],[10,54,24,35,55,25],[19,45,15,35,46,16],[17,145,115,1,146,116],[14,74,46,21,75,47],[29,54,24,19,55,25],[11,45,15,46,46,16],[13,145,115,6,146,116],[14,74,46,23,75,47],[44,54,24,7,55,25],[59,46,16,1,47,17],[12,151,121,7,152,122],[12,75,47,26,76,48],[39,54,24,14,55,25],[22,45,15,41,46,16],[6,151,121,14,152,122],[6,75,47,34,76,48],[46,54,24,10,55,25],[2,45,15,64,46,16],[17,152,122,4,153,123],[29,74,46,14,75,47],[49,54,24,10,55,25],[24,45,15,46,46,16],[4,152,122,18,153,123],[13,74,46,32,75,47],[48,54,24,14,55,25],[42,45,15,32,46,16],[20,147,117,4,148,118],[40,75,47,7,76,48],[43,54,24,22,55,25],[10,45,15,67,46,16],[19,148,118,6,149,119],[18,75,47,31,76,48],[34,54,24,34,55,25],[20,45,15,61,46,16]],u={I:function(h,f){var m=c(h,f);if(typeof m>"u")throw Error("bad rs block @ typeNumber:"+h+"/errorCorrectLevel:"+f);h=m.length/3,f=[];for(var g=0;g<h;g+=1)for(var b=m[3*g],k=m[3*g+1],$=m[3*g+2],w=0;w<b;w+=1){var _=$,v={};v.o=k,v.j=_,f.push(v)}return f}};return u}();return o}());const pf=QrCreator;var ie=class extends T{constructor(){super(...arguments),this.value="",this.label="",this.size=128,this.fill="black",this.background="white",this.radius=0,this.errorCorrection="H"}firstUpdated(){this.generate()}generate(){this.hasUpdated&&pf.render({text:this.value,radius:this.radius,ecLevel:this.errorCorrection,fill:this.fill,background:this.background,size:this.size*2},this.canvas)}render(){var e;return x`
      <canvas
        part="base"
        class="qr-code"
        role="img"
        aria-label=${((e=this.label)==null?void 0:e.length)>0?this.label:this.value}
        style=${Rt({width:`${this.size}px`,height:`${this.size}px`})}
      ></canvas>
    `}};ie.styles=[D,uf];l([S("canvas")],ie.prototype,"canvas",2);l([p()],ie.prototype,"value",2);l([p()],ie.prototype,"label",2);l([p({type:Number})],ie.prototype,"size",2);l([p()],ie.prototype,"fill",2);l([p()],ie.prototype,"background",2);l([p({type:Number})],ie.prototype,"radius",2);l([p({attribute:"error-correction"})],ie.prototype,"errorCorrection",2);l([C(["background","errorCorrection","fill","radius","size","value"])],ie.prototype,"generate",1);ie.define("sl-qr-code");var ff=O`
  :host {
    display: block;
  }

  :host(:focus-visible) {
    outline: 0px;
  }

  .radio {
    display: inline-flex;
    align-items: top;
    font-family: var(--sl-input-font-family);
    font-size: var(--sl-input-font-size-medium);
    font-weight: var(--sl-input-font-weight);
    color: var(--sl-input-label-color);
    vertical-align: middle;
    cursor: pointer;
  }

  .radio--small {
    --toggle-size: var(--sl-toggle-size-small);
    font-size: var(--sl-input-font-size-small);
  }

  .radio--medium {
    --toggle-size: var(--sl-toggle-size-medium);
    font-size: var(--sl-input-font-size-medium);
  }

  .radio--large {
    --toggle-size: var(--sl-toggle-size-large);
    font-size: var(--sl-input-font-size-large);
  }

  .radio__checked-icon {
    display: inline-flex;
    width: var(--toggle-size);
    height: var(--toggle-size);
  }

  .radio__control {
    flex: 0 0 auto;
    position: relative;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: var(--toggle-size);
    height: var(--toggle-size);
    border: solid var(--sl-input-border-width) var(--sl-input-border-color);
    border-radius: 50%;
    background-color: var(--sl-input-background-color);
    color: transparent;
    transition:
      var(--sl-transition-fast) border-color,
      var(--sl-transition-fast) background-color,
      var(--sl-transition-fast) color,
      var(--sl-transition-fast) box-shadow;
  }

  .radio__input {
    position: absolute;
    opacity: 0;
    padding: 0;
    margin: 0;
    pointer-events: none;
  }

  /* Hover */
  .radio:not(.radio--checked):not(.radio--disabled) .radio__control:hover {
    border-color: var(--sl-input-border-color-hover);
    background-color: var(--sl-input-background-color-hover);
  }

  /* Checked */
  .radio--checked .radio__control {
    color: var(--sl-color-neutral-0);
    border-color: var(--sl-color-primary-600);
    background-color: var(--sl-color-primary-600);
  }

  /* Checked + hover */
  .radio.radio--checked:not(.radio--disabled) .radio__control:hover {
    border-color: var(--sl-color-primary-500);
    background-color: var(--sl-color-primary-500);
  }

  /* Checked + focus */
  :host(:focus-visible) .radio__control {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  /* Disabled */
  .radio--disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  /* When the control isn't checked, hide the circle for Windows High Contrast mode a11y */
  .radio:not(.radio--checked) svg circle {
    opacity: 0;
  }

  .radio__label {
    display: inline-block;
    color: var(--sl-input-label-color);
    line-height: var(--toggle-size);
    margin-inline-start: 0.5em;
    user-select: none;
    -webkit-user-select: none;
  }
`,be=class extends T{constructor(){super(),this.checked=!1,this.hasFocus=!1,this.size="medium",this.disabled=!1,this.handleBlur=()=>{this.hasFocus=!1,this.emit("sl-blur")},this.handleClick=()=>{this.disabled||(this.checked=!0)},this.handleFocus=()=>{this.hasFocus=!0,this.emit("sl-focus")},this.addEventListener("blur",this.handleBlur),this.addEventListener("click",this.handleClick),this.addEventListener("focus",this.handleFocus)}connectedCallback(){super.connectedCallback(),this.setInitialAttributes()}setInitialAttributes(){this.setAttribute("role","radio"),this.setAttribute("tabindex","-1"),this.setAttribute("aria-disabled",this.disabled?"true":"false")}handleCheckedChange(){this.setAttribute("aria-checked",this.checked?"true":"false"),this.setAttribute("tabindex",this.checked?"0":"-1")}handleDisabledChange(){this.setAttribute("aria-disabled",this.disabled?"true":"false")}render(){return x`
      <span
        part="base"
        class=${R({radio:!0,"radio--checked":this.checked,"radio--disabled":this.disabled,"radio--focused":this.hasFocus,"radio--small":this.size==="small","radio--medium":this.size==="medium","radio--large":this.size==="large"})}
      >
        <span part="${`control${this.checked?" control--checked":""}`}" class="radio__control">
          ${this.checked?x` <sl-icon part="checked-icon" class="radio__checked-icon" library="system" name="radio"></sl-icon> `:""}
        </span>

        <slot part="label" class="radio__label"></slot>
      </span>
    `}};be.styles=[D,ff];be.dependencies={"sl-icon":J};l([z()],be.prototype,"checked",2);l([z()],be.prototype,"hasFocus",2);l([p()],be.prototype,"value",2);l([p({reflect:!0})],be.prototype,"size",2);l([p({type:Boolean,reflect:!0})],be.prototype,"disabled",2);l([C("checked")],be.prototype,"handleCheckedChange",1);l([C("disabled",{waitUntilFirstUpdate:!0})],be.prototype,"handleDisabledChange",1);be.define("sl-radio");var mf=O`
  :host {
    display: block;
    user-select: none;
    -webkit-user-select: none;
  }

  :host(:focus) {
    outline: none;
  }

  .option {
    position: relative;
    display: flex;
    align-items: center;
    font-family: var(--sl-font-sans);
    font-size: var(--sl-font-size-medium);
    font-weight: var(--sl-font-weight-normal);
    line-height: var(--sl-line-height-normal);
    letter-spacing: var(--sl-letter-spacing-normal);
    color: var(--sl-color-neutral-700);
    padding: var(--sl-spacing-x-small) var(--sl-spacing-medium) var(--sl-spacing-x-small) var(--sl-spacing-x-small);
    transition: var(--sl-transition-fast) fill;
    cursor: pointer;
  }

  .option--hover:not(.option--current):not(.option--disabled) {
    background-color: var(--sl-color-neutral-100);
    color: var(--sl-color-neutral-1000);
  }

  .option--current,
  .option--current.option--disabled {
    background-color: var(--sl-color-primary-600);
    color: var(--sl-color-neutral-0);
    opacity: 1;
  }

  .option--disabled {
    outline: none;
    opacity: 0.5;
    cursor: not-allowed;
  }

  .option__label {
    flex: 1 1 auto;
    display: inline-block;
    line-height: var(--sl-line-height-dense);
  }

  .option .option__check {
    flex: 0 0 auto;
    display: flex;
    align-items: center;
    justify-content: center;
    visibility: hidden;
    padding-inline-end: var(--sl-spacing-2x-small);
  }

  .option--selected .option__check {
    visibility: visible;
  }

  .option__prefix,
  .option__suffix {
    flex: 0 0 auto;
    display: flex;
    align-items: center;
  }

  .option__prefix::slotted(*) {
    margin-inline-end: var(--sl-spacing-x-small);
  }

  .option__suffix::slotted(*) {
    margin-inline-start: var(--sl-spacing-x-small);
  }

  @media (forced-colors: active) {
    :host(:hover:not([aria-disabled='true'])) .option {
      outline: dashed 1px SelectedItem;
      outline-offset: -1px;
    }
  }
`,Xt=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.isInitialized=!1,this.current=!1,this.selected=!1,this.hasHover=!1,this.value="",this.disabled=!1}connectedCallback(){super.connectedCallback(),this.setAttribute("role","option"),this.setAttribute("aria-selected","false")}handleDefaultSlotChange(){this.isInitialized?customElements.whenDefined("sl-select").then(()=>{const e=this.closest("sl-select");e&&e.handleDefaultSlotChange()}):this.isInitialized=!0}handleMouseEnter(){this.hasHover=!0}handleMouseLeave(){this.hasHover=!1}handleDisabledChange(){this.setAttribute("aria-disabled",this.disabled?"true":"false")}handleSelectedChange(){this.setAttribute("aria-selected",this.selected?"true":"false")}handleValueChange(){typeof this.value!="string"&&(this.value=String(this.value)),this.value.includes(" ")&&(console.error("Option values cannot include a space. All spaces have been replaced with underscores.",this),this.value=this.value.replace(/ /g,"_"))}getTextLabel(){const e=this.childNodes;let t="";return[...e].forEach(o=>{o.nodeType===Node.ELEMENT_NODE&&(o.hasAttribute("slot")||(t+=o.textContent)),o.nodeType===Node.TEXT_NODE&&(t+=o.textContent)}),t.trim()}render(){return x`
      <div
        part="base"
        class=${R({option:!0,"option--current":this.current,"option--disabled":this.disabled,"option--selected":this.selected,"option--hover":this.hasHover})}
        @mouseenter=${this.handleMouseEnter}
        @mouseleave=${this.handleMouseLeave}
      >
        <sl-icon part="checked-icon" class="option__check" name="check" library="system" aria-hidden="true"></sl-icon>
        <slot part="prefix" name="prefix" class="option__prefix"></slot>
        <slot part="label" class="option__label" @slotchange=${this.handleDefaultSlotChange}></slot>
        <slot part="suffix" name="suffix" class="option__suffix"></slot>
      </div>
    `}};Xt.styles=[D,mf];Xt.dependencies={"sl-icon":J};l([S(".option__label")],Xt.prototype,"defaultSlot",2);l([z()],Xt.prototype,"current",2);l([z()],Xt.prototype,"selected",2);l([z()],Xt.prototype,"hasHover",2);l([p({reflect:!0})],Xt.prototype,"value",2);l([p({type:Boolean,reflect:!0})],Xt.prototype,"disabled",2);l([C("disabled")],Xt.prototype,"handleDisabledChange",1);l([C("selected")],Xt.prototype,"handleSelectedChange",1);l([C("value")],Xt.prototype,"handleValueChange",1);Xt.define("sl-option");Y.define("sl-popup");var gf=O`
  :host {
    --height: 1rem;
    --track-color: var(--sl-color-neutral-200);
    --indicator-color: var(--sl-color-primary-600);
    --label-color: var(--sl-color-neutral-0);

    display: block;
  }

  .progress-bar {
    position: relative;
    background-color: var(--track-color);
    height: var(--height);
    border-radius: var(--sl-border-radius-pill);
    box-shadow: inset var(--sl-shadow-small);
    overflow: hidden;
  }

  .progress-bar__indicator {
    height: 100%;
    font-family: var(--sl-font-sans);
    font-size: 12px;
    font-weight: var(--sl-font-weight-normal);
    background-color: var(--indicator-color);
    color: var(--label-color);
    text-align: center;
    line-height: var(--height);
    white-space: nowrap;
    overflow: hidden;
    transition:
      400ms width,
      400ms background-color;
    user-select: none;
    -webkit-user-select: none;
  }

  /* Indeterminate */
  .progress-bar--indeterminate .progress-bar__indicator {
    position: absolute;
    animation: indeterminate 2.5s infinite cubic-bezier(0.37, 0, 0.63, 1);
  }

  .progress-bar--indeterminate.progress-bar--rtl .progress-bar__indicator {
    animation-name: indeterminate-rtl;
  }

  @media (forced-colors: active) {
    .progress-bar {
      outline: solid 1px SelectedItem;
      background-color: var(--sl-color-neutral-0);
    }

    .progress-bar__indicator {
      outline: solid 1px SelectedItem;
      background-color: SelectedItem;
    }
  }

  @keyframes indeterminate {
    0% {
      left: -50%;
      width: 50%;
    }
    75%,
    100% {
      left: 100%;
      width: 50%;
    }
  }

  @keyframes indeterminate-rtl {
    0% {
      right: -50%;
      width: 50%;
    }
    75%,
    100% {
      right: 100%;
      width: 50%;
    }
  }
`,Fi=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.value=0,this.indeterminate=!1,this.label=""}render(){return x`
      <div
        part="base"
        class=${R({"progress-bar":!0,"progress-bar--indeterminate":this.indeterminate,"progress-bar--rtl":this.localize.dir()==="rtl"})}
        role="progressbar"
        title=${E(this.title)}
        aria-label=${this.label.length>0?this.label:this.localize.term("progress")}
        aria-valuemin="0"
        aria-valuemax="100"
        aria-valuenow=${this.indeterminate?0:this.value}
      >
        <div part="indicator" class="progress-bar__indicator" style=${Rt({width:`${this.value}%`})}>
          ${this.indeterminate?"":x` <slot part="label" class="progress-bar__label"></slot> `}
        </div>
      </div>
    `}};Fi.styles=[D,gf];l([p({type:Number,reflect:!0})],Fi.prototype,"value",2);l([p({type:Boolean,reflect:!0})],Fi.prototype,"indeterminate",2);l([p()],Fi.prototype,"label",2);Fi.define("sl-progress-bar");var bf=O`
  :host {
    display: block;
  }

  .menu-label {
    display: inline-block;
    font-family: var(--sl-font-sans);
    font-size: var(--sl-font-size-small);
    font-weight: var(--sl-font-weight-semibold);
    line-height: var(--sl-line-height-normal);
    letter-spacing: var(--sl-letter-spacing-normal);
    color: var(--sl-color-neutral-500);
    padding: var(--sl-spacing-2x-small) var(--sl-spacing-x-large);
    user-select: none;
    -webkit-user-select: none;
  }
`,Jl=class extends T{render(){return x` <slot part="base" class="menu-label"></slot> `}};Jl.styles=[D,bf];Jl.define("sl-menu-label");var vf=O`
  :host {
    display: contents;
  }
`,ve=class extends T{constructor(){super(...arguments),this.attrOldValue=!1,this.charData=!1,this.charDataOldValue=!1,this.childList=!1,this.disabled=!1,this.handleMutation=e=>{this.emit("sl-mutation",{detail:{mutationList:e}})}}connectedCallback(){super.connectedCallback(),this.mutationObserver=new MutationObserver(this.handleMutation),this.disabled||this.startObserver()}disconnectedCallback(){super.disconnectedCallback(),this.stopObserver()}startObserver(){const e=typeof this.attr=="string"&&this.attr.length>0,t=e&&this.attr!=="*"?this.attr.split(" "):void 0;try{this.mutationObserver.observe(this,{subtree:!0,childList:this.childList,attributes:e,attributeFilter:t,attributeOldValue:this.attrOldValue,characterData:this.charData,characterDataOldValue:this.charDataOldValue})}catch{}}stopObserver(){this.mutationObserver.disconnect()}handleDisabledChange(){this.disabled?this.stopObserver():this.startObserver()}handleChange(){this.stopObserver(),this.startObserver()}render(){return x` <slot></slot> `}};ve.styles=[D,vf];l([p({reflect:!0})],ve.prototype,"attr",2);l([p({attribute:"attr-old-value",type:Boolean,reflect:!0})],ve.prototype,"attrOldValue",2);l([p({attribute:"char-data",type:Boolean,reflect:!0})],ve.prototype,"charData",2);l([p({attribute:"char-data-old-value",type:Boolean,reflect:!0})],ve.prototype,"charDataOldValue",2);l([p({attribute:"child-list",type:Boolean,reflect:!0})],ve.prototype,"childList",2);l([p({type:Boolean,reflect:!0})],ve.prototype,"disabled",2);l([C("disabled")],ve.prototype,"handleDisabledChange",1);l([C("attr",{waitUntilFirstUpdate:!0}),C("attr-old-value",{waitUntilFirstUpdate:!0}),C("char-data",{waitUntilFirstUpdate:!0}),C("char-data-old-value",{waitUntilFirstUpdate:!0}),C("childList",{waitUntilFirstUpdate:!0})],ve.prototype,"handleChange",1);ve.define("sl-mutation-observer");var yf=O`
  :host {
    display: block;
  }

  .input {
    flex: 1 1 auto;
    display: inline-flex;
    align-items: stretch;
    justify-content: start;
    position: relative;
    width: 100%;
    font-family: var(--sl-input-font-family);
    font-weight: var(--sl-input-font-weight);
    letter-spacing: var(--sl-input-letter-spacing);
    vertical-align: middle;
    overflow: hidden;
    cursor: text;
    transition:
      var(--sl-transition-fast) color,
      var(--sl-transition-fast) border,
      var(--sl-transition-fast) box-shadow,
      var(--sl-transition-fast) background-color;
  }

  /* Standard inputs */
  .input--standard {
    background-color: var(--sl-input-background-color);
    border: solid var(--sl-input-border-width) var(--sl-input-border-color);
  }

  .input--standard:hover:not(.input--disabled) {
    background-color: var(--sl-input-background-color-hover);
    border-color: var(--sl-input-border-color-hover);
  }

  .input--standard.input--focused:not(.input--disabled) {
    background-color: var(--sl-input-background-color-focus);
    border-color: var(--sl-input-border-color-focus);
    box-shadow: 0 0 0 var(--sl-focus-ring-width) var(--sl-input-focus-ring-color);
  }

  .input--standard.input--focused:not(.input--disabled) .input__control {
    color: var(--sl-input-color-focus);
  }

  .input--standard.input--disabled {
    background-color: var(--sl-input-background-color-disabled);
    border-color: var(--sl-input-border-color-disabled);
    opacity: 0.5;
    cursor: not-allowed;
  }

  .input--standard.input--disabled .input__control {
    color: var(--sl-input-color-disabled);
  }

  .input--standard.input--disabled .input__control::placeholder {
    color: var(--sl-input-placeholder-color-disabled);
  }

  /* Filled inputs */
  .input--filled {
    border: none;
    background-color: var(--sl-input-filled-background-color);
    color: var(--sl-input-color);
  }

  .input--filled:hover:not(.input--disabled) {
    background-color: var(--sl-input-filled-background-color-hover);
  }

  .input--filled.input--focused:not(.input--disabled) {
    background-color: var(--sl-input-filled-background-color-focus);
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  .input--filled.input--disabled {
    background-color: var(--sl-input-filled-background-color-disabled);
    opacity: 0.5;
    cursor: not-allowed;
  }

  .input__control {
    flex: 1 1 auto;
    font-family: inherit;
    font-size: inherit;
    font-weight: inherit;
    min-width: 0;
    height: 100%;
    color: var(--sl-input-color);
    border: none;
    background: inherit;
    box-shadow: none;
    padding: 0;
    margin: 0;
    cursor: inherit;
    -webkit-appearance: none;
  }

  .input__control::-webkit-search-decoration,
  .input__control::-webkit-search-cancel-button,
  .input__control::-webkit-search-results-button,
  .input__control::-webkit-search-results-decoration {
    -webkit-appearance: none;
  }

  .input__control:-webkit-autofill,
  .input__control:-webkit-autofill:hover,
  .input__control:-webkit-autofill:focus,
  .input__control:-webkit-autofill:active {
    box-shadow: 0 0 0 var(--sl-input-height-large) var(--sl-input-background-color-hover) inset !important;
    -webkit-text-fill-color: var(--sl-color-primary-500);
    caret-color: var(--sl-input-color);
  }

  .input--filled .input__control:-webkit-autofill,
  .input--filled .input__control:-webkit-autofill:hover,
  .input--filled .input__control:-webkit-autofill:focus,
  .input--filled .input__control:-webkit-autofill:active {
    box-shadow: 0 0 0 var(--sl-input-height-large) var(--sl-input-filled-background-color) inset !important;
  }

  .input__control::placeholder {
    color: var(--sl-input-placeholder-color);
    user-select: none;
    -webkit-user-select: none;
  }

  .input:hover:not(.input--disabled) .input__control {
    color: var(--sl-input-color-hover);
  }

  .input__control:focus {
    outline: none;
  }

  .input__prefix,
  .input__suffix {
    display: inline-flex;
    flex: 0 0 auto;
    align-items: center;
    cursor: default;
  }

  .input__prefix ::slotted(sl-icon),
  .input__suffix ::slotted(sl-icon) {
    color: var(--sl-input-icon-color);
  }

  /*
   * Size modifiers
   */

  .input--small {
    border-radius: var(--sl-input-border-radius-small);
    font-size: var(--sl-input-font-size-small);
    height: var(--sl-input-height-small);
  }

  .input--small .input__control {
    height: calc(var(--sl-input-height-small) - var(--sl-input-border-width) * 2);
    padding: 0 var(--sl-input-spacing-small);
  }

  .input--small .input__clear,
  .input--small .input__password-toggle {
    width: calc(1em + var(--sl-input-spacing-small) * 2);
  }

  .input--small .input__prefix ::slotted(*) {
    margin-inline-start: var(--sl-input-spacing-small);
  }

  .input--small .input__suffix ::slotted(*) {
    margin-inline-end: var(--sl-input-spacing-small);
  }

  .input--medium {
    border-radius: var(--sl-input-border-radius-medium);
    font-size: var(--sl-input-font-size-medium);
    height: var(--sl-input-height-medium);
  }

  .input--medium .input__control {
    height: calc(var(--sl-input-height-medium) - var(--sl-input-border-width) * 2);
    padding: 0 var(--sl-input-spacing-medium);
  }

  .input--medium .input__clear,
  .input--medium .input__password-toggle {
    width: calc(1em + var(--sl-input-spacing-medium) * 2);
  }

  .input--medium .input__prefix ::slotted(*) {
    margin-inline-start: var(--sl-input-spacing-medium);
  }

  .input--medium .input__suffix ::slotted(*) {
    margin-inline-end: var(--sl-input-spacing-medium);
  }

  .input--large {
    border-radius: var(--sl-input-border-radius-large);
    font-size: var(--sl-input-font-size-large);
    height: var(--sl-input-height-large);
  }

  .input--large .input__control {
    height: calc(var(--sl-input-height-large) - var(--sl-input-border-width) * 2);
    padding: 0 var(--sl-input-spacing-large);
  }

  .input--large .input__clear,
  .input--large .input__password-toggle {
    width: calc(1em + var(--sl-input-spacing-large) * 2);
  }

  .input--large .input__prefix ::slotted(*) {
    margin-inline-start: var(--sl-input-spacing-large);
  }

  .input--large .input__suffix ::slotted(*) {
    margin-inline-end: var(--sl-input-spacing-large);
  }

  /*
   * Pill modifier
   */

  .input--pill.input--small {
    border-radius: var(--sl-input-height-small);
  }

  .input--pill.input--medium {
    border-radius: var(--sl-input-height-medium);
  }

  .input--pill.input--large {
    border-radius: var(--sl-input-height-large);
  }

  /*
   * Clearable + Password Toggle
   */

  .input__clear,
  .input__password-toggle {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-size: inherit;
    color: var(--sl-input-icon-color);
    border: none;
    background: none;
    padding: 0;
    transition: var(--sl-transition-fast) color;
    cursor: pointer;
  }

  .input__clear:hover,
  .input__password-toggle:hover {
    color: var(--sl-input-icon-color-hover);
  }

  .input__clear:focus,
  .input__password-toggle:focus {
    outline: none;
  }

  /* Don't show the browser's password toggle in Edge */
  ::-ms-reveal {
    display: none;
  }

  /* Hide the built-in number spinner */
  .input--no-spin-buttons input[type='number']::-webkit-outer-spin-button,
  .input--no-spin-buttons input[type='number']::-webkit-inner-spin-button {
    -webkit-appearance: none;
    display: none;
  }

  .input--no-spin-buttons input[type='number'] {
    -moz-appearance: textfield;
  }
`,V=class extends T{constructor(){super(...arguments),this.formControlController=new Ee(this,{assumeInteractionOn:["sl-blur","sl-input"]}),this.hasSlotController=new Ot(this,"help-text","label"),this.localize=new j(this),this.hasFocus=!1,this.title="",this.__numberInput=Object.assign(document.createElement("input"),{type:"number"}),this.__dateInput=Object.assign(document.createElement("input"),{type:"date"}),this.type="text",this.name="",this.value="",this.defaultValue="",this.size="medium",this.filled=!1,this.pill=!1,this.label="",this.helpText="",this.clearable=!1,this.disabled=!1,this.placeholder="",this.readonly=!1,this.passwordToggle=!1,this.passwordVisible=!1,this.noSpinButtons=!1,this.form="",this.required=!1,this.spellcheck=!0}get valueAsDate(){var e;return this.__dateInput.type=this.type,this.__dateInput.value=this.value,((e=this.input)==null?void 0:e.valueAsDate)||this.__dateInput.valueAsDate}set valueAsDate(e){this.__dateInput.type=this.type,this.__dateInput.valueAsDate=e,this.value=this.__dateInput.value}get valueAsNumber(){var e;return this.__numberInput.value=this.value,((e=this.input)==null?void 0:e.valueAsNumber)||this.__numberInput.valueAsNumber}set valueAsNumber(e){this.__numberInput.valueAsNumber=e,this.value=this.__numberInput.value}get validity(){return this.input.validity}get validationMessage(){return this.input.validationMessage}firstUpdated(){this.formControlController.updateValidity()}handleBlur(){this.hasFocus=!1,this.emit("sl-blur")}handleChange(){this.value=this.input.value,this.emit("sl-change")}handleClearClick(e){e.preventDefault(),this.value!==""&&(this.value="",this.emit("sl-clear"),this.emit("sl-input"),this.emit("sl-change")),this.input.focus()}handleFocus(){this.hasFocus=!0,this.emit("sl-focus")}handleInput(){this.value=this.input.value,this.formControlController.updateValidity(),this.emit("sl-input")}handleInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}handleKeyDown(e){const t=e.metaKey||e.ctrlKey||e.shiftKey||e.altKey;e.key==="Enter"&&!t&&setTimeout(()=>{!e.defaultPrevented&&!e.isComposing&&this.formControlController.submit()})}handlePasswordToggle(){this.passwordVisible=!this.passwordVisible}handleDisabledChange(){this.formControlController.setValidity(this.disabled)}handleStepChange(){this.input.step=String(this.step),this.formControlController.updateValidity()}async handleValueChange(){await this.updateComplete,this.formControlController.updateValidity()}focus(e){this.input.focus(e)}blur(){this.input.blur()}select(){this.input.select()}setSelectionRange(e,t,o="none"){this.input.setSelectionRange(e,t,o)}setRangeText(e,t,o,i="preserve"){const s=t??this.input.selectionStart,r=o??this.input.selectionEnd;this.input.setRangeText(e,s,r,i),this.value!==this.input.value&&(this.value=this.input.value)}showPicker(){"showPicker"in HTMLInputElement.prototype&&this.input.showPicker()}stepUp(){this.input.stepUp(),this.value!==this.input.value&&(this.value=this.input.value)}stepDown(){this.input.stepDown(),this.value!==this.input.value&&(this.value=this.input.value)}checkValidity(){return this.input.checkValidity()}getForm(){return this.formControlController.getForm()}reportValidity(){return this.input.reportValidity()}setCustomValidity(e){this.input.setCustomValidity(e),this.formControlController.updateValidity()}render(){const e=this.hasSlotController.test("label"),t=this.hasSlotController.test("help-text"),o=this.label?!0:!!e,i=this.helpText?!0:!!t,r=this.clearable&&!this.disabled&&!this.readonly&&(typeof this.value=="number"||this.value.length>0);return x`
      <div
        part="form-control"
        class=${R({"form-control":!0,"form-control--small":this.size==="small","form-control--medium":this.size==="medium","form-control--large":this.size==="large","form-control--has-label":o,"form-control--has-help-text":i})}
      >
        <label
          part="form-control-label"
          class="form-control__label"
          for="input"
          aria-hidden=${o?"false":"true"}
        >
          <slot name="label">${this.label}</slot>
        </label>

        <div part="form-control-input" class="form-control-input">
          <div
            part="base"
            class=${R({input:!0,"input--small":this.size==="small","input--medium":this.size==="medium","input--large":this.size==="large","input--pill":this.pill,"input--standard":!this.filled,"input--filled":this.filled,"input--disabled":this.disabled,"input--focused":this.hasFocus,"input--empty":!this.value,"input--no-spin-buttons":this.noSpinButtons})}
          >
            <span part="prefix" class="input__prefix">
              <slot name="prefix"></slot>
            </span>

            <input
              part="input"
              id="input"
              class="input__control"
              type=${this.type==="password"&&this.passwordVisible?"text":this.type}
              title=${this.title}
              name=${E(this.name)}
              ?disabled=${this.disabled}
              ?readonly=${this.readonly}
              ?required=${this.required}
              placeholder=${E(this.placeholder)}
              minlength=${E(this.minlength)}
              maxlength=${E(this.maxlength)}
              min=${E(this.min)}
              max=${E(this.max)}
              step=${E(this.step)}
              .value=${io(this.value)}
              autocapitalize=${E(this.autocapitalize)}
              autocomplete=${E(this.autocomplete)}
              autocorrect=${E(this.autocorrect)}
              ?autofocus=${this.autofocus}
              spellcheck=${this.spellcheck}
              pattern=${E(this.pattern)}
              enterkeyhint=${E(this.enterkeyhint)}
              inputmode=${E(this.inputmode)}
              aria-describedby="help-text"
              @change=${this.handleChange}
              @input=${this.handleInput}
              @invalid=${this.handleInvalid}
              @keydown=${this.handleKeyDown}
              @focus=${this.handleFocus}
              @blur=${this.handleBlur}
            />

            ${r?x`
                  <button
                    part="clear-button"
                    class="input__clear"
                    type="button"
                    aria-label=${this.localize.term("clearEntry")}
                    @click=${this.handleClearClick}
                    tabindex="-1"
                  >
                    <slot name="clear-icon">
                      <sl-icon name="x-circle-fill" library="system"></sl-icon>
                    </slot>
                  </button>
                `:""}
            ${this.passwordToggle&&!this.disabled?x`
                  <button
                    part="password-toggle-button"
                    class="input__password-toggle"
                    type="button"
                    aria-label=${this.localize.term(this.passwordVisible?"hidePassword":"showPassword")}
                    @click=${this.handlePasswordToggle}
                    tabindex="-1"
                  >
                    ${this.passwordVisible?x`
                          <slot name="show-password-icon">
                            <sl-icon name="eye-slash" library="system"></sl-icon>
                          </slot>
                        `:x`
                          <slot name="hide-password-icon">
                            <sl-icon name="eye" library="system"></sl-icon>
                          </slot>
                        `}
                  </button>
                `:""}

            <span part="suffix" class="input__suffix">
              <slot name="suffix"></slot>
            </span>
          </div>
        </div>

        <div
          part="form-control-help-text"
          id="help-text"
          class="form-control__help-text"
          aria-hidden=${i?"false":"true"}
        >
          <slot name="help-text">${this.helpText}</slot>
        </div>
      </div>
    `}};V.styles=[D,uo,yf];V.dependencies={"sl-icon":J};l([S(".input__control")],V.prototype,"input",2);l([z()],V.prototype,"hasFocus",2);l([p()],V.prototype,"title",2);l([p({reflect:!0})],V.prototype,"type",2);l([p()],V.prototype,"name",2);l([p()],V.prototype,"value",2);l([Io()],V.prototype,"defaultValue",2);l([p({reflect:!0})],V.prototype,"size",2);l([p({type:Boolean,reflect:!0})],V.prototype,"filled",2);l([p({type:Boolean,reflect:!0})],V.prototype,"pill",2);l([p()],V.prototype,"label",2);l([p({attribute:"help-text"})],V.prototype,"helpText",2);l([p({type:Boolean})],V.prototype,"clearable",2);l([p({type:Boolean,reflect:!0})],V.prototype,"disabled",2);l([p()],V.prototype,"placeholder",2);l([p({type:Boolean,reflect:!0})],V.prototype,"readonly",2);l([p({attribute:"password-toggle",type:Boolean})],V.prototype,"passwordToggle",2);l([p({attribute:"password-visible",type:Boolean})],V.prototype,"passwordVisible",2);l([p({attribute:"no-spin-buttons",type:Boolean})],V.prototype,"noSpinButtons",2);l([p({reflect:!0})],V.prototype,"form",2);l([p({type:Boolean,reflect:!0})],V.prototype,"required",2);l([p()],V.prototype,"pattern",2);l([p({type:Number})],V.prototype,"minlength",2);l([p({type:Number})],V.prototype,"maxlength",2);l([p()],V.prototype,"min",2);l([p()],V.prototype,"max",2);l([p()],V.prototype,"step",2);l([p()],V.prototype,"autocapitalize",2);l([p()],V.prototype,"autocorrect",2);l([p()],V.prototype,"autocomplete",2);l([p({type:Boolean})],V.prototype,"autofocus",2);l([p()],V.prototype,"enterkeyhint",2);l([p({type:Boolean,converter:{fromAttribute:e=>!(!e||e==="false"),toAttribute:e=>e?"true":"false"}})],V.prototype,"spellcheck",2);l([p()],V.prototype,"inputmode",2);l([C("disabled",{waitUntilFirstUpdate:!0})],V.prototype,"handleDisabledChange",1);l([C("step",{waitUntilFirstUpdate:!0})],V.prototype,"handleStepChange",1);l([C("value",{waitUntilFirstUpdate:!0})],V.prototype,"handleValueChange",1);V.define("sl-input");var wf=O`
  :host {
    display: block;
    position: relative;
    background: var(--sl-panel-background-color);
    border: solid var(--sl-panel-border-width) var(--sl-panel-border-color);
    border-radius: var(--sl-border-radius-medium);
    padding: var(--sl-spacing-x-small) 0;
    overflow: auto;
    overscroll-behavior: none;
  }

  ::slotted(sl-divider) {
    --spacing: var(--sl-spacing-x-small);
  }
`,ma=class extends T{connectedCallback(){super.connectedCallback(),this.setAttribute("role","menu")}handleClick(e){const t=["menuitem","menuitemcheckbox"],o=e.composedPath(),i=o.find(n=>{var c;return t.includes(((c=n==null?void 0:n.getAttribute)==null?void 0:c.call(n,"role"))||"")});if(!i||o.find(n=>{var c;return((c=n==null?void 0:n.getAttribute)==null?void 0:c.call(n,"role"))==="menu"})!==this)return;const a=i;a.type==="checkbox"&&(a.checked=!a.checked),this.emit("sl-select",{detail:{item:a}})}handleKeyDown(e){if(e.key==="Enter"||e.key===" "){const t=this.getCurrentItem();e.preventDefault(),e.stopPropagation(),t==null||t.click()}else if(["ArrowDown","ArrowUp","Home","End"].includes(e.key)){const t=this.getAllItems(),o=this.getCurrentItem();let i=o?t.indexOf(o):0;t.length>0&&(e.preventDefault(),e.stopPropagation(),e.key==="ArrowDown"?i++:e.key==="ArrowUp"?i--:e.key==="Home"?i=0:e.key==="End"&&(i=t.length-1),i<0&&(i=t.length-1),i>t.length-1&&(i=0),this.setCurrentItem(t[i]),t[i].focus())}}handleMouseDown(e){const t=e.target;this.isMenuItem(t)&&this.setCurrentItem(t)}handleSlotChange(){const e=this.getAllItems();e.length>0&&this.setCurrentItem(e[0])}isMenuItem(e){var t;return e.tagName.toLowerCase()==="sl-menu-item"||["menuitem","menuitemcheckbox","menuitemradio"].includes((t=e.getAttribute("role"))!=null?t:"")}getAllItems(){return[...this.defaultSlot.assignedElements({flatten:!0})].filter(e=>!(e.inert||!this.isMenuItem(e)))}getCurrentItem(){return this.getAllItems().find(e=>e.getAttribute("tabindex")==="0")}setCurrentItem(e){this.getAllItems().forEach(o=>{o.setAttribute("tabindex",o===e?"0":"-1")})}render(){return x`
      <slot
        @slotchange=${this.handleSlotChange}
        @click=${this.handleClick}
        @keydown=${this.handleKeyDown}
        @mousedown=${this.handleMouseDown}
      ></slot>
    `}};ma.styles=[D,wf];l([S("slot")],ma.prototype,"defaultSlot",2);ma.define("sl-menu");var xf=O`
  :host {
    --submenu-offset: -2px;

    display: block;
  }

  :host([inert]) {
    display: none;
  }

  .menu-item {
    position: relative;
    display: flex;
    align-items: stretch;
    font-family: var(--sl-font-sans);
    font-size: var(--sl-font-size-medium);
    font-weight: var(--sl-font-weight-normal);
    line-height: var(--sl-line-height-normal);
    letter-spacing: var(--sl-letter-spacing-normal);
    color: var(--sl-color-neutral-700);
    padding: var(--sl-spacing-2x-small) var(--sl-spacing-2x-small);
    transition: var(--sl-transition-fast) fill;
    user-select: none;
    -webkit-user-select: none;
    white-space: nowrap;
    cursor: pointer;
  }

  .menu-item.menu-item--disabled {
    outline: none;
    opacity: 0.5;
    cursor: not-allowed;
  }

  .menu-item.menu-item--loading {
    outline: none;
    cursor: wait;
  }

  .menu-item.menu-item--loading *:not(sl-spinner) {
    opacity: 0.5;
  }

  .menu-item--loading sl-spinner {
    --indicator-color: currentColor;
    --track-width: 1px;
    position: absolute;
    font-size: 0.75em;
    top: calc(50% - 0.5em);
    left: 0.65rem;
    opacity: 1;
  }

  .menu-item .menu-item__label {
    flex: 1 1 auto;
    display: inline-block;
    text-overflow: ellipsis;
    overflow: hidden;
  }

  .menu-item .menu-item__prefix {
    flex: 0 0 auto;
    display: flex;
    align-items: center;
  }

  .menu-item .menu-item__prefix::slotted(*) {
    margin-inline-end: var(--sl-spacing-x-small);
  }

  .menu-item .menu-item__suffix {
    flex: 0 0 auto;
    display: flex;
    align-items: center;
  }

  .menu-item .menu-item__suffix::slotted(*) {
    margin-inline-start: var(--sl-spacing-x-small);
  }

  /* Safe triangle */
  .menu-item--submenu-expanded::after {
    content: '';
    position: fixed;
    z-index: calc(var(--sl-z-index-dropdown) - 1);
    top: 0;
    right: 0;
    bottom: 0;
    left: 0;
    clip-path: polygon(
      var(--safe-triangle-cursor-x, 0) var(--safe-triangle-cursor-y, 0),
      var(--safe-triangle-submenu-start-x, 0) var(--safe-triangle-submenu-start-y, 0),
      var(--safe-triangle-submenu-end-x, 0) var(--safe-triangle-submenu-end-y, 0)
    );
  }

  :host(:focus-visible) {
    outline: none;
  }

  :host(:hover:not([aria-disabled='true'], :focus-visible)) .menu-item,
  .menu-item--submenu-expanded {
    background-color: var(--sl-color-neutral-100);
    color: var(--sl-color-neutral-1000);
  }

  :host(:focus-visible) .menu-item {
    outline: none;
    background-color: var(--sl-color-primary-600);
    color: var(--sl-color-neutral-0);
    opacity: 1;
  }

  .menu-item .menu-item__check,
  .menu-item .menu-item__chevron {
    flex: 0 0 auto;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 1.5em;
    visibility: hidden;
  }

  .menu-item--checked .menu-item__check,
  .menu-item--has-submenu .menu-item__chevron {
    visibility: visible;
  }

  /* Add elevation and z-index to submenus */
  sl-popup::part(popup) {
    box-shadow: var(--sl-shadow-large);
    z-index: var(--sl-z-index-dropdown);
    margin-left: var(--submenu-offset);
  }

  .menu-item--rtl sl-popup::part(popup) {
    margin-left: calc(-1 * var(--submenu-offset));
  }

  @media (forced-colors: active) {
    :host(:hover:not([aria-disabled='true'])) .menu-item,
    :host(:focus-visible) .menu-item {
      outline: dashed 1px SelectedItem;
      outline-offset: -1px;
    }
  }

  ::slotted(sl-menu) {
    max-width: var(--auto-size-available-width) !important;
    max-height: var(--auto-size-available-height) !important;
  }
`;/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const pi=(e,t)=>{var i;const o=e._$AN;if(o===void 0)return!1;for(const s of o)(i=s._$AO)==null||i.call(s,t,!1),pi(s,t);return!0},ks=e=>{let t,o;do{if((t=e._$AM)===void 0)break;o=t._$AN,o.delete(e),e=t}while((o==null?void 0:o.size)===0)},tc=e=>{for(let t;t=e._$AM;e=t){let o=t._$AN;if(o===void 0)t._$AN=o=new Set;else if(o.has(e))break;o.add(e),$f(t)}};function kf(e){this._$AN!==void 0?(ks(this),this._$AM=e,tc(this)):this._$AM=e}function _f(e,t=!1,o=0){const i=this._$AH,s=this._$AN;if(s!==void 0&&s.size!==0)if(t)if(Array.isArray(i))for(let r=o;r<i.length;r++)pi(i[r],!1),ks(i[r]);else i!=null&&(pi(i,!1),ks(i));else pi(this,e)}const $f=e=>{e.type==ce.CHILD&&(e._$AP??(e._$AP=_f),e._$AQ??(e._$AQ=kf))};class Cf extends Ri{constructor(){super(...arguments),this._$AN=void 0}_$AT(t,o,i){super._$AT(t,o,i),tc(this),this.isConnected=t._$AU}_$AO(t,o=!0){var i,s;t!==this.isConnected&&(this.isConnected=t,t?(i=this.reconnected)==null||i.call(this):(s=this.disconnected)==null||s.call(this)),o&&(pi(this,t),ks(this))}setValue(t){if(Ol(this._$Ct))this._$Ct._$AI(t,this);else{const o=[...this._$Ct._$AH];o[this._$Ci]=t,this._$Ct._$AI(o,this,0)}}disconnected(){}reconnected(){}}/**
 * @license
 * Copyright 2020 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Sf=()=>new Af;class Af{}const or=new WeakMap,zf=Li(class extends Cf{render(e){return et}update(e,[t]){var i;const o=t!==this.Y;return o&&this.Y!==void 0&&this.rt(void 0),(o||this.lt!==this.ct)&&(this.Y=t,this.ht=(i=e.options)==null?void 0:i.host,this.rt(this.ct=e.element)),et}rt(e){if(this.isConnected||(e=void 0),typeof this.Y=="function"){const t=this.ht??globalThis;let o=or.get(t);o===void 0&&(o=new WeakMap,or.set(t,o)),o.get(this.Y)!==void 0&&this.Y.call(this.ht,void 0),o.set(this.Y,e),e!==void 0&&this.Y.call(this.ht,e)}else this.Y.value=e}get lt(){var e,t;return typeof this.Y=="function"?(e=or.get(this.ht??globalThis))==null?void 0:e.get(this.Y):(t=this.Y)==null?void 0:t.value}disconnected(){this.lt===this.ct&&this.rt(void 0)}reconnected(){this.rt(this.ct)}});var Ef=class{constructor(e,t){this.popupRef=Sf(),this.enableSubmenuTimer=-1,this.isConnected=!1,this.isPopupConnected=!1,this.skidding=0,this.submenuOpenDelay=100,this.handleMouseMove=o=>{this.host.style.setProperty("--safe-triangle-cursor-x",`${o.clientX}px`),this.host.style.setProperty("--safe-triangle-cursor-y",`${o.clientY}px`)},this.handleMouseOver=()=>{this.hasSlotController.test("submenu")&&this.enableSubmenu()},this.handleKeyDown=o=>{switch(o.key){case"Escape":case"Tab":this.disableSubmenu();break;case"ArrowLeft":o.target!==this.host&&(o.preventDefault(),o.stopPropagation(),this.host.focus(),this.disableSubmenu());break;case"ArrowRight":case"Enter":case" ":this.handleSubmenuEntry(o);break}},this.handleClick=o=>{var i;o.target===this.host?(o.preventDefault(),o.stopPropagation()):o.target instanceof Element&&(o.target.tagName==="sl-menu-item"||(i=o.target.role)!=null&&i.startsWith("menuitem"))&&this.disableSubmenu()},this.handleFocusOut=o=>{o.relatedTarget&&o.relatedTarget instanceof Element&&this.host.contains(o.relatedTarget)||this.disableSubmenu()},this.handlePopupMouseover=o=>{o.stopPropagation()},this.handlePopupReposition=()=>{const o=this.host.renderRoot.querySelector("slot[name='submenu']"),i=o==null?void 0:o.assignedElements({flatten:!0}).filter(d=>d.localName==="sl-menu")[0],s=getComputedStyle(this.host).direction==="rtl";if(!i)return;const{left:r,top:a,width:n,height:c}=i.getBoundingClientRect();this.host.style.setProperty("--safe-triangle-submenu-start-x",`${s?r+n:r}px`),this.host.style.setProperty("--safe-triangle-submenu-start-y",`${a}px`),this.host.style.setProperty("--safe-triangle-submenu-end-x",`${s?r+n:r}px`),this.host.style.setProperty("--safe-triangle-submenu-end-y",`${a+c}px`)},(this.host=e).addController(this),this.hasSlotController=t}hostConnected(){this.hasSlotController.test("submenu")&&!this.host.disabled&&this.addListeners()}hostDisconnected(){this.removeListeners()}hostUpdated(){this.hasSlotController.test("submenu")&&!this.host.disabled?(this.addListeners(),this.updateSkidding()):this.removeListeners()}addListeners(){this.isConnected||(this.host.addEventListener("mousemove",this.handleMouseMove),this.host.addEventListener("mouseover",this.handleMouseOver),this.host.addEventListener("keydown",this.handleKeyDown),this.host.addEventListener("click",this.handleClick),this.host.addEventListener("focusout",this.handleFocusOut),this.isConnected=!0),this.isPopupConnected||this.popupRef.value&&(this.popupRef.value.addEventListener("mouseover",this.handlePopupMouseover),this.popupRef.value.addEventListener("sl-reposition",this.handlePopupReposition),this.isPopupConnected=!0)}removeListeners(){this.isConnected&&(this.host.removeEventListener("mousemove",this.handleMouseMove),this.host.removeEventListener("mouseover",this.handleMouseOver),this.host.removeEventListener("keydown",this.handleKeyDown),this.host.removeEventListener("click",this.handleClick),this.host.removeEventListener("focusout",this.handleFocusOut),this.isConnected=!1),this.isPopupConnected&&this.popupRef.value&&(this.popupRef.value.removeEventListener("mouseover",this.handlePopupMouseover),this.popupRef.value.removeEventListener("sl-reposition",this.handlePopupReposition),this.isPopupConnected=!1)}handleSubmenuEntry(e){const t=this.host.renderRoot.querySelector("slot[name='submenu']");if(!t){console.error("Cannot activate a submenu if no corresponding menuitem can be found.",this);return}let o=null;for(const i of t.assignedElements())if(o=i.querySelectorAll("sl-menu-item, [role^='menuitem']"),o.length!==0)break;if(!(!o||o.length===0)){o[0].setAttribute("tabindex","0");for(let i=1;i!==o.length;++i)o[i].setAttribute("tabindex","-1");this.popupRef.value&&(e.preventDefault(),e.stopPropagation(),this.popupRef.value.active?o[0]instanceof HTMLElement&&o[0].focus():(this.enableSubmenu(!1),this.host.updateComplete.then(()=>{o[0]instanceof HTMLElement&&o[0].focus()}),this.host.requestUpdate()))}}setSubmenuState(e){this.popupRef.value&&this.popupRef.value.active!==e&&(this.popupRef.value.active=e,this.host.requestUpdate())}enableSubmenu(e=!0){e?(window.clearTimeout(this.enableSubmenuTimer),this.enableSubmenuTimer=window.setTimeout(()=>{this.setSubmenuState(!0)},this.submenuOpenDelay)):this.setSubmenuState(!0)}disableSubmenu(){window.clearTimeout(this.enableSubmenuTimer),this.setSubmenuState(!1)}updateSkidding(){var e;if(!((e=this.host.parentElement)!=null&&e.computedStyleMap))return;const t=this.host.parentElement.computedStyleMap(),i=["padding-top","border-top-width","margin-top"].reduce((s,r)=>{var a;const n=(a=t.get(r))!=null?a:new CSSUnitValue(0,"px"),d=(n instanceof CSSUnitValue?n:new CSSUnitValue(0,"px")).to("px");return s-d.value},0);this.skidding=i}isExpanded(){return this.popupRef.value?this.popupRef.value.active:!1}renderSubmenu(){const e=getComputedStyle(this.host).direction==="rtl";return this.isConnected?x`
      <sl-popup
        ${zf(this.popupRef)}
        placement=${e?"left-start":"right-start"}
        anchor="anchor"
        flip
        flip-fallback-strategy="best-fit"
        skidding="${this.skidding}"
        strategy="fixed"
        auto-size="vertical"
        auto-size-padding="10"
      >
        <slot name="submenu"></slot>
      </sl-popup>
    `:x` <slot name="submenu" hidden></slot> `}},Nt=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.type="normal",this.checked=!1,this.value="",this.loading=!1,this.disabled=!1,this.hasSlotController=new Ot(this,"submenu"),this.submenuController=new Ef(this,this.hasSlotController),this.handleHostClick=e=>{this.disabled&&(e.preventDefault(),e.stopImmediatePropagation())},this.handleMouseOver=e=>{this.focus(),e.stopPropagation()}}connectedCallback(){super.connectedCallback(),this.addEventListener("click",this.handleHostClick),this.addEventListener("mouseover",this.handleMouseOver)}disconnectedCallback(){super.disconnectedCallback(),this.removeEventListener("click",this.handleHostClick),this.removeEventListener("mouseover",this.handleMouseOver)}handleDefaultSlotChange(){const e=this.getTextLabel();if(typeof this.cachedTextLabel>"u"){this.cachedTextLabel=e;return}e!==this.cachedTextLabel&&(this.cachedTextLabel=e,this.emit("slotchange",{bubbles:!0,composed:!1,cancelable:!1}))}handleCheckedChange(){if(this.checked&&this.type!=="checkbox"){this.checked=!1,console.error('The checked attribute can only be used on menu items with type="checkbox"',this);return}this.type==="checkbox"?this.setAttribute("aria-checked",this.checked?"true":"false"):this.removeAttribute("aria-checked")}handleDisabledChange(){this.setAttribute("aria-disabled",this.disabled?"true":"false")}handleTypeChange(){this.type==="checkbox"?(this.setAttribute("role","menuitemcheckbox"),this.setAttribute("aria-checked",this.checked?"true":"false")):(this.setAttribute("role","menuitem"),this.removeAttribute("aria-checked"))}getTextLabel(){return Ou(this.defaultSlot)}isSubmenu(){return this.hasSlotController.test("submenu")}render(){const e=this.localize.dir()==="rtl",t=this.submenuController.isExpanded();return x`
      <div
        id="anchor"
        part="base"
        class=${R({"menu-item":!0,"menu-item--rtl":e,"menu-item--checked":this.checked,"menu-item--disabled":this.disabled,"menu-item--loading":this.loading,"menu-item--has-submenu":this.isSubmenu(),"menu-item--submenu-expanded":t})}
        ?aria-haspopup="${this.isSubmenu()}"
        ?aria-expanded="${!!t}"
      >
        <span part="checked-icon" class="menu-item__check">
          <sl-icon name="check" library="system" aria-hidden="true"></sl-icon>
        </span>

        <slot name="prefix" part="prefix" class="menu-item__prefix"></slot>

        <slot part="label" class="menu-item__label" @slotchange=${this.handleDefaultSlotChange}></slot>

        <slot name="suffix" part="suffix" class="menu-item__suffix"></slot>

        <span part="submenu-icon" class="menu-item__chevron">
          <sl-icon name=${e?"chevron-left":"chevron-right"} library="system" aria-hidden="true"></sl-icon>
        </span>

        ${this.submenuController.renderSubmenu()}
        ${this.loading?x` <sl-spinner part="spinner" exportparts="base:spinner__base"></sl-spinner> `:""}
      </div>
    `}};Nt.styles=[D,xf];Nt.dependencies={"sl-icon":J,"sl-popup":Y,"sl-spinner":Di};l([S("slot:not([name])")],Nt.prototype,"defaultSlot",2);l([S(".menu-item")],Nt.prototype,"menuItem",2);l([p()],Nt.prototype,"type",2);l([p({type:Boolean,reflect:!0})],Nt.prototype,"checked",2);l([p()],Nt.prototype,"value",2);l([p({type:Boolean,reflect:!0})],Nt.prototype,"loading",2);l([p({type:Boolean,reflect:!0})],Nt.prototype,"disabled",2);l([C("checked")],Nt.prototype,"handleCheckedChange",1);l([C("disabled")],Nt.prototype,"handleDisabledChange",1);l([C("type")],Nt.prototype,"handleTypeChange",1);Nt.define("sl-menu-item");var Tf=O`
  :host {
    --divider-width: 2px;
    --handle-size: 2.5rem;

    display: inline-block;
    position: relative;
  }

  .image-comparer {
    max-width: 100%;
    max-height: 100%;
    overflow: hidden;
  }

  .image-comparer__before,
  .image-comparer__after {
    display: block;
    pointer-events: none;
  }

  .image-comparer__before::slotted(img),
  .image-comparer__after::slotted(img),
  .image-comparer__before::slotted(svg),
  .image-comparer__after::slotted(svg) {
    display: block;
    max-width: 100% !important;
    height: auto;
  }

  .image-comparer__after {
    position: absolute;
    top: 0;
    left: 0;
    height: 100%;
    width: 100%;
  }

  .image-comparer__divider {
    display: flex;
    align-items: center;
    justify-content: center;
    position: absolute;
    top: 0;
    width: var(--divider-width);
    height: 100%;
    background-color: var(--sl-color-neutral-0);
    translate: calc(var(--divider-width) / -2);
    cursor: ew-resize;
  }

  .image-comparer__handle {
    display: flex;
    align-items: center;
    justify-content: center;
    position: absolute;
    top: calc(50% - (var(--handle-size) / 2));
    width: var(--handle-size);
    height: var(--handle-size);
    background-color: var(--sl-color-neutral-0);
    border-radius: var(--sl-border-radius-circle);
    font-size: calc(var(--handle-size) * 0.5);
    color: var(--sl-color-neutral-700);
    cursor: inherit;
    z-index: 10;
  }

  .image-comparer__handle:focus-visible {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }
`,go=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.position=50}handleDrag(e){const{width:t}=this.base.getBoundingClientRect(),o=this.localize.dir()==="rtl";e.preventDefault(),ui(this.base,{onMove:i=>{this.position=parseFloat(ht(i/t*100,0,100).toFixed(2)),o&&(this.position=100-this.position)},initialEvent:e})}handleKeyDown(e){const t=this.localize.dir()==="ltr",o=this.localize.dir()==="rtl";if(["ArrowLeft","ArrowRight","Home","End"].includes(e.key)){const i=e.shiftKey?10:1;let s=this.position;e.preventDefault(),(t&&e.key==="ArrowLeft"||o&&e.key==="ArrowRight")&&(s-=i),(t&&e.key==="ArrowRight"||o&&e.key==="ArrowLeft")&&(s+=i),e.key==="Home"&&(s=0),e.key==="End"&&(s=100),s=ht(s,0,100),this.position=s}}handlePositionChange(){this.emit("sl-change")}render(){const e=this.localize.dir()==="rtl";return x`
      <div
        part="base"
        id="image-comparer"
        class=${R({"image-comparer":!0,"image-comparer--rtl":e})}
        @keydown=${this.handleKeyDown}
      >
        <div class="image-comparer__image">
          <div part="before" class="image-comparer__before">
            <slot name="before"></slot>
          </div>

          <div
            part="after"
            class="image-comparer__after"
            style=${Rt({clipPath:e?`inset(0 0 0 ${100-this.position}%)`:`inset(0 ${100-this.position}% 0 0)`})}
          >
            <slot name="after"></slot>
          </div>
        </div>

        <div
          part="divider"
          class="image-comparer__divider"
          style=${Rt({left:e?`${100-this.position}%`:`${this.position}%`})}
          @mousedown=${this.handleDrag}
          @touchstart=${this.handleDrag}
        >
          <div
            part="handle"
            class="image-comparer__handle"
            role="scrollbar"
            aria-valuenow=${this.position}
            aria-valuemin="0"
            aria-valuemax="100"
            aria-controls="image-comparer"
            tabindex="0"
          >
            <slot name="handle">
              <sl-icon library="system" name="grip-vertical"></sl-icon>
            </slot>
          </div>
        </div>
      </div>
    `}};go.styles=[D,Tf];go.scopedElement={"sl-icon":J};l([S(".image-comparer")],go.prototype,"base",2);l([S(".image-comparer__handle")],go.prototype,"handle",2);l([p({type:Number,reflect:!0})],go.prototype,"position",2);l([C("position",{waitUntilFirstUpdate:!0})],go.prototype,"handlePositionChange",1);go.define("sl-image-comparer");var Pf=O`
  :host {
    display: block;
  }
`,ir=new Map;function Of(e,t="cors"){const o=ir.get(e);if(o!==void 0)return Promise.resolve(o);const i=fetch(e,{mode:t}).then(async s=>{const r={ok:s.ok,status:s.status,html:await s.text()};return ir.set(e,r),r});return ir.set(e,i),i}var No=class extends T{constructor(){super(...arguments),this.mode="cors",this.allowScripts=!1}executeScript(e){const t=document.createElement("script");[...e.attributes].forEach(o=>t.setAttribute(o.name,o.value)),t.textContent=e.textContent,e.parentNode.replaceChild(t,e)}async handleSrcChange(){try{const e=this.src,t=await Of(e,this.mode);if(e!==this.src)return;if(!t.ok){this.emit("sl-error",{detail:{status:t.status}});return}this.innerHTML=t.html,this.allowScripts&&[...this.querySelectorAll("script")].forEach(o=>this.executeScript(o)),this.emit("sl-load")}catch{this.emit("sl-error",{detail:{status:-1}})}}render(){return x`<slot></slot>`}};No.styles=[D,Pf];l([p()],No.prototype,"src",2);l([p()],No.prototype,"mode",2);l([p({attribute:"allow-scripts",type:Boolean})],No.prototype,"allowScripts",2);l([C("src")],No.prototype,"handleSrcChange",1);No.define("sl-include");J.define("sl-icon");gt.define("sl-icon-button");var Is=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.value=0,this.unit="byte",this.display="short"}render(){if(isNaN(this.value))return"";const e=["","kilo","mega","giga","tera"],t=["","kilo","mega","giga","tera","peta"],o=this.unit==="bit"?e:t,i=Math.max(0,Math.min(Math.floor(Math.log10(this.value)/3),o.length-1)),s=o[i]+this.unit,r=parseFloat((this.value/Math.pow(1e3,i)).toPrecision(3));return this.localize.number(r,{style:"unit",unit:s,unitDisplay:this.display})}};l([p({type:Number})],Is.prototype,"value",2);l([p()],Is.prototype,"unit",2);l([p()],Is.prototype,"display",2);Is.define("sl-format-bytes");var Ht=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.date=new Date,this.hourFormat="auto"}render(){const e=new Date(this.date),t=this.hourFormat==="auto"?void 0:this.hourFormat==="12";if(!isNaN(e.getMilliseconds()))return x`
      <time datetime=${e.toISOString()}>
        ${this.localize.date(e,{weekday:this.weekday,era:this.era,year:this.year,month:this.month,day:this.day,hour:this.hour,minute:this.minute,second:this.second,timeZoneName:this.timeZoneName,timeZone:this.timeZone,hour12:t})}
      </time>
    `}};l([p()],Ht.prototype,"date",2);l([p()],Ht.prototype,"weekday",2);l([p()],Ht.prototype,"era",2);l([p()],Ht.prototype,"year",2);l([p()],Ht.prototype,"month",2);l([p()],Ht.prototype,"day",2);l([p()],Ht.prototype,"hour",2);l([p()],Ht.prototype,"minute",2);l([p()],Ht.prototype,"second",2);l([p({attribute:"time-zone-name"})],Ht.prototype,"timeZoneName",2);l([p({attribute:"time-zone"})],Ht.prototype,"timeZone",2);l([p({attribute:"hour-format"})],Ht.prototype,"hourFormat",2);Ht.define("sl-format-date");var se=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.value=0,this.type="decimal",this.noGrouping=!1,this.currency="USD",this.currencyDisplay="symbol"}render(){return isNaN(this.value)?"":this.localize.number(this.value,{style:this.type,currency:this.currency,currencyDisplay:this.currencyDisplay,useGrouping:!this.noGrouping,minimumIntegerDigits:this.minimumIntegerDigits,minimumFractionDigits:this.minimumFractionDigits,maximumFractionDigits:this.maximumFractionDigits,minimumSignificantDigits:this.minimumSignificantDigits,maximumSignificantDigits:this.maximumSignificantDigits})}};l([p({type:Number})],se.prototype,"value",2);l([p()],se.prototype,"type",2);l([p({attribute:"no-grouping",type:Boolean})],se.prototype,"noGrouping",2);l([p()],se.prototype,"currency",2);l([p({attribute:"currency-display"})],se.prototype,"currencyDisplay",2);l([p({attribute:"minimum-integer-digits",type:Number})],se.prototype,"minimumIntegerDigits",2);l([p({attribute:"minimum-fraction-digits",type:Number})],se.prototype,"minimumFractionDigits",2);l([p({attribute:"maximum-fraction-digits",type:Number})],se.prototype,"maximumFractionDigits",2);l([p({attribute:"minimum-significant-digits",type:Number})],se.prototype,"minimumSignificantDigits",2);l([p({attribute:"maximum-significant-digits",type:Number})],se.prototype,"maximumSignificantDigits",2);se.define("sl-format-number");var Lf=O`
  :host {
    --color: var(--sl-panel-border-color);
    --width: var(--sl-panel-border-width);
    --spacing: var(--sl-spacing-medium);
  }

  :host(:not([vertical])) {
    display: block;
    border-top: solid var(--width) var(--color);
    margin: var(--spacing) 0;
  }

  :host([vertical]) {
    display: inline-block;
    height: 100%;
    border-left: solid var(--width) var(--color);
    margin: 0 var(--spacing);
  }
`,Bs=class extends T{constructor(){super(...arguments),this.vertical=!1}connectedCallback(){super.connectedCallback(),this.setAttribute("role","separator")}handleVerticalChange(){this.setAttribute("aria-orientation",this.vertical?"vertical":"horizontal")}};Bs.styles=[D,Lf];l([p({type:Boolean,reflect:!0})],Bs.prototype,"vertical",2);l([C("vertical")],Bs.prototype,"handleVerticalChange",1);Bs.define("sl-divider");var Rf=O`
  :host {
    --size: 25rem;
    --header-spacing: var(--sl-spacing-large);
    --body-spacing: var(--sl-spacing-large);
    --footer-spacing: var(--sl-spacing-large);

    display: contents;
  }

  .drawer {
    top: 0;
    inset-inline-start: 0;
    width: 100%;
    height: 100%;
    pointer-events: none;
    overflow: hidden;
  }

  .drawer--contained {
    position: absolute;
    z-index: initial;
  }

  .drawer--fixed {
    position: fixed;
    z-index: var(--sl-z-index-drawer);
  }

  .drawer__panel {
    position: absolute;
    display: flex;
    flex-direction: column;
    z-index: 2;
    max-width: 100%;
    max-height: 100%;
    background-color: var(--sl-panel-background-color);
    box-shadow: var(--sl-shadow-x-large);
    overflow: auto;
    pointer-events: all;
  }

  .drawer__panel:focus {
    outline: none;
  }

  .drawer--top .drawer__panel {
    top: 0;
    inset-inline-end: auto;
    bottom: auto;
    inset-inline-start: 0;
    width: 100%;
    height: var(--size);
  }

  .drawer--end .drawer__panel {
    top: 0;
    inset-inline-end: 0;
    bottom: auto;
    inset-inline-start: auto;
    width: var(--size);
    height: 100%;
  }

  .drawer--bottom .drawer__panel {
    top: auto;
    inset-inline-end: auto;
    bottom: 0;
    inset-inline-start: 0;
    width: 100%;
    height: var(--size);
  }

  .drawer--start .drawer__panel {
    top: 0;
    inset-inline-end: auto;
    bottom: auto;
    inset-inline-start: 0;
    width: var(--size);
    height: 100%;
  }

  .drawer__header {
    display: flex;
  }

  .drawer__title {
    flex: 1 1 auto;
    font: inherit;
    font-size: var(--sl-font-size-large);
    line-height: var(--sl-line-height-dense);
    padding: var(--header-spacing);
    margin: 0;
  }

  .drawer__header-actions {
    flex-shrink: 0;
    display: flex;
    flex-wrap: wrap;
    justify-content: end;
    gap: var(--sl-spacing-2x-small);
    padding: 0 var(--header-spacing);
  }

  .drawer__header-actions sl-icon-button,
  .drawer__header-actions ::slotted(sl-icon-button) {
    flex: 0 0 auto;
    display: flex;
    align-items: center;
    font-size: var(--sl-font-size-medium);
  }

  .drawer__body {
    flex: 1 1 auto;
    display: block;
    padding: var(--body-spacing);
    overflow: auto;
    -webkit-overflow-scrolling: touch;
  }

  .drawer__footer {
    text-align: right;
    padding: var(--footer-spacing);
  }

  .drawer__footer ::slotted(sl-button:not(:last-of-type)) {
    margin-inline-end: var(--sl-spacing-x-small);
  }

  .drawer:not(.drawer--has-footer) .drawer__footer {
    display: none;
  }

  .drawer__overlay {
    display: block;
    position: fixed;
    top: 0;
    right: 0;
    bottom: 0;
    left: 0;
    background-color: var(--sl-overlay-background-color);
    pointer-events: all;
  }

  .drawer--contained .drawer__overlay {
    display: none;
  }

  @media (forced-colors: active) {
    .drawer__panel {
      border: solid 1px var(--sl-color-neutral-0);
    }
  }
`;function*ga(e=document.activeElement){e!=null&&(yield e,"shadowRoot"in e&&e.shadowRoot&&e.shadowRoot.mode!=="closed"&&(yield*Au(ga(e.shadowRoot.activeElement))))}function ec(){return[...ga()].pop()}var _n=new WeakMap;function oc(e){let t=_n.get(e);return t||(t=window.getComputedStyle(e,null),_n.set(e,t)),t}function Df(e){if(typeof e.checkVisibility=="function")return e.checkVisibility({checkOpacity:!1,checkVisibilityCSS:!0});const t=oc(e);return t.visibility!=="hidden"&&t.display!=="none"}function Mf(e){const t=oc(e),{overflowY:o,overflowX:i}=t;return o==="scroll"||i==="scroll"?!0:o!=="auto"||i!=="auto"?!1:e.scrollHeight>e.clientHeight&&o==="auto"||e.scrollWidth>e.clientWidth&&i==="auto"}function If(e){const t=e.tagName.toLowerCase(),o=Number(e.getAttribute("tabindex"));if(e.hasAttribute("tabindex")&&(isNaN(o)||o<=-1)||e.hasAttribute("disabled")||e.closest("[inert]"))return!1;if(t==="input"&&e.getAttribute("type")==="radio"){const r=e.getRootNode(),a=`input[type='radio'][name="${e.getAttribute("name")}"]`,n=r.querySelector(`${a}:checked`);return n?n===e:r.querySelector(a)===e}return Df(e)?(t==="audio"||t==="video")&&e.hasAttribute("controls")||e.hasAttribute("tabindex")||e.hasAttribute("contenteditable")&&e.getAttribute("contenteditable")!=="false"||["button","input","select","textarea","a","audio","video","summary","iframe"].includes(t)?!0:Mf(e):!1}function Bf(e){var t,o;const i=Sr(e),s=(t=i[0])!=null?t:null,r=(o=i[i.length-1])!=null?o:null;return{start:s,end:r}}function Ff(e,t){var o;return((o=e.getRootNode({composed:!0}))==null?void 0:o.host)!==t}function Sr(e){const t=new WeakMap,o=[];function i(s){if(s instanceof Element){if(s.hasAttribute("inert")||s.closest("[inert]")||t.has(s))return;t.set(s,!0),!o.includes(s)&&If(s)&&o.push(s),s instanceof HTMLSlotElement&&Ff(s,e)&&s.assignedElements({flatten:!0}).forEach(r=>{i(r)}),s.shadowRoot!==null&&s.shadowRoot.mode==="open"&&i(s.shadowRoot)}for(const r of s.children)i(r)}return i(e),o.sort((s,r)=>{const a=Number(s.getAttribute("tabindex"))||0;return(Number(r.getAttribute("tabindex"))||0)-a})}var ti=[],ic=class{constructor(e){this.tabDirection="forward",this.handleFocusIn=()=>{this.isActive()&&this.checkFocus()},this.handleKeyDown=t=>{var o;if(t.key!=="Tab"||this.isExternalActivated||!this.isActive())return;const i=ec();if(this.previousFocus=i,this.previousFocus&&this.possiblyHasTabbableChildren(this.previousFocus))return;t.shiftKey?this.tabDirection="backward":this.tabDirection="forward";const s=Sr(this.element);let r=s.findIndex(n=>n===i);this.previousFocus=this.currentFocus;const a=this.tabDirection==="forward"?1:-1;for(;;){r+a>=s.length?r=0:r+a<0?r=s.length-1:r+=a,this.previousFocus=this.currentFocus;const n=s[r];if(this.tabDirection==="backward"&&this.previousFocus&&this.possiblyHasTabbableChildren(this.previousFocus)||n&&this.possiblyHasTabbableChildren(n))return;t.preventDefault(),this.currentFocus=n,(o=this.currentFocus)==null||o.focus({preventScroll:!1});const c=[...ga()];if(c.includes(this.currentFocus)||!c.includes(this.previousFocus))break}setTimeout(()=>this.checkFocus())},this.handleKeyUp=()=>{this.tabDirection="forward"},this.element=e,this.elementsWithTabbableControls=["iframe"]}activate(){ti.push(this.element),document.addEventListener("focusin",this.handleFocusIn),document.addEventListener("keydown",this.handleKeyDown),document.addEventListener("keyup",this.handleKeyUp)}deactivate(){ti=ti.filter(e=>e!==this.element),this.currentFocus=null,document.removeEventListener("focusin",this.handleFocusIn),document.removeEventListener("keydown",this.handleKeyDown),document.removeEventListener("keyup",this.handleKeyUp)}isActive(){return ti[ti.length-1]===this.element}activateExternal(){this.isExternalActivated=!0}deactivateExternal(){this.isExternalActivated=!1}checkFocus(){if(this.isActive()&&!this.isExternalActivated){const e=Sr(this.element);if(!this.element.matches(":focus-within")){const t=e[0],o=e[e.length-1],i=this.tabDirection==="forward"?t:o;typeof(i==null?void 0:i.focus)=="function"&&(this.currentFocus=i,i.focus({preventScroll:!1}))}}}possiblyHasTabbableChildren(e){return this.elementsWithTabbableControls.includes(e.tagName.toLowerCase())||e.hasAttribute("controls")}},ba=e=>{var t;const{activeElement:o}=document;o&&e.contains(o)&&((t=document.activeElement)==null||t.blur())};function $n(e){return e.charAt(0).toUpperCase()+e.slice(1)}var jt=class extends T{constructor(){super(...arguments),this.hasSlotController=new Ot(this,"footer"),this.localize=new j(this),this.modal=new ic(this),this.open=!1,this.label="",this.placement="end",this.contained=!1,this.noHeader=!1,this.handleDocumentKeyDown=e=>{this.contained||e.key==="Escape"&&this.modal.isActive()&&this.open&&(e.stopImmediatePropagation(),this.requestClose("keyboard"))}}firstUpdated(){this.drawer.hidden=!this.open,this.open&&(this.addOpenListeners(),this.contained||(this.modal.activate(),di(this)))}disconnectedCallback(){super.disconnectedCallback(),hi(this),this.removeOpenListeners()}requestClose(e){if(this.emit("sl-request-close",{cancelable:!0,detail:{source:e}}).defaultPrevented){const o=ot(this,"drawer.denyClose",{dir:this.localize.dir()});at(this.panel,o.keyframes,o.options);return}this.hide()}addOpenListeners(){var e;"CloseWatcher"in window?((e=this.closeWatcher)==null||e.destroy(),this.contained||(this.closeWatcher=new CloseWatcher,this.closeWatcher.onclose=()=>this.requestClose("keyboard"))):document.addEventListener("keydown",this.handleDocumentKeyDown)}removeOpenListeners(){var e;document.removeEventListener("keydown",this.handleDocumentKeyDown),(e=this.closeWatcher)==null||e.destroy()}async handleOpenChange(){if(this.open){this.emit("sl-show"),this.addOpenListeners(),this.originalTrigger=document.activeElement,this.contained||(this.modal.activate(),di(this));const e=this.querySelector("[autofocus]");e&&e.removeAttribute("autofocus"),await Promise.all([ut(this.drawer),ut(this.overlay)]),this.drawer.hidden=!1,requestAnimationFrame(()=>{this.emit("sl-initial-focus",{cancelable:!0}).defaultPrevented||(e?e.focus({preventScroll:!0}):this.panel.focus({preventScroll:!0})),e&&e.setAttribute("autofocus","")});const t=ot(this,`drawer.show${$n(this.placement)}`,{dir:this.localize.dir()}),o=ot(this,"drawer.overlay.show",{dir:this.localize.dir()});await Promise.all([at(this.panel,t.keyframes,t.options),at(this.overlay,o.keyframes,o.options)]),this.emit("sl-after-show")}else{ba(this),this.emit("sl-hide"),this.removeOpenListeners(),this.contained||(this.modal.deactivate(),hi(this)),await Promise.all([ut(this.drawer),ut(this.overlay)]);const e=ot(this,`drawer.hide${$n(this.placement)}`,{dir:this.localize.dir()}),t=ot(this,"drawer.overlay.hide",{dir:this.localize.dir()});await Promise.all([at(this.overlay,t.keyframes,t.options).then(()=>{this.overlay.hidden=!0}),at(this.panel,e.keyframes,e.options).then(()=>{this.panel.hidden=!0})]),this.drawer.hidden=!0,this.overlay.hidden=!1,this.panel.hidden=!1;const o=this.originalTrigger;typeof(o==null?void 0:o.focus)=="function"&&setTimeout(()=>o.focus()),this.emit("sl-after-hide")}}handleNoModalChange(){this.open&&!this.contained&&(this.modal.activate(),di(this)),this.open&&this.contained&&(this.modal.deactivate(),hi(this))}async show(){if(!this.open)return this.open=!0,Pt(this,"sl-after-show")}async hide(){if(this.open)return this.open=!1,Pt(this,"sl-after-hide")}render(){return x`
      <div
        part="base"
        class=${R({drawer:!0,"drawer--open":this.open,"drawer--top":this.placement==="top","drawer--end":this.placement==="end","drawer--bottom":this.placement==="bottom","drawer--start":this.placement==="start","drawer--contained":this.contained,"drawer--fixed":!this.contained,"drawer--rtl":this.localize.dir()==="rtl","drawer--has-footer":this.hasSlotController.test("footer")})}
      >
        <div part="overlay" class="drawer__overlay" @click=${()=>this.requestClose("overlay")} tabindex="-1"></div>

        <div
          part="panel"
          class="drawer__panel"
          role="dialog"
          aria-modal="true"
          aria-hidden=${this.open?"false":"true"}
          aria-label=${E(this.noHeader?this.label:void 0)}
          aria-labelledby=${E(this.noHeader?void 0:"title")}
          tabindex="0"
        >
          ${this.noHeader?"":x`
                <header part="header" class="drawer__header">
                  <h2 part="title" class="drawer__title" id="title">
                    <!-- If there's no label, use an invisible character to prevent the header from collapsing -->
                    <slot name="label"> ${this.label.length>0?this.label:"\uFEFF"} </slot>
                  </h2>
                  <div part="header-actions" class="drawer__header-actions">
                    <slot name="header-actions"></slot>
                    <sl-icon-button
                      part="close-button"
                      exportparts="base:close-button__base"
                      class="drawer__close"
                      name="x-lg"
                      label=${this.localize.term("close")}
                      library="system"
                      @click=${()=>this.requestClose("close-button")}
                    ></sl-icon-button>
                  </div>
                </header>
              `}

          <slot part="body" class="drawer__body"></slot>

          <footer part="footer" class="drawer__footer">
            <slot name="footer"></slot>
          </footer>
        </div>
      </div>
    `}};jt.styles=[D,Rf];jt.dependencies={"sl-icon-button":gt};l([S(".drawer")],jt.prototype,"drawer",2);l([S(".drawer__panel")],jt.prototype,"panel",2);l([S(".drawer__overlay")],jt.prototype,"overlay",2);l([p({type:Boolean,reflect:!0})],jt.prototype,"open",2);l([p({reflect:!0})],jt.prototype,"label",2);l([p({reflect:!0})],jt.prototype,"placement",2);l([p({type:Boolean,reflect:!0})],jt.prototype,"contained",2);l([p({attribute:"no-header",type:Boolean,reflect:!0})],jt.prototype,"noHeader",2);l([C("open",{waitUntilFirstUpdate:!0})],jt.prototype,"handleOpenChange",1);l([C("contained",{waitUntilFirstUpdate:!0})],jt.prototype,"handleNoModalChange",1);K("drawer.showTop",{keyframes:[{opacity:0,translate:"0 -100%"},{opacity:1,translate:"0 0"}],options:{duration:250,easing:"ease"}});K("drawer.hideTop",{keyframes:[{opacity:1,translate:"0 0"},{opacity:0,translate:"0 -100%"}],options:{duration:250,easing:"ease"}});K("drawer.showEnd",{keyframes:[{opacity:0,translate:"100%"},{opacity:1,translate:"0"}],rtlKeyframes:[{opacity:0,translate:"-100%"},{opacity:1,translate:"0"}],options:{duration:250,easing:"ease"}});K("drawer.hideEnd",{keyframes:[{opacity:1,translate:"0"},{opacity:0,translate:"100%"}],rtlKeyframes:[{opacity:1,translate:"0"},{opacity:0,translate:"-100%"}],options:{duration:250,easing:"ease"}});K("drawer.showBottom",{keyframes:[{opacity:0,translate:"0 100%"},{opacity:1,translate:"0 0"}],options:{duration:250,easing:"ease"}});K("drawer.hideBottom",{keyframes:[{opacity:1,translate:"0 0"},{opacity:0,translate:"0 100%"}],options:{duration:250,easing:"ease"}});K("drawer.showStart",{keyframes:[{opacity:0,translate:"-100%"},{opacity:1,translate:"0"}],rtlKeyframes:[{opacity:0,translate:"100%"},{opacity:1,translate:"0"}],options:{duration:250,easing:"ease"}});K("drawer.hideStart",{keyframes:[{opacity:1,translate:"0"},{opacity:0,translate:"-100%"}],rtlKeyframes:[{opacity:1,translate:"0"},{opacity:0,translate:"100%"}],options:{duration:250,easing:"ease"}});K("drawer.denyClose",{keyframes:[{scale:1},{scale:1.01},{scale:1}],options:{duration:250}});K("drawer.overlay.show",{keyframes:[{opacity:0},{opacity:1}],options:{duration:250}});K("drawer.overlay.hide",{keyframes:[{opacity:1},{opacity:0}],options:{duration:250}});jt.define("sl-drawer");var Vf=O`
  :host {
    display: inline-block;
  }

  .dropdown::part(popup) {
    z-index: var(--sl-z-index-dropdown);
  }

  .dropdown[data-current-placement^='top']::part(popup) {
    transform-origin: bottom;
  }

  .dropdown[data-current-placement^='bottom']::part(popup) {
    transform-origin: top;
  }

  .dropdown[data-current-placement^='left']::part(popup) {
    transform-origin: right;
  }

  .dropdown[data-current-placement^='right']::part(popup) {
    transform-origin: left;
  }

  .dropdown__trigger {
    display: block;
  }

  .dropdown__panel {
    font-family: var(--sl-font-sans);
    font-size: var(--sl-font-size-medium);
    font-weight: var(--sl-font-weight-normal);
    box-shadow: var(--sl-shadow-large);
    border-radius: var(--sl-border-radius-medium);
    pointer-events: none;
  }

  .dropdown--open .dropdown__panel {
    display: block;
    pointer-events: all;
  }

  /* When users slot a menu, make sure it conforms to the popup's auto-size */
  ::slotted(sl-menu) {
    max-width: var(--auto-size-available-width) !important;
    max-height: var(--auto-size-available-height) !important;
  }
`,St=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.open=!1,this.placement="bottom-start",this.disabled=!1,this.stayOpenOnSelect=!1,this.distance=0,this.skidding=0,this.hoist=!1,this.sync=void 0,this.handleKeyDown=e=>{this.open&&e.key==="Escape"&&(e.stopPropagation(),this.hide(),this.focusOnTrigger())},this.handleDocumentKeyDown=e=>{var t;if(e.key==="Escape"&&this.open&&!this.closeWatcher){e.stopPropagation(),this.focusOnTrigger(),this.hide();return}if(e.key==="Tab"){if(this.open&&((t=document.activeElement)==null?void 0:t.tagName.toLowerCase())==="sl-menu-item"){e.preventDefault(),this.hide(),this.focusOnTrigger();return}const o=(i,s)=>{if(!i)return null;const r=i.closest(s);if(r)return r;const a=i.getRootNode();return a instanceof ShadowRoot?o(a.host,s):null};setTimeout(()=>{var i;const s=((i=this.containingElement)==null?void 0:i.getRootNode())instanceof ShadowRoot?ec():document.activeElement;(!this.containingElement||o(s,this.containingElement.tagName.toLowerCase())!==this.containingElement)&&this.hide()})}},this.handleDocumentMouseDown=e=>{const t=e.composedPath();this.containingElement&&!t.includes(this.containingElement)&&this.hide()},this.handlePanelSelect=e=>{const t=e.target;!this.stayOpenOnSelect&&t.tagName.toLowerCase()==="sl-menu"&&(this.hide(),this.focusOnTrigger())}}connectedCallback(){super.connectedCallback(),this.containingElement||(this.containingElement=this)}firstUpdated(){this.panel.hidden=!this.open,this.open&&(this.addOpenListeners(),this.popup.active=!0)}disconnectedCallback(){super.disconnectedCallback(),this.removeOpenListeners(),this.hide()}focusOnTrigger(){const e=this.trigger.assignedElements({flatten:!0})[0];typeof(e==null?void 0:e.focus)=="function"&&e.focus()}getMenu(){return this.panel.assignedElements({flatten:!0}).find(e=>e.tagName.toLowerCase()==="sl-menu")}handleTriggerClick(){this.open?this.hide():(this.show(),this.focusOnTrigger())}async handleTriggerKeyDown(e){if([" ","Enter"].includes(e.key)){e.preventDefault(),this.handleTriggerClick();return}const t=this.getMenu();if(t){const o=t.getAllItems(),i=o[0],s=o[o.length-1];["ArrowDown","ArrowUp","Home","End"].includes(e.key)&&(e.preventDefault(),this.open||(this.show(),await this.updateComplete),o.length>0&&this.updateComplete.then(()=>{(e.key==="ArrowDown"||e.key==="Home")&&(t.setCurrentItem(i),i.focus()),(e.key==="ArrowUp"||e.key==="End")&&(t.setCurrentItem(s),s.focus())}))}}handleTriggerKeyUp(e){e.key===" "&&e.preventDefault()}handleTriggerSlotChange(){this.updateAccessibleTrigger()}updateAccessibleTrigger(){const t=this.trigger.assignedElements({flatten:!0}).find(i=>Bf(i).start);let o;if(t){switch(t.tagName.toLowerCase()){case"sl-button":case"sl-icon-button":o=t.button;break;default:o=t}o.setAttribute("aria-haspopup","true"),o.setAttribute("aria-expanded",this.open?"true":"false")}}async show(){if(!this.open)return this.open=!0,Pt(this,"sl-after-show")}async hide(){if(this.open)return this.open=!1,Pt(this,"sl-after-hide")}reposition(){this.popup.reposition()}addOpenListeners(){var e;this.panel.addEventListener("sl-select",this.handlePanelSelect),"CloseWatcher"in window?((e=this.closeWatcher)==null||e.destroy(),this.closeWatcher=new CloseWatcher,this.closeWatcher.onclose=()=>{this.hide(),this.focusOnTrigger()}):this.panel.addEventListener("keydown",this.handleKeyDown),document.addEventListener("keydown",this.handleDocumentKeyDown),document.addEventListener("mousedown",this.handleDocumentMouseDown)}removeOpenListeners(){var e;this.panel&&(this.panel.removeEventListener("sl-select",this.handlePanelSelect),this.panel.removeEventListener("keydown",this.handleKeyDown)),document.removeEventListener("keydown",this.handleDocumentKeyDown),document.removeEventListener("mousedown",this.handleDocumentMouseDown),(e=this.closeWatcher)==null||e.destroy()}async handleOpenChange(){if(this.disabled){this.open=!1;return}if(this.updateAccessibleTrigger(),this.open){this.emit("sl-show"),this.addOpenListeners(),await ut(this),this.panel.hidden=!1,this.popup.active=!0;const{keyframes:e,options:t}=ot(this,"dropdown.show",{dir:this.localize.dir()});await at(this.popup.popup,e,t),this.emit("sl-after-show")}else{this.emit("sl-hide"),this.removeOpenListeners(),await ut(this);const{keyframes:e,options:t}=ot(this,"dropdown.hide",{dir:this.localize.dir()});await at(this.popup.popup,e,t),this.panel.hidden=!0,this.popup.active=!1,this.emit("sl-after-hide")}}render(){return x`
      <sl-popup
        part="base"
        exportparts="popup:base__popup"
        id="dropdown"
        placement=${this.placement}
        distance=${this.distance}
        skidding=${this.skidding}
        strategy=${this.hoist?"fixed":"absolute"}
        flip
        shift
        auto-size="vertical"
        auto-size-padding="10"
        sync=${E(this.sync?this.sync:void 0)}
        class=${R({dropdown:!0,"dropdown--open":this.open})}
      >
        <slot
          name="trigger"
          slot="anchor"
          part="trigger"
          class="dropdown__trigger"
          @click=${this.handleTriggerClick}
          @keydown=${this.handleTriggerKeyDown}
          @keyup=${this.handleTriggerKeyUp}
          @slotchange=${this.handleTriggerSlotChange}
        ></slot>

        <div aria-hidden=${this.open?"false":"true"} aria-labelledby="dropdown">
          <slot part="panel" class="dropdown__panel"></slot>
        </div>
      </sl-popup>
    `}};St.styles=[D,Vf];St.dependencies={"sl-popup":Y};l([S(".dropdown")],St.prototype,"popup",2);l([S(".dropdown__trigger")],St.prototype,"trigger",2);l([S(".dropdown__panel")],St.prototype,"panel",2);l([p({type:Boolean,reflect:!0})],St.prototype,"open",2);l([p({reflect:!0})],St.prototype,"placement",2);l([p({type:Boolean,reflect:!0})],St.prototype,"disabled",2);l([p({attribute:"stay-open-on-select",type:Boolean,reflect:!0})],St.prototype,"stayOpenOnSelect",2);l([p({attribute:!1})],St.prototype,"containingElement",2);l([p({type:Number})],St.prototype,"distance",2);l([p({type:Number})],St.prototype,"skidding",2);l([p({type:Boolean})],St.prototype,"hoist",2);l([p({reflect:!0})],St.prototype,"sync",2);l([C("open",{waitUntilFirstUpdate:!0})],St.prototype,"handleOpenChange",1);K("dropdown.show",{keyframes:[{opacity:0,scale:.9},{opacity:1,scale:1}],options:{duration:100,easing:"ease"}});K("dropdown.hide",{keyframes:[{opacity:1,scale:1},{opacity:0,scale:.9}],options:{duration:100,easing:"ease"}});St.define("sl-dropdown");var Uf=O`
  :host {
    --error-color: var(--sl-color-danger-600);
    --success-color: var(--sl-color-success-600);

    display: inline-block;
  }

  .copy-button__button {
    flex: 0 0 auto;
    display: flex;
    align-items: center;
    background: none;
    border: none;
    border-radius: var(--sl-border-radius-medium);
    font-size: inherit;
    color: inherit;
    padding: var(--sl-spacing-x-small);
    cursor: pointer;
    transition: var(--sl-transition-x-fast) color;
  }

  .copy-button--success .copy-button__button {
    color: var(--success-color);
  }

  .copy-button--error .copy-button__button {
    color: var(--error-color);
  }

  .copy-button__button:focus-visible {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  .copy-button__button[disabled] {
    opacity: 0.5;
    cursor: not-allowed !important;
  }

  slot {
    display: inline-flex;
  }
`,wt=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.isCopying=!1,this.status="rest",this.value="",this.from="",this.disabled=!1,this.copyLabel="",this.successLabel="",this.errorLabel="",this.feedbackDuration=1e3,this.tooltipPlacement="top",this.hoist=!1}async handleCopy(){if(this.disabled||this.isCopying)return;this.isCopying=!0;let e=this.value;if(this.from){const t=this.getRootNode(),o=this.from.includes("."),i=this.from.includes("[")&&this.from.includes("]");let s=this.from,r="";o?[s,r]=this.from.trim().split("."):i&&([s,r]=this.from.trim().replace(/\]$/,"").split("["));const a="getElementById"in t?t.getElementById(s):null;a?i?e=a.getAttribute(r)||"":o?e=a[r]||"":e=a.textContent||"":(this.showStatus("error"),this.emit("sl-error"))}if(!e)this.showStatus("error"),this.emit("sl-error");else try{await navigator.clipboard.writeText(e),this.showStatus("success"),this.emit("sl-copy",{detail:{value:e}})}catch{this.showStatus("error"),this.emit("sl-error")}}async showStatus(e){const t=this.copyLabel||this.localize.term("copy"),o=this.successLabel||this.localize.term("copied"),i=this.errorLabel||this.localize.term("error"),s=e==="success"?this.successIcon:this.errorIcon,r=ot(this,"copy.in",{dir:"ltr"}),a=ot(this,"copy.out",{dir:"ltr"});this.tooltip.content=e==="success"?o:i,await this.copyIcon.animate(a.keyframes,a.options).finished,this.copyIcon.hidden=!0,this.status=e,s.hidden=!1,await s.animate(r.keyframes,r.options).finished,setTimeout(async()=>{await s.animate(a.keyframes,a.options).finished,s.hidden=!0,this.status="rest",this.copyIcon.hidden=!1,await this.copyIcon.animate(r.keyframes,r.options).finished,this.tooltip.content=t,this.isCopying=!1},this.feedbackDuration)}render(){const e=this.copyLabel||this.localize.term("copy");return x`
      <sl-tooltip
        class=${R({"copy-button":!0,"copy-button--success":this.status==="success","copy-button--error":this.status==="error"})}
        content=${e}
        placement=${this.tooltipPlacement}
        ?disabled=${this.disabled}
        ?hoist=${this.hoist}
        exportparts="
          base:tooltip__base,
          base__popup:tooltip__base__popup,
          base__arrow:tooltip__base__arrow,
          body:tooltip__body
        "
      >
        <button
          class="copy-button__button"
          part="button"
          type="button"
          ?disabled=${this.disabled}
          @click=${this.handleCopy}
        >
          <slot part="copy-icon" name="copy-icon">
            <sl-icon library="system" name="copy"></sl-icon>
          </slot>
          <slot part="success-icon" name="success-icon" hidden>
            <sl-icon library="system" name="check"></sl-icon>
          </slot>
          <slot part="error-icon" name="error-icon" hidden>
            <sl-icon library="system" name="x-lg"></sl-icon>
          </slot>
        </button>
      </sl-tooltip>
    `}};wt.styles=[D,Uf];wt.dependencies={"sl-icon":J,"sl-tooltip":yt};l([S('slot[name="copy-icon"]')],wt.prototype,"copyIcon",2);l([S('slot[name="success-icon"]')],wt.prototype,"successIcon",2);l([S('slot[name="error-icon"]')],wt.prototype,"errorIcon",2);l([S("sl-tooltip")],wt.prototype,"tooltip",2);l([z()],wt.prototype,"isCopying",2);l([z()],wt.prototype,"status",2);l([p()],wt.prototype,"value",2);l([p()],wt.prototype,"from",2);l([p({type:Boolean,reflect:!0})],wt.prototype,"disabled",2);l([p({attribute:"copy-label"})],wt.prototype,"copyLabel",2);l([p({attribute:"success-label"})],wt.prototype,"successLabel",2);l([p({attribute:"error-label"})],wt.prototype,"errorLabel",2);l([p({attribute:"feedback-duration",type:Number})],wt.prototype,"feedbackDuration",2);l([p({attribute:"tooltip-placement"})],wt.prototype,"tooltipPlacement",2);l([p({type:Boolean})],wt.prototype,"hoist",2);K("copy.in",{keyframes:[{scale:".25",opacity:".25"},{scale:"1",opacity:"1"}],options:{duration:100}});K("copy.out",{keyframes:[{scale:"1",opacity:"1"},{scale:".25",opacity:"0"}],options:{duration:100}});wt.define("sl-copy-button");var Nf=O`
  :host {
    display: block;
  }

  .details {
    border: solid 1px var(--sl-color-neutral-200);
    border-radius: var(--sl-border-radius-medium);
    background-color: var(--sl-color-neutral-0);
    overflow-anchor: none;
  }

  .details--disabled {
    opacity: 0.5;
  }

  .details__header {
    display: flex;
    align-items: center;
    border-radius: inherit;
    padding: var(--sl-spacing-medium);
    user-select: none;
    -webkit-user-select: none;
    cursor: pointer;
  }

  .details__header::-webkit-details-marker {
    display: none;
  }

  .details__header:focus {
    outline: none;
  }

  .details__header:focus-visible {
    outline: var(--sl-focus-ring);
    outline-offset: calc(1px + var(--sl-focus-ring-offset));
  }

  .details--disabled .details__header {
    cursor: not-allowed;
  }

  .details--disabled .details__header:focus-visible {
    outline: none;
    box-shadow: none;
  }

  .details__summary {
    flex: 1 1 auto;
    display: flex;
    align-items: center;
  }

  .details__summary-icon {
    flex: 0 0 auto;
    display: flex;
    align-items: center;
    transition: var(--sl-transition-medium) rotate ease;
  }

  .details--open .details__summary-icon {
    rotate: 90deg;
  }

  .details--open.details--rtl .details__summary-icon {
    rotate: -90deg;
  }

  .details--open slot[name='expand-icon'],
  .details:not(.details--open) slot[name='collapse-icon'] {
    display: none;
  }

  .details__body {
    overflow: hidden;
  }

  .details__content {
    display: block;
    padding: var(--sl-spacing-medium);
  }
`,re=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.open=!1,this.disabled=!1}firstUpdated(){this.body.style.height=this.open?"auto":"0",this.open&&(this.details.open=!0),this.detailsObserver=new MutationObserver(e=>{for(const t of e)t.type==="attributes"&&t.attributeName==="open"&&(this.details.open?this.show():this.hide())}),this.detailsObserver.observe(this.details,{attributes:!0})}disconnectedCallback(){var e;super.disconnectedCallback(),(e=this.detailsObserver)==null||e.disconnect()}handleSummaryClick(e){e.preventDefault(),this.disabled||(this.open?this.hide():this.show(),this.header.focus())}handleSummaryKeyDown(e){(e.key==="Enter"||e.key===" ")&&(e.preventDefault(),this.open?this.hide():this.show()),(e.key==="ArrowUp"||e.key==="ArrowLeft")&&(e.preventDefault(),this.hide()),(e.key==="ArrowDown"||e.key==="ArrowRight")&&(e.preventDefault(),this.show())}async handleOpenChange(){if(this.open){if(this.details.open=!0,this.emit("sl-show",{cancelable:!0}).defaultPrevented){this.open=!1,this.details.open=!1;return}await ut(this.body);const{keyframes:t,options:o}=ot(this,"details.show",{dir:this.localize.dir()});await at(this.body,bs(t,this.body.scrollHeight),o),this.body.style.height="auto",this.emit("sl-after-show")}else{if(this.emit("sl-hide",{cancelable:!0}).defaultPrevented){this.details.open=!0,this.open=!0;return}await ut(this.body);const{keyframes:t,options:o}=ot(this,"details.hide",{dir:this.localize.dir()});await at(this.body,bs(t,this.body.scrollHeight),o),this.body.style.height="auto",this.details.open=!1,this.emit("sl-after-hide")}}async show(){if(!(this.open||this.disabled))return this.open=!0,Pt(this,"sl-after-show")}async hide(){if(!(!this.open||this.disabled))return this.open=!1,Pt(this,"sl-after-hide")}render(){const e=this.localize.dir()==="rtl";return x`
      <details
        part="base"
        class=${R({details:!0,"details--open":this.open,"details--disabled":this.disabled,"details--rtl":e})}
      >
        <summary
          part="header"
          id="header"
          class="details__header"
          role="button"
          aria-expanded=${this.open?"true":"false"}
          aria-controls="content"
          aria-disabled=${this.disabled?"true":"false"}
          tabindex=${this.disabled?"-1":"0"}
          @click=${this.handleSummaryClick}
          @keydown=${this.handleSummaryKeyDown}
        >
          <slot name="summary" part="summary" class="details__summary">${this.summary}</slot>

          <span part="summary-icon" class="details__summary-icon">
            <slot name="expand-icon">
              <sl-icon library="system" name=${e?"chevron-left":"chevron-right"}></sl-icon>
            </slot>
            <slot name="collapse-icon">
              <sl-icon library="system" name=${e?"chevron-left":"chevron-right"}></sl-icon>
            </slot>
          </span>
        </summary>

        <div class="details__body" role="region" aria-labelledby="header">
          <slot part="content" id="content" class="details__content"></slot>
        </div>
      </details>
    `}};re.styles=[D,Nf];re.dependencies={"sl-icon":J};l([S(".details")],re.prototype,"details",2);l([S(".details__header")],re.prototype,"header",2);l([S(".details__body")],re.prototype,"body",2);l([S(".details__expand-icon-slot")],re.prototype,"expandIconSlot",2);l([p({type:Boolean,reflect:!0})],re.prototype,"open",2);l([p()],re.prototype,"summary",2);l([p({type:Boolean,reflect:!0})],re.prototype,"disabled",2);l([C("open",{waitUntilFirstUpdate:!0})],re.prototype,"handleOpenChange",1);K("details.show",{keyframes:[{height:"0",opacity:"0"},{height:"auto",opacity:"1"}],options:{duration:250,easing:"linear"}});K("details.hide",{keyframes:[{height:"auto",opacity:"1"},{height:"0",opacity:"0"}],options:{duration:250,easing:"linear"}});re.define("sl-details");var Hf=O`
  :host {
    --width: 31rem;
    --header-spacing: var(--sl-spacing-large);
    --body-spacing: var(--sl-spacing-large);
    --footer-spacing: var(--sl-spacing-large);

    display: contents;
  }

  .dialog {
    display: flex;
    align-items: center;
    justify-content: center;
    position: fixed;
    top: 0;
    right: 0;
    bottom: 0;
    left: 0;
    z-index: var(--sl-z-index-dialog);
  }

  .dialog__panel {
    display: flex;
    flex-direction: column;
    z-index: 2;
    width: var(--width);
    max-width: calc(100% - var(--sl-spacing-2x-large));
    max-height: calc(100% - var(--sl-spacing-2x-large));
    background-color: var(--sl-panel-background-color);
    border-radius: var(--sl-border-radius-medium);
    box-shadow: var(--sl-shadow-x-large);
  }

  .dialog__panel:focus {
    outline: none;
  }

  /* Ensure there's enough vertical padding for phones that don't update vh when chrome appears (e.g. iPhone) */
  @media screen and (max-width: 420px) {
    .dialog__panel {
      max-height: 80vh;
    }
  }

  .dialog--open .dialog__panel {
    display: flex;
    opacity: 1;
  }

  .dialog__header {
    flex: 0 0 auto;
    display: flex;
  }

  .dialog__title {
    flex: 1 1 auto;
    font: inherit;
    font-size: var(--sl-font-size-large);
    line-height: var(--sl-line-height-dense);
    padding: var(--header-spacing);
    margin: 0;
  }

  .dialog__header-actions {
    flex-shrink: 0;
    display: flex;
    flex-wrap: wrap;
    justify-content: end;
    gap: var(--sl-spacing-2x-small);
    padding: 0 var(--header-spacing);
  }

  .dialog__header-actions sl-icon-button,
  .dialog__header-actions ::slotted(sl-icon-button) {
    flex: 0 0 auto;
    display: flex;
    align-items: center;
    font-size: var(--sl-font-size-medium);
  }

  .dialog__body {
    flex: 1 1 auto;
    display: block;
    padding: var(--body-spacing);
    overflow: auto;
    -webkit-overflow-scrolling: touch;
  }

  .dialog__footer {
    flex: 0 0 auto;
    text-align: right;
    padding: var(--footer-spacing);
  }

  .dialog__footer ::slotted(sl-button:not(:first-of-type)) {
    margin-inline-start: var(--sl-spacing-x-small);
  }

  .dialog:not(.dialog--has-footer) .dialog__footer {
    display: none;
  }

  .dialog__overlay {
    position: fixed;
    top: 0;
    right: 0;
    bottom: 0;
    left: 0;
    background-color: var(--sl-overlay-background-color);
  }

  @media (forced-colors: active) {
    .dialog__panel {
      border: solid 1px var(--sl-color-neutral-0);
    }
  }
`,ye=class extends T{constructor(){super(...arguments),this.hasSlotController=new Ot(this,"footer"),this.localize=new j(this),this.modal=new ic(this),this.open=!1,this.label="",this.noHeader=!1,this.handleDocumentKeyDown=e=>{e.key==="Escape"&&this.modal.isActive()&&this.open&&(e.stopPropagation(),this.requestClose("keyboard"))}}firstUpdated(){this.dialog.hidden=!this.open,this.open&&(this.addOpenListeners(),this.modal.activate(),di(this))}disconnectedCallback(){super.disconnectedCallback(),this.modal.deactivate(),hi(this),this.removeOpenListeners()}requestClose(e){if(this.emit("sl-request-close",{cancelable:!0,detail:{source:e}}).defaultPrevented){const o=ot(this,"dialog.denyClose",{dir:this.localize.dir()});at(this.panel,o.keyframes,o.options);return}this.hide()}addOpenListeners(){var e;"CloseWatcher"in window?((e=this.closeWatcher)==null||e.destroy(),this.closeWatcher=new CloseWatcher,this.closeWatcher.onclose=()=>this.requestClose("keyboard")):document.addEventListener("keydown",this.handleDocumentKeyDown)}removeOpenListeners(){var e;(e=this.closeWatcher)==null||e.destroy(),document.removeEventListener("keydown",this.handleDocumentKeyDown)}async handleOpenChange(){if(this.open){this.emit("sl-show"),this.addOpenListeners(),this.originalTrigger=document.activeElement,this.modal.activate(),di(this);const e=this.querySelector("[autofocus]");e&&e.removeAttribute("autofocus"),await Promise.all([ut(this.dialog),ut(this.overlay)]),this.dialog.hidden=!1,requestAnimationFrame(()=>{this.emit("sl-initial-focus",{cancelable:!0}).defaultPrevented||(e?e.focus({preventScroll:!0}):this.panel.focus({preventScroll:!0})),e&&e.setAttribute("autofocus","")});const t=ot(this,"dialog.show",{dir:this.localize.dir()}),o=ot(this,"dialog.overlay.show",{dir:this.localize.dir()});await Promise.all([at(this.panel,t.keyframes,t.options),at(this.overlay,o.keyframes,o.options)]),this.emit("sl-after-show")}else{ba(this),this.emit("sl-hide"),this.removeOpenListeners(),this.modal.deactivate(),await Promise.all([ut(this.dialog),ut(this.overlay)]);const e=ot(this,"dialog.hide",{dir:this.localize.dir()}),t=ot(this,"dialog.overlay.hide",{dir:this.localize.dir()});await Promise.all([at(this.overlay,t.keyframes,t.options).then(()=>{this.overlay.hidden=!0}),at(this.panel,e.keyframes,e.options).then(()=>{this.panel.hidden=!0})]),this.dialog.hidden=!0,this.overlay.hidden=!1,this.panel.hidden=!1,hi(this);const o=this.originalTrigger;typeof(o==null?void 0:o.focus)=="function"&&setTimeout(()=>o.focus()),this.emit("sl-after-hide")}}async show(){if(!this.open)return this.open=!0,Pt(this,"sl-after-show")}async hide(){if(this.open)return this.open=!1,Pt(this,"sl-after-hide")}render(){return x`
      <div
        part="base"
        class=${R({dialog:!0,"dialog--open":this.open,"dialog--has-footer":this.hasSlotController.test("footer")})}
      >
        <div part="overlay" class="dialog__overlay" @click=${()=>this.requestClose("overlay")} tabindex="-1"></div>

        <div
          part="panel"
          class="dialog__panel"
          role="dialog"
          aria-modal="true"
          aria-hidden=${this.open?"false":"true"}
          aria-label=${E(this.noHeader?this.label:void 0)}
          aria-labelledby=${E(this.noHeader?void 0:"title")}
          tabindex="-1"
        >
          ${this.noHeader?"":x`
                <header part="header" class="dialog__header">
                  <h2 part="title" class="dialog__title" id="title">
                    <slot name="label"> ${this.label.length>0?this.label:"\uFEFF"} </slot>
                  </h2>
                  <div part="header-actions" class="dialog__header-actions">
                    <slot name="header-actions"></slot>
                    <sl-icon-button
                      part="close-button"
                      exportparts="base:close-button__base"
                      class="dialog__close"
                      name="x-lg"
                      label=${this.localize.term("close")}
                      library="system"
                      @click="${()=>this.requestClose("close-button")}"
                    ></sl-icon-button>
                  </div>
                </header>
              `}
          ${""}
          <div part="body" class="dialog__body" tabindex="-1"><slot></slot></div>

          <footer part="footer" class="dialog__footer">
            <slot name="footer"></slot>
          </footer>
        </div>
      </div>
    `}};ye.styles=[D,Hf];ye.dependencies={"sl-icon-button":gt};l([S(".dialog")],ye.prototype,"dialog",2);l([S(".dialog__panel")],ye.prototype,"panel",2);l([S(".dialog__overlay")],ye.prototype,"overlay",2);l([p({type:Boolean,reflect:!0})],ye.prototype,"open",2);l([p({reflect:!0})],ye.prototype,"label",2);l([p({attribute:"no-header",type:Boolean,reflect:!0})],ye.prototype,"noHeader",2);l([C("open",{waitUntilFirstUpdate:!0})],ye.prototype,"handleOpenChange",1);K("dialog.show",{keyframes:[{opacity:0,scale:.8},{opacity:1,scale:1}],options:{duration:250,easing:"ease"}});K("dialog.hide",{keyframes:[{opacity:1,scale:1},{opacity:0,scale:.8}],options:{duration:250,easing:"ease"}});K("dialog.denyClose",{keyframes:[{scale:1},{scale:1.02},{scale:1}],options:{duration:250}});K("dialog.overlay.show",{keyframes:[{opacity:0},{opacity:1}],options:{duration:250}});K("dialog.overlay.hide",{keyframes:[{opacity:1},{opacity:0}],options:{duration:250}});ye.define("sl-dialog");mt.define("sl-checkbox");var jf=O`
  :host {
    --grid-width: 280px;
    --grid-height: 200px;
    --grid-handle-size: 16px;
    --slider-height: 15px;
    --slider-handle-size: 17px;
    --swatch-size: 25px;

    display: inline-block;
  }

  .color-picker {
    width: var(--grid-width);
    font-family: var(--sl-font-sans);
    font-size: var(--sl-font-size-medium);
    font-weight: var(--sl-font-weight-normal);
    color: var(--color);
    background-color: var(--sl-panel-background-color);
    border-radius: var(--sl-border-radius-medium);
    user-select: none;
    -webkit-user-select: none;
  }

  .color-picker--inline {
    border: solid var(--sl-panel-border-width) var(--sl-panel-border-color);
  }

  .color-picker--inline:focus-visible {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  .color-picker__grid {
    position: relative;
    height: var(--grid-height);
    background-image: linear-gradient(to bottom, rgba(0, 0, 0, 0) 0%, rgba(0, 0, 0, 1) 100%),
      linear-gradient(to right, #fff 0%, rgba(255, 255, 255, 0) 100%);
    border-top-left-radius: var(--sl-border-radius-medium);
    border-top-right-radius: var(--sl-border-radius-medium);
    cursor: crosshair;
    forced-color-adjust: none;
  }

  .color-picker__grid-handle {
    position: absolute;
    width: var(--grid-handle-size);
    height: var(--grid-handle-size);
    border-radius: 50%;
    box-shadow: 0 0 0 1px rgba(0, 0, 0, 0.25);
    border: solid 2px white;
    margin-top: calc(var(--grid-handle-size) / -2);
    margin-left: calc(var(--grid-handle-size) / -2);
    transition: var(--sl-transition-fast) scale;
  }

  .color-picker__grid-handle--dragging {
    cursor: none;
    scale: 1.5;
  }

  .color-picker__grid-handle:focus-visible {
    outline: var(--sl-focus-ring);
  }

  .color-picker__controls {
    padding: var(--sl-spacing-small);
    display: flex;
    align-items: center;
  }

  .color-picker__sliders {
    flex: 1 1 auto;
  }

  .color-picker__slider {
    position: relative;
    height: var(--slider-height);
    border-radius: var(--sl-border-radius-pill);
    box-shadow: inset 0 0 0 1px rgba(0, 0, 0, 0.2);
    forced-color-adjust: none;
  }

  .color-picker__slider:not(:last-of-type) {
    margin-bottom: var(--sl-spacing-small);
  }

  .color-picker__slider-handle {
    position: absolute;
    top: calc(50% - var(--slider-handle-size) / 2);
    width: var(--slider-handle-size);
    height: var(--slider-handle-size);
    background-color: white;
    border-radius: 50%;
    box-shadow: 0 0 0 1px rgba(0, 0, 0, 0.25);
    margin-left: calc(var(--slider-handle-size) / -2);
  }

  .color-picker__slider-handle:focus-visible {
    outline: var(--sl-focus-ring);
  }

  .color-picker__hue {
    background-image: linear-gradient(
      to right,
      rgb(255, 0, 0) 0%,
      rgb(255, 255, 0) 17%,
      rgb(0, 255, 0) 33%,
      rgb(0, 255, 255) 50%,
      rgb(0, 0, 255) 67%,
      rgb(255, 0, 255) 83%,
      rgb(255, 0, 0) 100%
    );
  }

  .color-picker__alpha .color-picker__alpha-gradient {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    border-radius: inherit;
  }

  .color-picker__preview {
    flex: 0 0 auto;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    position: relative;
    width: 2.25rem;
    height: 2.25rem;
    border: none;
    border-radius: var(--sl-border-radius-circle);
    background: none;
    margin-left: var(--sl-spacing-small);
    cursor: copy;
    forced-color-adjust: none;
  }

  .color-picker__preview:before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    border-radius: inherit;
    box-shadow: inset 0 0 0 1px rgba(0, 0, 0, 0.2);

    /* We use a custom property in lieu of currentColor because of https://bugs.webkit.org/show_bug.cgi?id=216780 */
    background-color: var(--preview-color);
  }

  .color-picker__preview:focus-visible {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  .color-picker__preview-color {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    border: solid 1px rgba(0, 0, 0, 0.125);
  }

  .color-picker__preview-color--copied {
    animation: pulse 0.75s;
  }

  @keyframes pulse {
    0% {
      box-shadow: 0 0 0 0 var(--sl-color-primary-500);
    }
    70% {
      box-shadow: 0 0 0 0.5rem transparent;
    }
    100% {
      box-shadow: 0 0 0 0 transparent;
    }
  }

  .color-picker__user-input {
    display: flex;
    padding: 0 var(--sl-spacing-small) var(--sl-spacing-small) var(--sl-spacing-small);
  }

  .color-picker__user-input sl-input {
    min-width: 0; /* fix input width in Safari */
    flex: 1 1 auto;
  }

  .color-picker__user-input sl-button-group {
    margin-left: var(--sl-spacing-small);
  }

  .color-picker__user-input sl-button {
    min-width: 3.25rem;
    max-width: 3.25rem;
    font-size: 1rem;
  }

  .color-picker__swatches {
    display: grid;
    grid-template-columns: repeat(8, 1fr);
    grid-gap: 0.5rem;
    justify-items: center;
    border-top: solid 1px var(--sl-color-neutral-200);
    padding: var(--sl-spacing-small);
    forced-color-adjust: none;
  }

  .color-picker__swatch {
    position: relative;
    width: var(--swatch-size);
    height: var(--swatch-size);
    border-radius: var(--sl-border-radius-small);
  }

  .color-picker__swatch .color-picker__swatch-color {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    border: solid 1px rgba(0, 0, 0, 0.125);
    border-radius: inherit;
    cursor: pointer;
  }

  .color-picker__swatch:focus-visible {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  .color-picker__transparent-bg {
    background-image: linear-gradient(45deg, var(--sl-color-neutral-300) 25%, transparent 25%),
      linear-gradient(45deg, transparent 75%, var(--sl-color-neutral-300) 75%),
      linear-gradient(45deg, transparent 75%, var(--sl-color-neutral-300) 75%),
      linear-gradient(45deg, var(--sl-color-neutral-300) 25%, transparent 25%);
    background-size: 10px 10px;
    background-position:
      0 0,
      0 0,
      -5px -5px,
      5px 5px;
  }

  .color-picker--disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .color-picker--disabled .color-picker__grid,
  .color-picker--disabled .color-picker__grid-handle,
  .color-picker--disabled .color-picker__slider,
  .color-picker--disabled .color-picker__slider-handle,
  .color-picker--disabled .color-picker__preview,
  .color-picker--disabled .color-picker__swatch,
  .color-picker--disabled .color-picker__swatch-color {
    pointer-events: none;
  }

  /*
   * Color dropdown
   */

  .color-dropdown::part(panel) {
    max-height: none;
    background-color: var(--sl-panel-background-color);
    border: solid var(--sl-panel-border-width) var(--sl-panel-border-color);
    border-radius: var(--sl-border-radius-medium);
    overflow: visible;
  }

  .color-dropdown__trigger {
    display: inline-block;
    position: relative;
    background-color: transparent;
    border: none;
    cursor: pointer;
    forced-color-adjust: none;
  }

  .color-dropdown__trigger.color-dropdown__trigger--small {
    width: var(--sl-input-height-small);
    height: var(--sl-input-height-small);
    border-radius: var(--sl-border-radius-circle);
  }

  .color-dropdown__trigger.color-dropdown__trigger--medium {
    width: var(--sl-input-height-medium);
    height: var(--sl-input-height-medium);
    border-radius: var(--sl-border-radius-circle);
  }

  .color-dropdown__trigger.color-dropdown__trigger--large {
    width: var(--sl-input-height-large);
    height: var(--sl-input-height-large);
    border-radius: var(--sl-border-radius-circle);
  }

  .color-dropdown__trigger:before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    border-radius: inherit;
    background-color: currentColor;
    box-shadow:
      inset 0 0 0 2px var(--sl-input-border-color),
      inset 0 0 0 4px var(--sl-color-neutral-0);
  }

  .color-dropdown__trigger--empty:before {
    background-color: transparent;
  }

  .color-dropdown__trigger:focus-visible {
    outline: none;
  }

  .color-dropdown__trigger:focus-visible:not(.color-dropdown__trigger--disabled) {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  .color-dropdown__trigger.color-dropdown__trigger--disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
`,X=class extends T{constructor(){super(...arguments),this.formControlController=new Ee(this,{assumeInteractionOn:["click"]}),this.hasSlotController=new Ot(this,"[default]","prefix","suffix"),this.localize=new j(this),this.hasFocus=!1,this.invalid=!1,this.title="",this.variant="default",this.size="medium",this.caret=!1,this.disabled=!1,this.loading=!1,this.outline=!1,this.pill=!1,this.circle=!1,this.type="button",this.name="",this.value="",this.href="",this.rel="noreferrer noopener"}get validity(){return this.isButton()?this.button.validity:Os}get validationMessage(){return this.isButton()?this.button.validationMessage:""}firstUpdated(){this.isButton()&&this.formControlController.updateValidity()}handleBlur(){this.hasFocus=!1,this.emit("sl-blur")}handleFocus(){this.hasFocus=!0,this.emit("sl-focus")}handleClick(){this.type==="submit"&&this.formControlController.submit(this),this.type==="reset"&&this.formControlController.reset(this)}handleInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}isButton(){return!this.href}isLink(){return!!this.href}handleDisabledChange(){this.isButton()&&this.formControlController.setValidity(this.disabled)}click(){this.button.click()}focus(e){this.button.focus(e)}blur(){this.button.blur()}checkValidity(){return this.isButton()?this.button.checkValidity():!0}getForm(){return this.formControlController.getForm()}reportValidity(){return this.isButton()?this.button.reportValidity():!0}setCustomValidity(e){this.isButton()&&(this.button.setCustomValidity(e),this.formControlController.updateValidity())}render(){const e=this.isLink(),t=e?xs`a`:xs`button`;return ci`
      <${t}
        part="base"
        class=${R({button:!0,"button--default":this.variant==="default","button--primary":this.variant==="primary","button--success":this.variant==="success","button--neutral":this.variant==="neutral","button--warning":this.variant==="warning","button--danger":this.variant==="danger","button--text":this.variant==="text","button--small":this.size==="small","button--medium":this.size==="medium","button--large":this.size==="large","button--caret":this.caret,"button--circle":this.circle,"button--disabled":this.disabled,"button--focused":this.hasFocus,"button--loading":this.loading,"button--standard":!this.outline,"button--outline":this.outline,"button--pill":this.pill,"button--rtl":this.localize.dir()==="rtl","button--has-label":this.hasSlotController.test("[default]"),"button--has-prefix":this.hasSlotController.test("prefix"),"button--has-suffix":this.hasSlotController.test("suffix")})}
        ?disabled=${E(e?void 0:this.disabled)}
        type=${E(e?void 0:this.type)}
        title=${this.title}
        name=${E(e?void 0:this.name)}
        value=${E(e?void 0:this.value)}
        href=${E(e&&!this.disabled?this.href:void 0)}
        target=${E(e?this.target:void 0)}
        download=${E(e?this.download:void 0)}
        rel=${E(e?this.rel:void 0)}
        role=${E(e?void 0:"button")}
        aria-disabled=${this.disabled?"true":"false"}
        tabindex=${this.disabled?"-1":"0"}
        @blur=${this.handleBlur}
        @focus=${this.handleFocus}
        @invalid=${this.isButton()?this.handleInvalid:null}
        @click=${this.handleClick}
      >
        <slot name="prefix" part="prefix" class="button__prefix"></slot>
        <slot part="label" class="button__label"></slot>
        <slot name="suffix" part="suffix" class="button__suffix"></slot>
        ${this.caret?ci` <sl-icon part="caret" class="button__caret" library="system" name="caret"></sl-icon> `:""}
        ${this.loading?ci`<sl-spinner part="spinner"></sl-spinner>`:""}
      </${t}>
    `}};X.styles=[D,Ql];X.dependencies={"sl-icon":J,"sl-spinner":Di};l([S(".button")],X.prototype,"button",2);l([z()],X.prototype,"hasFocus",2);l([z()],X.prototype,"invalid",2);l([p()],X.prototype,"title",2);l([p({reflect:!0})],X.prototype,"variant",2);l([p({reflect:!0})],X.prototype,"size",2);l([p({type:Boolean,reflect:!0})],X.prototype,"caret",2);l([p({type:Boolean,reflect:!0})],X.prototype,"disabled",2);l([p({type:Boolean,reflect:!0})],X.prototype,"loading",2);l([p({type:Boolean,reflect:!0})],X.prototype,"outline",2);l([p({type:Boolean,reflect:!0})],X.prototype,"pill",2);l([p({type:Boolean,reflect:!0})],X.prototype,"circle",2);l([p()],X.prototype,"type",2);l([p()],X.prototype,"name",2);l([p()],X.prototype,"value",2);l([p()],X.prototype,"href",2);l([p()],X.prototype,"target",2);l([p()],X.prototype,"rel",2);l([p()],X.prototype,"download",2);l([p()],X.prototype,"form",2);l([p({attribute:"formaction"})],X.prototype,"formAction",2);l([p({attribute:"formenctype"})],X.prototype,"formEnctype",2);l([p({attribute:"formmethod"})],X.prototype,"formMethod",2);l([p({attribute:"formnovalidate",type:Boolean})],X.prototype,"formNoValidate",2);l([p({attribute:"formtarget"})],X.prototype,"formTarget",2);l([C("disabled",{waitUntilFirstUpdate:!0})],X.prototype,"handleDisabledChange",1);function _t(e,t){qf(e)&&(e="100%");const o=Wf(e);return e=t===360?e:Math.min(t,Math.max(0,parseFloat(e))),o&&(e=parseInt(String(e*t),10)/100),Math.abs(e-t)<1e-6?1:(t===360?e=(e<0?e%t+t:e%t)/parseFloat(String(t)):e=e%t/parseFloat(String(t)),e)}function Ki(e){return Math.min(1,Math.max(0,e))}function qf(e){return typeof e=="string"&&e.indexOf(".")!==-1&&parseFloat(e)===1}function Wf(e){return typeof e=="string"&&e.indexOf("%")!==-1}function sc(e){return e=parseFloat(e),(isNaN(e)||e<0||e>1)&&(e=1),e}function Yi(e){return Number(e)<=1?`${Number(e)*100}%`:e}function Qe(e){return e.length===1?"0"+e:String(e)}function Kf(e,t,o){return{r:_t(e,255)*255,g:_t(t,255)*255,b:_t(o,255)*255}}function Cn(e,t,o){e=_t(e,255),t=_t(t,255),o=_t(o,255);const i=Math.max(e,t,o),s=Math.min(e,t,o);let r=0,a=0;const n=(i+s)/2;if(i===s)a=0,r=0;else{const c=i-s;switch(a=n>.5?c/(2-i-s):c/(i+s),i){case e:r=(t-o)/c+(t<o?6:0);break;case t:r=(o-e)/c+2;break;case o:r=(e-t)/c+4;break}r/=6}return{h:r,s:a,l:n}}function sr(e,t,o){return o<0&&(o+=1),o>1&&(o-=1),o<1/6?e+(t-e)*(6*o):o<1/2?t:o<2/3?e+(t-e)*(2/3-o)*6:e}function Yf(e,t,o){let i,s,r;if(e=_t(e,360),t=_t(t,100),o=_t(o,100),t===0)s=o,r=o,i=o;else{const a=o<.5?o*(1+t):o+t-o*t,n=2*o-a;i=sr(n,a,e+1/3),s=sr(n,a,e),r=sr(n,a,e-1/3)}return{r:i*255,g:s*255,b:r*255}}function Sn(e,t,o){e=_t(e,255),t=_t(t,255),o=_t(o,255);const i=Math.max(e,t,o),s=Math.min(e,t,o);let r=0;const a=i,n=i-s,c=i===0?0:n/i;if(i===s)r=0;else{switch(i){case e:r=(t-o)/n+(t<o?6:0);break;case t:r=(o-e)/n+2;break;case o:r=(e-t)/n+4;break}r/=6}return{h:r,s:c,v:a}}function Xf(e,t,o){e=_t(e,360)*6,t=_t(t,100),o=_t(o,100);const i=Math.floor(e),s=e-i,r=o*(1-t),a=o*(1-s*t),n=o*(1-(1-s)*t),c=i%6,d=[o,a,r,r,n,o][c],u=[n,o,o,a,r,r][c],h=[r,r,n,o,o,a][c];return{r:d*255,g:u*255,b:h*255}}function An(e,t,o,i){const s=[Qe(Math.round(e).toString(16)),Qe(Math.round(t).toString(16)),Qe(Math.round(o).toString(16))];return i&&s[0].startsWith(s[0].charAt(1))&&s[1].startsWith(s[1].charAt(1))&&s[2].startsWith(s[2].charAt(1))?s[0].charAt(0)+s[1].charAt(0)+s[2].charAt(0):s.join("")}function Qf(e,t,o,i,s){const r=[Qe(Math.round(e).toString(16)),Qe(Math.round(t).toString(16)),Qe(Math.round(o).toString(16)),Qe(Gf(i))];return s&&r[0].startsWith(r[0].charAt(1))&&r[1].startsWith(r[1].charAt(1))&&r[2].startsWith(r[2].charAt(1))&&r[3].startsWith(r[3].charAt(1))?r[0].charAt(0)+r[1].charAt(0)+r[2].charAt(0)+r[3].charAt(0):r.join("")}function Zf(e,t,o,i){const s=e/100,r=t/100,a=o/100,n=i/100,c=255*(1-s)*(1-n),d=255*(1-r)*(1-n),u=255*(1-a)*(1-n);return{r:c,g:d,b:u}}function zn(e,t,o){let i=1-e/255,s=1-t/255,r=1-o/255,a=Math.min(i,s,r);return a===1?(i=0,s=0,r=0):(i=(i-a)/(1-a)*100,s=(s-a)/(1-a)*100,r=(r-a)/(1-a)*100),a*=100,{c:Math.round(i),m:Math.round(s),y:Math.round(r),k:Math.round(a)}}function Gf(e){return Math.round(parseFloat(e)*255).toString(16)}function En(e){return Mt(e)/255}function Mt(e){return parseInt(e,16)}function Jf(e){return{r:e>>16,g:(e&65280)>>8,b:e&255}}const Ar={aliceblue:"#f0f8ff",antiquewhite:"#faebd7",aqua:"#00ffff",aquamarine:"#7fffd4",azure:"#f0ffff",beige:"#f5f5dc",bisque:"#ffe4c4",black:"#000000",blanchedalmond:"#ffebcd",blue:"#0000ff",blueviolet:"#8a2be2",brown:"#a52a2a",burlywood:"#deb887",cadetblue:"#5f9ea0",chartreuse:"#7fff00",chocolate:"#d2691e",coral:"#ff7f50",cornflowerblue:"#6495ed",cornsilk:"#fff8dc",crimson:"#dc143c",cyan:"#00ffff",darkblue:"#00008b",darkcyan:"#008b8b",darkgoldenrod:"#b8860b",darkgray:"#a9a9a9",darkgreen:"#006400",darkgrey:"#a9a9a9",darkkhaki:"#bdb76b",darkmagenta:"#8b008b",darkolivegreen:"#556b2f",darkorange:"#ff8c00",darkorchid:"#9932cc",darkred:"#8b0000",darksalmon:"#e9967a",darkseagreen:"#8fbc8f",darkslateblue:"#483d8b",darkslategray:"#2f4f4f",darkslategrey:"#2f4f4f",darkturquoise:"#00ced1",darkviolet:"#9400d3",deeppink:"#ff1493",deepskyblue:"#00bfff",dimgray:"#696969",dimgrey:"#696969",dodgerblue:"#1e90ff",firebrick:"#b22222",floralwhite:"#fffaf0",forestgreen:"#228b22",fuchsia:"#ff00ff",gainsboro:"#dcdcdc",ghostwhite:"#f8f8ff",goldenrod:"#daa520",gold:"#ffd700",gray:"#808080",green:"#008000",greenyellow:"#adff2f",grey:"#808080",honeydew:"#f0fff0",hotpink:"#ff69b4",indianred:"#cd5c5c",indigo:"#4b0082",ivory:"#fffff0",khaki:"#f0e68c",lavenderblush:"#fff0f5",lavender:"#e6e6fa",lawngreen:"#7cfc00",lemonchiffon:"#fffacd",lightblue:"#add8e6",lightcoral:"#f08080",lightcyan:"#e0ffff",lightgoldenrodyellow:"#fafad2",lightgray:"#d3d3d3",lightgreen:"#90ee90",lightgrey:"#d3d3d3",lightpink:"#ffb6c1",lightsalmon:"#ffa07a",lightseagreen:"#20b2aa",lightskyblue:"#87cefa",lightslategray:"#778899",lightslategrey:"#778899",lightsteelblue:"#b0c4de",lightyellow:"#ffffe0",lime:"#00ff00",limegreen:"#32cd32",linen:"#faf0e6",magenta:"#ff00ff",maroon:"#800000",mediumaquamarine:"#66cdaa",mediumblue:"#0000cd",mediumorchid:"#ba55d3",mediumpurple:"#9370db",mediumseagreen:"#3cb371",mediumslateblue:"#7b68ee",mediumspringgreen:"#00fa9a",mediumturquoise:"#48d1cc",mediumvioletred:"#c71585",midnightblue:"#191970",mintcream:"#f5fffa",mistyrose:"#ffe4e1",moccasin:"#ffe4b5",navajowhite:"#ffdead",navy:"#000080",oldlace:"#fdf5e6",olive:"#808000",olivedrab:"#6b8e23",orange:"#ffa500",orangered:"#ff4500",orchid:"#da70d6",palegoldenrod:"#eee8aa",palegreen:"#98fb98",paleturquoise:"#afeeee",palevioletred:"#db7093",papayawhip:"#ffefd5",peachpuff:"#ffdab9",peru:"#cd853f",pink:"#ffc0cb",plum:"#dda0dd",powderblue:"#b0e0e6",purple:"#800080",rebeccapurple:"#663399",red:"#ff0000",rosybrown:"#bc8f8f",royalblue:"#4169e1",saddlebrown:"#8b4513",salmon:"#fa8072",sandybrown:"#f4a460",seagreen:"#2e8b57",seashell:"#fff5ee",sienna:"#a0522d",silver:"#c0c0c0",skyblue:"#87ceeb",slateblue:"#6a5acd",slategray:"#708090",slategrey:"#708090",snow:"#fffafa",springgreen:"#00ff7f",steelblue:"#4682b4",tan:"#d2b48c",teal:"#008080",thistle:"#d8bfd8",tomato:"#ff6347",turquoise:"#40e0d0",violet:"#ee82ee",wheat:"#f5deb3",white:"#ffffff",whitesmoke:"#f5f5f5",yellow:"#ffff00",yellowgreen:"#9acd32"};function tm(e){let t={r:0,g:0,b:0},o=1,i=null,s=null,r=null,a=!1,n=!1;return typeof e=="string"&&(e=im(e)),typeof e=="object"&&(Dt(e.r)&&Dt(e.g)&&Dt(e.b)?(t=Kf(e.r,e.g,e.b),a=!0,n=String(e.r).substr(-1)==="%"?"prgb":"rgb"):Dt(e.h)&&Dt(e.s)&&Dt(e.v)?(i=Yi(e.s),s=Yi(e.v),t=Xf(e.h,i,s),a=!0,n="hsv"):Dt(e.h)&&Dt(e.s)&&Dt(e.l)?(i=Yi(e.s),r=Yi(e.l),t=Yf(e.h,i,r),a=!0,n="hsl"):Dt(e.c)&&Dt(e.m)&&Dt(e.y)&&Dt(e.k)&&(t=Zf(e.c,e.m,e.y,e.k),a=!0,n="cmyk"),Object.prototype.hasOwnProperty.call(e,"a")&&(o=e.a)),o=sc(o),{ok:a,format:e.format||n,r:Math.min(255,Math.max(t.r,0)),g:Math.min(255,Math.max(t.g,0)),b:Math.min(255,Math.max(t.b,0)),a:o}}const em="[-\\+]?\\d+%?",om="[-\\+]?\\d*\\.\\d+%?",Oe="(?:"+om+")|(?:"+em+")",rr="[\\s|\\(]+("+Oe+")[,|\\s]+("+Oe+")[,|\\s]+("+Oe+")\\s*\\)?",Xi="[\\s|\\(]+("+Oe+")[,|\\s]+("+Oe+")[,|\\s]+("+Oe+")[,|\\s]+("+Oe+")\\s*\\)?",Wt={CSS_UNIT:new RegExp(Oe),rgb:new RegExp("rgb"+rr),rgba:new RegExp("rgba"+Xi),hsl:new RegExp("hsl"+rr),hsla:new RegExp("hsla"+Xi),hsv:new RegExp("hsv"+rr),hsva:new RegExp("hsva"+Xi),cmyk:new RegExp("cmyk"+Xi),hex3:/^#?([0-9a-fA-F]{1})([0-9a-fA-F]{1})([0-9a-fA-F]{1})$/,hex6:/^#?([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})$/,hex4:/^#?([0-9a-fA-F]{1})([0-9a-fA-F]{1})([0-9a-fA-F]{1})([0-9a-fA-F]{1})$/,hex8:/^#?([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})$/};function im(e){if(e=e.trim().toLowerCase(),e.length===0)return!1;let t=!1;if(Ar[e])e=Ar[e],t=!0;else if(e==="transparent")return{r:0,g:0,b:0,a:0,format:"name"};let o=Wt.rgb.exec(e);return o?{r:o[1],g:o[2],b:o[3]}:(o=Wt.rgba.exec(e),o?{r:o[1],g:o[2],b:o[3],a:o[4]}:(o=Wt.hsl.exec(e),o?{h:o[1],s:o[2],l:o[3]}:(o=Wt.hsla.exec(e),o?{h:o[1],s:o[2],l:o[3],a:o[4]}:(o=Wt.hsv.exec(e),o?{h:o[1],s:o[2],v:o[3]}:(o=Wt.hsva.exec(e),o?{h:o[1],s:o[2],v:o[3],a:o[4]}:(o=Wt.cmyk.exec(e),o?{c:o[1],m:o[2],y:o[3],k:o[4]}:(o=Wt.hex8.exec(e),o?{r:Mt(o[1]),g:Mt(o[2]),b:Mt(o[3]),a:En(o[4]),format:t?"name":"hex8"}:(o=Wt.hex6.exec(e),o?{r:Mt(o[1]),g:Mt(o[2]),b:Mt(o[3]),format:t?"name":"hex"}:(o=Wt.hex4.exec(e),o?{r:Mt(o[1]+o[1]),g:Mt(o[2]+o[2]),b:Mt(o[3]+o[3]),a:En(o[4]+o[4]),format:t?"name":"hex8"}:(o=Wt.hex3.exec(e),o?{r:Mt(o[1]+o[1]),g:Mt(o[2]+o[2]),b:Mt(o[3]+o[3]),format:t?"name":"hex"}:!1))))))))))}function Dt(e){return typeof e=="number"?!Number.isNaN(e):Wt.CSS_UNIT.test(e)}class rt{constructor(t="",o={}){if(t instanceof rt)return t;typeof t=="number"&&(t=Jf(t)),this.originalInput=t;const i=tm(t);this.originalInput=t,this.r=i.r,this.g=i.g,this.b=i.b,this.a=i.a,this.roundA=Math.round(100*this.a)/100,this.format=o.format??i.format,this.gradientType=o.gradientType,this.r<1&&(this.r=Math.round(this.r)),this.g<1&&(this.g=Math.round(this.g)),this.b<1&&(this.b=Math.round(this.b)),this.isValid=i.ok}isDark(){return this.getBrightness()<128}isLight(){return!this.isDark()}getBrightness(){const t=this.toRgb();return(t.r*299+t.g*587+t.b*114)/1e3}getLuminance(){const t=this.toRgb();let o,i,s;const r=t.r/255,a=t.g/255,n=t.b/255;return r<=.03928?o=r/12.92:o=Math.pow((r+.055)/1.055,2.4),a<=.03928?i=a/12.92:i=Math.pow((a+.055)/1.055,2.4),n<=.03928?s=n/12.92:s=Math.pow((n+.055)/1.055,2.4),.2126*o+.7152*i+.0722*s}getAlpha(){return this.a}setAlpha(t){return this.a=sc(t),this.roundA=Math.round(100*this.a)/100,this}isMonochrome(){const{s:t}=this.toHsl();return t===0}toHsv(){const t=Sn(this.r,this.g,this.b);return{h:t.h*360,s:t.s,v:t.v,a:this.a}}toHsvString(){const t=Sn(this.r,this.g,this.b),o=Math.round(t.h*360),i=Math.round(t.s*100),s=Math.round(t.v*100);return this.a===1?`hsv(${o}, ${i}%, ${s}%)`:`hsva(${o}, ${i}%, ${s}%, ${this.roundA})`}toHsl(){const t=Cn(this.r,this.g,this.b);return{h:t.h*360,s:t.s,l:t.l,a:this.a}}toHslString(){const t=Cn(this.r,this.g,this.b),o=Math.round(t.h*360),i=Math.round(t.s*100),s=Math.round(t.l*100);return this.a===1?`hsl(${o}, ${i}%, ${s}%)`:`hsla(${o}, ${i}%, ${s}%, ${this.roundA})`}toHex(t=!1){return An(this.r,this.g,this.b,t)}toHexString(t=!1){return"#"+this.toHex(t)}toHex8(t=!1){return Qf(this.r,this.g,this.b,this.a,t)}toHex8String(t=!1){return"#"+this.toHex8(t)}toHexShortString(t=!1){return this.a===1?this.toHexString(t):this.toHex8String(t)}toRgb(){return{r:Math.round(this.r),g:Math.round(this.g),b:Math.round(this.b),a:this.a}}toRgbString(){const t=Math.round(this.r),o=Math.round(this.g),i=Math.round(this.b);return this.a===1?`rgb(${t}, ${o}, ${i})`:`rgba(${t}, ${o}, ${i}, ${this.roundA})`}toPercentageRgb(){const t=o=>`${Math.round(_t(o,255)*100)}%`;return{r:t(this.r),g:t(this.g),b:t(this.b),a:this.a}}toPercentageRgbString(){const t=o=>Math.round(_t(o,255)*100);return this.a===1?`rgb(${t(this.r)}%, ${t(this.g)}%, ${t(this.b)}%)`:`rgba(${t(this.r)}%, ${t(this.g)}%, ${t(this.b)}%, ${this.roundA})`}toCmyk(){return{...zn(this.r,this.g,this.b)}}toCmykString(){const{c:t,m:o,y:i,k:s}=zn(this.r,this.g,this.b);return`cmyk(${t}, ${o}, ${i}, ${s})`}toName(){if(this.a===0)return"transparent";if(this.a<1)return!1;const t="#"+An(this.r,this.g,this.b,!1);for(const[o,i]of Object.entries(Ar))if(t===i)return o;return!1}toString(t){const o=!!t;t=t??this.format;let i=!1;const s=this.a<1&&this.a>=0;return!o&&s&&(t.startsWith("hex")||t==="name")?t==="name"&&this.a===0?this.toName():this.toRgbString():(t==="rgb"&&(i=this.toRgbString()),t==="prgb"&&(i=this.toPercentageRgbString()),(t==="hex"||t==="hex6")&&(i=this.toHexString()),t==="hex3"&&(i=this.toHexString(!0)),t==="hex4"&&(i=this.toHex8String(!0)),t==="hex8"&&(i=this.toHex8String()),t==="name"&&(i=this.toName()),t==="hsl"&&(i=this.toHslString()),t==="hsv"&&(i=this.toHsvString()),t==="cmyk"&&(i=this.toCmykString()),i||this.toHexString())}toNumber(){return(Math.round(this.r)<<16)+(Math.round(this.g)<<8)+Math.round(this.b)}clone(){return new rt(this.toString())}lighten(t=10){const o=this.toHsl();return o.l+=t/100,o.l=Ki(o.l),new rt(o)}brighten(t=10){const o=this.toRgb();return o.r=Math.max(0,Math.min(255,o.r-Math.round(255*-(t/100)))),o.g=Math.max(0,Math.min(255,o.g-Math.round(255*-(t/100)))),o.b=Math.max(0,Math.min(255,o.b-Math.round(255*-(t/100)))),new rt(o)}darken(t=10){const o=this.toHsl();return o.l-=t/100,o.l=Ki(o.l),new rt(o)}tint(t=10){return this.mix("white",t)}shade(t=10){return this.mix("black",t)}desaturate(t=10){const o=this.toHsl();return o.s-=t/100,o.s=Ki(o.s),new rt(o)}saturate(t=10){const o=this.toHsl();return o.s+=t/100,o.s=Ki(o.s),new rt(o)}greyscale(){return this.desaturate(100)}spin(t){const o=this.toHsl(),i=(o.h+t)%360;return o.h=i<0?360+i:i,new rt(o)}mix(t,o=50){const i=this.toRgb(),s=new rt(t).toRgb(),r=o/100,a={r:(s.r-i.r)*r+i.r,g:(s.g-i.g)*r+i.g,b:(s.b-i.b)*r+i.b,a:(s.a-i.a)*r+i.a};return new rt(a)}analogous(t=6,o=30){const i=this.toHsl(),s=360/o,r=[this];for(i.h=(i.h-(s*t>>1)+720)%360;--t;)i.h=(i.h+s)%360,r.push(new rt(i));return r}complement(){const t=this.toHsl();return t.h=(t.h+180)%360,new rt(t)}monochromatic(t=6){const o=this.toHsv(),{h:i}=o,{s}=o;let{v:r}=o;const a=[],n=1/t;for(;t--;)a.push(new rt({h:i,s,v:r})),r=(r+n)%1;return a}splitcomplement(){const t=this.toHsl(),{h:o}=t;return[this,new rt({h:(o+72)%360,s:t.s,l:t.l}),new rt({h:(o+216)%360,s:t.s,l:t.l})]}onBackground(t){const o=this.toRgb(),i=new rt(t).toRgb(),s=o.a+i.a*(1-o.a);return new rt({r:(o.r*o.a+i.r*i.a*(1-o.a))/s,g:(o.g*o.a+i.g*i.a*(1-o.a))/s,b:(o.b*o.a+i.b*i.a*(1-o.a))/s,a:s})}triad(){return this.polyad(3)}tetrad(){return this.polyad(4)}polyad(t){const o=this.toHsl(),{h:i}=o,s=[this],r=360/t;for(let a=1;a<t;a++)s.push(new rt({h:(i+a*r)%360,s:o.s,l:o.l}));return s}equals(t){const o=new rt(t);return this.format==="cmyk"||o.format==="cmyk"?this.toCmykString()===o.toCmykString():this.toRgbString()===o.toRgbString()}}var Tn="EyeDropper"in window,H=class extends T{constructor(){super(),this.formControlController=new Ee(this),this.isSafeValue=!1,this.localize=new j(this),this.hasFocus=!1,this.isDraggingGridHandle=!1,this.isEmpty=!1,this.inputValue="",this.hue=0,this.saturation=100,this.brightness=100,this.alpha=100,this.value="",this.defaultValue="",this.label="",this.format="hex",this.inline=!1,this.size="medium",this.noFormatToggle=!1,this.name="",this.disabled=!1,this.hoist=!1,this.opacity=!1,this.uppercase=!1,this.swatches="",this.form="",this.required=!1,this.handleFocusIn=()=>{this.hasFocus=!0,this.emit("sl-focus")},this.handleFocusOut=()=>{this.hasFocus=!1,this.emit("sl-blur")},this.addEventListener("focusin",this.handleFocusIn),this.addEventListener("focusout",this.handleFocusOut)}get validity(){return this.input.validity}get validationMessage(){return this.input.validationMessage}firstUpdated(){this.input.updateComplete.then(()=>{this.formControlController.updateValidity()})}handleCopy(){this.input.select(),document.execCommand("copy"),this.previewButton.focus(),this.previewButton.classList.add("color-picker__preview-color--copied"),this.previewButton.addEventListener("animationend",()=>{this.previewButton.classList.remove("color-picker__preview-color--copied")})}handleFormatToggle(){const e=["hex","rgb","hsl","hsv"],t=(e.indexOf(this.format)+1)%e.length;this.format=e[t],this.setColor(this.value),this.emit("sl-change"),this.emit("sl-input")}handleAlphaDrag(e){const t=this.shadowRoot.querySelector(".color-picker__slider.color-picker__alpha"),o=t.querySelector(".color-picker__slider-handle"),{width:i}=t.getBoundingClientRect();let s=this.value,r=this.value;o.focus(),e.preventDefault(),ui(t,{onMove:a=>{this.alpha=ht(a/i*100,0,100),this.syncValues(),this.value!==r&&(r=this.value,this.emit("sl-input"))},onStop:()=>{this.value!==s&&(s=this.value,this.emit("sl-change"))},initialEvent:e})}handleHueDrag(e){const t=this.shadowRoot.querySelector(".color-picker__slider.color-picker__hue"),o=t.querySelector(".color-picker__slider-handle"),{width:i}=t.getBoundingClientRect();let s=this.value,r=this.value;o.focus(),e.preventDefault(),ui(t,{onMove:a=>{this.hue=ht(a/i*360,0,360),this.syncValues(),this.value!==r&&(r=this.value,this.emit("sl-input"))},onStop:()=>{this.value!==s&&(s=this.value,this.emit("sl-change"))},initialEvent:e})}handleGridDrag(e){const t=this.shadowRoot.querySelector(".color-picker__grid"),o=t.querySelector(".color-picker__grid-handle"),{width:i,height:s}=t.getBoundingClientRect();let r=this.value,a=this.value;o.focus(),e.preventDefault(),this.isDraggingGridHandle=!0,ui(t,{onMove:(n,c)=>{this.saturation=ht(n/i*100,0,100),this.brightness=ht(100-c/s*100,0,100),this.syncValues(),this.value!==a&&(a=this.value,this.emit("sl-input"))},onStop:()=>{this.isDraggingGridHandle=!1,this.value!==r&&(r=this.value,this.emit("sl-change"))},initialEvent:e})}handleAlphaKeyDown(e){const t=e.shiftKey?10:1,o=this.value;e.key==="ArrowLeft"&&(e.preventDefault(),this.alpha=ht(this.alpha-t,0,100),this.syncValues()),e.key==="ArrowRight"&&(e.preventDefault(),this.alpha=ht(this.alpha+t,0,100),this.syncValues()),e.key==="Home"&&(e.preventDefault(),this.alpha=0,this.syncValues()),e.key==="End"&&(e.preventDefault(),this.alpha=100,this.syncValues()),this.value!==o&&(this.emit("sl-change"),this.emit("sl-input"))}handleHueKeyDown(e){const t=e.shiftKey?10:1,o=this.value;e.key==="ArrowLeft"&&(e.preventDefault(),this.hue=ht(this.hue-t,0,360),this.syncValues()),e.key==="ArrowRight"&&(e.preventDefault(),this.hue=ht(this.hue+t,0,360),this.syncValues()),e.key==="Home"&&(e.preventDefault(),this.hue=0,this.syncValues()),e.key==="End"&&(e.preventDefault(),this.hue=360,this.syncValues()),this.value!==o&&(this.emit("sl-change"),this.emit("sl-input"))}handleGridKeyDown(e){const t=e.shiftKey?10:1,o=this.value;e.key==="ArrowLeft"&&(e.preventDefault(),this.saturation=ht(this.saturation-t,0,100),this.syncValues()),e.key==="ArrowRight"&&(e.preventDefault(),this.saturation=ht(this.saturation+t,0,100),this.syncValues()),e.key==="ArrowUp"&&(e.preventDefault(),this.brightness=ht(this.brightness+t,0,100),this.syncValues()),e.key==="ArrowDown"&&(e.preventDefault(),this.brightness=ht(this.brightness-t,0,100),this.syncValues()),this.value!==o&&(this.emit("sl-change"),this.emit("sl-input"))}handleInputChange(e){const t=e.target,o=this.value;e.stopPropagation(),this.input.value?(this.setColor(t.value),t.value=this.value):this.value="",this.value!==o&&(this.emit("sl-change"),this.emit("sl-input"))}handleInputInput(e){this.formControlController.updateValidity(),e.stopPropagation()}handleInputKeyDown(e){if(e.key==="Enter"){const t=this.value;this.input.value?(this.setColor(this.input.value),this.input.value=this.value,this.value!==t&&(this.emit("sl-change"),this.emit("sl-input")),setTimeout(()=>this.input.select())):this.hue=0}}handleInputInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}handleTouchMove(e){e.preventDefault()}parseColor(e){const t=new rt(e);if(!t.isValid)return null;const o=t.toHsl(),i={h:o.h,s:o.s*100,l:o.l*100,a:o.a},s=t.toRgb(),r=t.toHexString(),a=t.toHex8String(),n=t.toHsv(),c={h:n.h,s:n.s*100,v:n.v*100,a:n.a};return{hsl:{h:i.h,s:i.s,l:i.l,string:this.setLetterCase(`hsl(${Math.round(i.h)}, ${Math.round(i.s)}%, ${Math.round(i.l)}%)`)},hsla:{h:i.h,s:i.s,l:i.l,a:i.a,string:this.setLetterCase(`hsla(${Math.round(i.h)}, ${Math.round(i.s)}%, ${Math.round(i.l)}%, ${i.a.toFixed(2).toString()})`)},hsv:{h:c.h,s:c.s,v:c.v,string:this.setLetterCase(`hsv(${Math.round(c.h)}, ${Math.round(c.s)}%, ${Math.round(c.v)}%)`)},hsva:{h:c.h,s:c.s,v:c.v,a:c.a,string:this.setLetterCase(`hsva(${Math.round(c.h)}, ${Math.round(c.s)}%, ${Math.round(c.v)}%, ${c.a.toFixed(2).toString()})`)},rgb:{r:s.r,g:s.g,b:s.b,string:this.setLetterCase(`rgb(${Math.round(s.r)}, ${Math.round(s.g)}, ${Math.round(s.b)})`)},rgba:{r:s.r,g:s.g,b:s.b,a:s.a,string:this.setLetterCase(`rgba(${Math.round(s.r)}, ${Math.round(s.g)}, ${Math.round(s.b)}, ${s.a.toFixed(2).toString()})`)},hex:this.setLetterCase(r),hexa:this.setLetterCase(a)}}setColor(e){const t=this.parseColor(e);return t===null?!1:(this.hue=t.hsva.h,this.saturation=t.hsva.s,this.brightness=t.hsva.v,this.alpha=this.opacity?t.hsva.a*100:100,this.syncValues(),!0)}setLetterCase(e){return typeof e!="string"?"":this.uppercase?e.toUpperCase():e.toLowerCase()}async syncValues(){const e=this.parseColor(`hsva(${this.hue}, ${this.saturation}%, ${this.brightness}%, ${this.alpha/100})`);e!==null&&(this.format==="hsl"?this.inputValue=this.opacity?e.hsla.string:e.hsl.string:this.format==="rgb"?this.inputValue=this.opacity?e.rgba.string:e.rgb.string:this.format==="hsv"?this.inputValue=this.opacity?e.hsva.string:e.hsv.string:this.inputValue=this.opacity?e.hexa:e.hex,this.isSafeValue=!0,this.value=this.inputValue,await this.updateComplete,this.isSafeValue=!1)}handleAfterHide(){this.previewButton.classList.remove("color-picker__preview-color--copied")}handleEyeDropper(){if(!Tn)return;new EyeDropper().open().then(t=>{const o=this.value;this.setColor(t.sRGBHex),this.value!==o&&(this.emit("sl-change"),this.emit("sl-input"))}).catch(()=>{})}selectSwatch(e){const t=this.value;this.disabled||(this.setColor(e),this.value!==t&&(this.emit("sl-change"),this.emit("sl-input")))}getHexString(e,t,o,i=100){const s=new rt(`hsva(${e}, ${t}%, ${o}%, ${i/100})`);return s.isValid?s.toHex8String():""}stopNestedEventPropagation(e){e.stopImmediatePropagation()}handleFormatChange(){this.syncValues()}handleOpacityChange(){this.alpha=100}handleValueChange(e,t){if(this.isEmpty=!t,t||(this.hue=0,this.saturation=0,this.brightness=100,this.alpha=100),!this.isSafeValue){const o=this.parseColor(t);o!==null?(this.inputValue=this.value,this.hue=o.hsva.h,this.saturation=o.hsva.s,this.brightness=o.hsva.v,this.alpha=o.hsva.a*100,this.syncValues()):this.inputValue=e??""}}focus(e){this.inline?this.base.focus(e):this.trigger.focus(e)}blur(){var e;const t=this.inline?this.base:this.trigger;this.hasFocus&&(t.focus({preventScroll:!0}),t.blur()),(e=this.dropdown)!=null&&e.open&&this.dropdown.hide()}getFormattedValue(e="hex"){const t=this.parseColor(`hsva(${this.hue}, ${this.saturation}%, ${this.brightness}%, ${this.alpha/100})`);if(t===null)return"";switch(e){case"hex":return t.hex;case"hexa":return t.hexa;case"rgb":return t.rgb.string;case"rgba":return t.rgba.string;case"hsl":return t.hsl.string;case"hsla":return t.hsla.string;case"hsv":return t.hsv.string;case"hsva":return t.hsva.string;default:return""}}checkValidity(){return this.input.checkValidity()}getForm(){return this.formControlController.getForm()}reportValidity(){return!this.inline&&!this.validity.valid?(this.dropdown.show(),this.addEventListener("sl-after-show",()=>this.input.reportValidity(),{once:!0}),this.disabled||this.formControlController.emitInvalidEvent(),!1):this.input.reportValidity()}setCustomValidity(e){this.input.setCustomValidity(e),this.formControlController.updateValidity()}render(){const e=this.saturation,t=100-this.brightness,o=Array.isArray(this.swatches)?this.swatches:this.swatches.split(";").filter(s=>s.trim()!==""),i=x`
      <div
        part="base"
        class=${R({"color-picker":!0,"color-picker--inline":this.inline,"color-picker--disabled":this.disabled,"color-picker--focused":this.hasFocus})}
        aria-disabled=${this.disabled?"true":"false"}
        aria-labelledby="label"
        tabindex=${this.inline?"0":"-1"}
      >
        ${this.inline?x`
              <sl-visually-hidden id="label">
                <slot name="label">${this.label}</slot>
              </sl-visually-hidden>
            `:null}

        <div
          part="grid"
          class="color-picker__grid"
          style=${Rt({backgroundColor:this.getHexString(this.hue,100,100)})}
          @pointerdown=${this.handleGridDrag}
          @touchmove=${this.handleTouchMove}
        >
          <span
            part="grid-handle"
            class=${R({"color-picker__grid-handle":!0,"color-picker__grid-handle--dragging":this.isDraggingGridHandle})}
            style=${Rt({top:`${t}%`,left:`${e}%`,backgroundColor:this.getHexString(this.hue,this.saturation,this.brightness,this.alpha)})}
            role="application"
            aria-label="HSV"
            tabindex=${E(this.disabled?void 0:"0")}
            @keydown=${this.handleGridKeyDown}
          ></span>
        </div>

        <div class="color-picker__controls">
          <div class="color-picker__sliders">
            <div
              part="slider hue-slider"
              class="color-picker__hue color-picker__slider"
              @pointerdown=${this.handleHueDrag}
              @touchmove=${this.handleTouchMove}
            >
              <span
                part="slider-handle hue-slider-handle"
                class="color-picker__slider-handle"
                style=${Rt({left:`${this.hue===0?0:100/(360/this.hue)}%`})}
                role="slider"
                aria-label="hue"
                aria-orientation="horizontal"
                aria-valuemin="0"
                aria-valuemax="360"
                aria-valuenow=${`${Math.round(this.hue)}`}
                tabindex=${E(this.disabled?void 0:"0")}
                @keydown=${this.handleHueKeyDown}
              ></span>
            </div>

            ${this.opacity?x`
                  <div
                    part="slider opacity-slider"
                    class="color-picker__alpha color-picker__slider color-picker__transparent-bg"
                    @pointerdown="${this.handleAlphaDrag}"
                    @touchmove=${this.handleTouchMove}
                  >
                    <div
                      class="color-picker__alpha-gradient"
                      style=${Rt({backgroundImage:`linear-gradient(
                          to right,
                          ${this.getHexString(this.hue,this.saturation,this.brightness,0)} 0%,
                          ${this.getHexString(this.hue,this.saturation,this.brightness,100)} 100%
                        )`})}
                    ></div>
                    <span
                      part="slider-handle opacity-slider-handle"
                      class="color-picker__slider-handle"
                      style=${Rt({left:`${this.alpha}%`})}
                      role="slider"
                      aria-label="alpha"
                      aria-orientation="horizontal"
                      aria-valuemin="0"
                      aria-valuemax="100"
                      aria-valuenow=${Math.round(this.alpha)}
                      tabindex=${E(this.disabled?void 0:"0")}
                      @keydown=${this.handleAlphaKeyDown}
                    ></span>
                  </div>
                `:""}
          </div>

          <button
            type="button"
            part="preview"
            class="color-picker__preview color-picker__transparent-bg"
            aria-label=${this.localize.term("copy")}
            style=${Rt({"--preview-color":this.getHexString(this.hue,this.saturation,this.brightness,this.alpha)})}
            @click=${this.handleCopy}
          ></button>
        </div>

        <div class="color-picker__user-input" aria-live="polite">
          <sl-input
            part="input"
            type="text"
            name=${this.name}
            autocomplete="off"
            autocorrect="off"
            autocapitalize="off"
            spellcheck="false"
            value=${this.isEmpty?"":this.inputValue}
            ?required=${this.required}
            ?disabled=${this.disabled}
            aria-label=${this.localize.term("currentValue")}
            @keydown=${this.handleInputKeyDown}
            @sl-change=${this.handleInputChange}
            @sl-input=${this.handleInputInput}
            @sl-invalid=${this.handleInputInvalid}
            @sl-blur=${this.stopNestedEventPropagation}
            @sl-focus=${this.stopNestedEventPropagation}
          ></sl-input>

          <sl-button-group>
            ${this.noFormatToggle?"":x`
                  <sl-button
                    part="format-button"
                    aria-label=${this.localize.term("toggleColorFormat")}
                    exportparts="
                      base:format-button__base,
                      prefix:format-button__prefix,
                      label:format-button__label,
                      suffix:format-button__suffix,
                      caret:format-button__caret
                    "
                    @click=${this.handleFormatToggle}
                    @sl-blur=${this.stopNestedEventPropagation}
                    @sl-focus=${this.stopNestedEventPropagation}
                  >
                    ${this.setLetterCase(this.format)}
                  </sl-button>
                `}
            ${Tn?x`
                  <sl-button
                    part="eye-dropper-button"
                    exportparts="
                      base:eye-dropper-button__base,
                      prefix:eye-dropper-button__prefix,
                      label:eye-dropper-button__label,
                      suffix:eye-dropper-button__suffix,
                      caret:eye-dropper-button__caret
                    "
                    @click=${this.handleEyeDropper}
                    @sl-blur=${this.stopNestedEventPropagation}
                    @sl-focus=${this.stopNestedEventPropagation}
                  >
                    <sl-icon
                      library="system"
                      name="eyedropper"
                      label=${this.localize.term("selectAColorFromTheScreen")}
                    ></sl-icon>
                  </sl-button>
                `:""}
          </sl-button-group>
        </div>

        ${o.length>0?x`
              <div part="swatches" class="color-picker__swatches">
                ${o.map(s=>{const r=this.parseColor(s);return r?x`
                    <div
                      part="swatch"
                      class="color-picker__swatch color-picker__transparent-bg"
                      tabindex=${E(this.disabled?void 0:"0")}
                      role="button"
                      aria-label=${s}
                      @click=${()=>this.selectSwatch(s)}
                      @keydown=${a=>!this.disabled&&a.key==="Enter"&&this.setColor(r.hexa)}
                    >
                      <div
                        class="color-picker__swatch-color"
                        style=${Rt({backgroundColor:r.hexa})}
                      ></div>
                    </div>
                  `:(console.error(`Unable to parse swatch color: "${s}"`,this),"")})}
              </div>
            `:""}
      </div>
    `;return this.inline?i:x`
      <sl-dropdown
        class="color-dropdown"
        aria-disabled=${this.disabled?"true":"false"}
        .containingElement=${this}
        ?disabled=${this.disabled}
        ?hoist=${this.hoist}
        @sl-after-hide=${this.handleAfterHide}
      >
        <button
          part="trigger"
          slot="trigger"
          class=${R({"color-dropdown__trigger":!0,"color-dropdown__trigger--disabled":this.disabled,"color-dropdown__trigger--small":this.size==="small","color-dropdown__trigger--medium":this.size==="medium","color-dropdown__trigger--large":this.size==="large","color-dropdown__trigger--empty":this.isEmpty,"color-dropdown__trigger--focused":this.hasFocus,"color-picker__transparent-bg":!0})}
          style=${Rt({color:this.getHexString(this.hue,this.saturation,this.brightness,this.alpha)})}
          type="button"
        >
          <sl-visually-hidden>
            <slot name="label">${this.label}</slot>
          </sl-visually-hidden>
        </button>
        ${i}
      </sl-dropdown>
    `}};H.styles=[D,jf];H.dependencies={"sl-button-group":mo,"sl-button":X,"sl-dropdown":St,"sl-icon":J,"sl-input":V,"sl-visually-hidden":pa};l([S('[part~="base"]')],H.prototype,"base",2);l([S('[part~="input"]')],H.prototype,"input",2);l([S(".color-dropdown")],H.prototype,"dropdown",2);l([S('[part~="preview"]')],H.prototype,"previewButton",2);l([S('[part~="trigger"]')],H.prototype,"trigger",2);l([z()],H.prototype,"hasFocus",2);l([z()],H.prototype,"isDraggingGridHandle",2);l([z()],H.prototype,"isEmpty",2);l([z()],H.prototype,"inputValue",2);l([z()],H.prototype,"hue",2);l([z()],H.prototype,"saturation",2);l([z()],H.prototype,"brightness",2);l([z()],H.prototype,"alpha",2);l([p()],H.prototype,"value",2);l([Io()],H.prototype,"defaultValue",2);l([p()],H.prototype,"label",2);l([p()],H.prototype,"format",2);l([p({type:Boolean,reflect:!0})],H.prototype,"inline",2);l([p({reflect:!0})],H.prototype,"size",2);l([p({attribute:"no-format-toggle",type:Boolean})],H.prototype,"noFormatToggle",2);l([p()],H.prototype,"name",2);l([p({type:Boolean,reflect:!0})],H.prototype,"disabled",2);l([p({type:Boolean})],H.prototype,"hoist",2);l([p({type:Boolean})],H.prototype,"opacity",2);l([p({type:Boolean})],H.prototype,"uppercase",2);l([p()],H.prototype,"swatches",2);l([p({reflect:!0})],H.prototype,"form",2);l([p({type:Boolean,reflect:!0})],H.prototype,"required",2);l([Ai({passive:!1})],H.prototype,"handleTouchMove",1);l([C("format",{waitUntilFirstUpdate:!0})],H.prototype,"handleFormatChange",1);l([C("opacity",{waitUntilFirstUpdate:!0})],H.prototype,"handleOpacityChange",1);l([C("value")],H.prototype,"handleValueChange",1);H.define("sl-color-picker");var sm=O`
  :host {
    --border-color: var(--sl-color-neutral-200);
    --border-radius: var(--sl-border-radius-medium);
    --border-width: 1px;
    --padding: var(--sl-spacing-large);

    display: inline-block;
  }

  .card {
    display: flex;
    flex-direction: column;
    background-color: var(--sl-panel-background-color);
    box-shadow: var(--sl-shadow-x-small);
    border: solid var(--border-width) var(--border-color);
    border-radius: var(--border-radius);
  }

  .card__image {
    display: flex;
    border-top-left-radius: var(--border-radius);
    border-top-right-radius: var(--border-radius);
    margin: calc(-1 * var(--border-width));
    overflow: hidden;
  }

  .card__image::slotted(img) {
    display: block;
    width: 100%;
  }

  .card:not(.card--has-image) .card__image {
    display: none;
  }

  .card__header {
    display: block;
    border-bottom: solid var(--border-width) var(--border-color);
    padding: calc(var(--padding) / 2) var(--padding);
  }

  .card:not(.card--has-header) .card__header {
    display: none;
  }

  .card:not(.card--has-image) .card__header {
    border-top-left-radius: var(--border-radius);
    border-top-right-radius: var(--border-radius);
  }

  .card__body {
    display: block;
    padding: var(--padding);
  }

  .card--has-footer .card__footer {
    display: block;
    border-top: solid var(--border-width) var(--border-color);
    padding: var(--padding);
  }

  .card:not(.card--has-footer) .card__footer {
    display: none;
  }
`,rc=class extends T{constructor(){super(...arguments),this.hasSlotController=new Ot(this,"footer","header","image")}render(){return x`
      <div
        part="base"
        class=${R({card:!0,"card--has-footer":this.hasSlotController.test("footer"),"card--has-image":this.hasSlotController.test("image"),"card--has-header":this.hasSlotController.test("header")})}
      >
        <slot name="image" part="image" class="card__image"></slot>
        <slot name="header" part="header" class="card__header"></slot>
        <slot part="body" class="card__body"></slot>
        <slot name="footer" part="footer" class="card__footer"></slot>
      </div>
    `}};rc.styles=[D,sm];rc.define("sl-card");var rm=class{constructor(e,t){this.timerId=0,this.activeInteractions=0,this.paused=!1,this.stopped=!0,this.pause=()=>{this.activeInteractions++||(this.paused=!0,this.host.requestUpdate())},this.resume=()=>{--this.activeInteractions||(this.paused=!1,this.host.requestUpdate())},e.addController(this),this.host=e,this.tickCallback=t}hostConnected(){this.host.addEventListener("mouseenter",this.pause),this.host.addEventListener("mouseleave",this.resume),this.host.addEventListener("focusin",this.pause),this.host.addEventListener("focusout",this.resume),this.host.addEventListener("touchstart",this.pause,{passive:!0}),this.host.addEventListener("touchend",this.resume)}hostDisconnected(){this.stop(),this.host.removeEventListener("mouseenter",this.pause),this.host.removeEventListener("mouseleave",this.resume),this.host.removeEventListener("focusin",this.pause),this.host.removeEventListener("focusout",this.resume),this.host.removeEventListener("touchstart",this.pause),this.host.removeEventListener("touchend",this.resume)}start(e){this.stop(),this.stopped=!1,this.timerId=window.setInterval(()=>{this.paused||this.tickCallback()},e)}stop(){clearInterval(this.timerId),this.stopped=!0,this.host.requestUpdate()}},am=O`
  :host {
    --slide-gap: var(--sl-spacing-medium, 1rem);
    --aspect-ratio: 16 / 9;
    --scroll-hint: 0px;

    display: flex;
  }

  .carousel {
    display: grid;
    grid-template-columns: min-content 1fr min-content;
    grid-template-rows: 1fr min-content;
    grid-template-areas:
      '. slides .'
      '. pagination .';
    gap: var(--sl-spacing-medium);
    align-items: center;
    min-height: 100%;
    min-width: 100%;
    position: relative;
  }

  .carousel__pagination {
    grid-area: pagination;
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: var(--sl-spacing-small);
  }

  .carousel__slides {
    grid-area: slides;

    display: grid;
    height: 100%;
    width: 100%;
    align-items: center;
    justify-items: center;
    overflow: auto;
    overscroll-behavior-x: contain;
    scrollbar-width: none;
    aspect-ratio: calc(var(--aspect-ratio) * var(--slides-per-page));
    border-radius: var(--sl-border-radius-small);

    --slide-size: calc((100% - (var(--slides-per-page) - 1) * var(--slide-gap)) / var(--slides-per-page));
  }

  @media (prefers-reduced-motion) {
    :where(.carousel__slides) {
      scroll-behavior: auto;
    }
  }

  .carousel__slides--horizontal {
    grid-auto-flow: column;
    grid-auto-columns: var(--slide-size);
    grid-auto-rows: 100%;
    column-gap: var(--slide-gap);
    scroll-snap-type: x mandatory;
    scroll-padding-inline: var(--scroll-hint);
    padding-inline: var(--scroll-hint);
    overflow-y: hidden;
  }

  .carousel__slides--vertical {
    grid-auto-flow: row;
    grid-auto-columns: 100%;
    grid-auto-rows: var(--slide-size);
    row-gap: var(--slide-gap);
    scroll-snap-type: y mandatory;
    scroll-padding-block: var(--scroll-hint);
    padding-block: var(--scroll-hint);
    overflow-x: hidden;
  }

  .carousel__slides--dragging {
  }

  :host([vertical]) ::slotted(sl-carousel-item) {
    height: 100%;
  }

  .carousel__slides::-webkit-scrollbar {
    display: none;
  }

  .carousel__navigation {
    grid-area: navigation;
    display: contents;
    font-size: var(--sl-font-size-x-large);
  }

  .carousel__navigation-button {
    flex: 0 0 auto;
    display: flex;
    align-items: center;
    background: none;
    border: none;
    border-radius: var(--sl-border-radius-small);
    font-size: inherit;
    color: var(--sl-color-neutral-600);
    padding: var(--sl-spacing-x-small);
    cursor: pointer;
    transition: var(--sl-transition-medium) color;
    appearance: none;
  }

  .carousel__navigation-button--disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .carousel__navigation-button--disabled::part(base) {
    pointer-events: none;
  }

  .carousel__navigation-button--previous {
    grid-column: 1;
    grid-row: 1;
  }

  .carousel__navigation-button--next {
    grid-column: 3;
    grid-row: 1;
  }

  .carousel__pagination-item {
    display: block;
    cursor: pointer;
    background: none;
    border: 0;
    border-radius: var(--sl-border-radius-circle);
    width: var(--sl-spacing-small);
    height: var(--sl-spacing-small);
    background-color: var(--sl-color-neutral-300);
    padding: 0;
    margin: 0;
  }

  .carousel__pagination-item--active {
    background-color: var(--sl-color-neutral-700);
    transform: scale(1.2);
  }

  /* Focus styles */
  .carousel__slides:focus-visible,
  .carousel__navigation-button:focus-visible,
  .carousel__pagination-item:focus-visible {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }
`;/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function*nm(e,t){if(e!==void 0){let o=0;for(const i of e)yield t(i,o++)}}/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function*lm(e,t,o=1){const i=t===void 0?0:e;t??(t=e);for(let s=i;o>0?s<t:t<s;s+=o)yield s}var ct=class extends T{constructor(){super(...arguments),this.loop=!1,this.navigation=!1,this.pagination=!1,this.autoplay=!1,this.autoplayInterval=3e3,this.slidesPerPage=1,this.slidesPerMove=1,this.orientation="horizontal",this.mouseDragging=!1,this.activeSlide=0,this.scrolling=!1,this.dragging=!1,this.autoplayController=new rm(this,()=>this.next()),this.dragStartPosition=[-1,-1],this.localize=new j(this),this.pendingSlideChange=!1,this.handleMouseDrag=e=>{this.dragging||(this.scrollContainer.style.setProperty("scroll-snap-type","none"),this.dragging=!0,this.dragStartPosition=[e.clientX,e.clientY]),this.scrollContainer.scrollBy({left:-e.movementX,top:-e.movementY,behavior:"instant"})},this.handleMouseDragEnd=()=>{const e=this.scrollContainer;document.removeEventListener("pointermove",this.handleMouseDrag,{capture:!0});const t=e.scrollLeft,o=e.scrollTop;e.style.removeProperty("scroll-snap-type"),e.style.setProperty("overflow","hidden");const i=e.scrollLeft,s=e.scrollTop;e.style.removeProperty("overflow"),e.style.setProperty("scroll-snap-type","none"),e.scrollTo({left:t,top:o,behavior:"instant"}),requestAnimationFrame(async()=>{(t!==i||o!==s)&&(e.scrollTo({left:i,top:s,behavior:vr()?"auto":"smooth"}),await Pt(e,"scrollend")),e.style.removeProperty("scroll-snap-type"),this.dragging=!1,this.dragStartPosition=[-1,-1],this.handleScrollEnd()})},this.handleSlotChange=e=>{e.some(o=>[...o.addedNodes,...o.removedNodes].some(i=>this.isCarouselItem(i)&&!i.hasAttribute("data-clone")))&&this.initializeSlides(),this.requestUpdate()}}connectedCallback(){super.connectedCallback(),this.setAttribute("role","region"),this.setAttribute("aria-label",this.localize.term("carousel"))}disconnectedCallback(){var e;super.disconnectedCallback(),(e=this.mutationObserver)==null||e.disconnect()}firstUpdated(){this.initializeSlides(),this.mutationObserver=new MutationObserver(this.handleSlotChange),this.mutationObserver.observe(this,{childList:!0,subtree:!0})}willUpdate(e){(e.has("slidesPerMove")||e.has("slidesPerPage"))&&(this.slidesPerMove=Math.min(this.slidesPerMove,this.slidesPerPage))}getPageCount(){const e=this.getSlides().length,{slidesPerPage:t,slidesPerMove:o,loop:i}=this,s=i?e/o:(e-t)/o+1;return Math.ceil(s)}getCurrentPage(){return Math.ceil(this.activeSlide/this.slidesPerMove)}canScrollNext(){return this.loop||this.getCurrentPage()<this.getPageCount()-1}canScrollPrev(){return this.loop||this.getCurrentPage()>0}getSlides({excludeClones:e=!0}={}){return[...this.children].filter(t=>this.isCarouselItem(t)&&(!e||!t.hasAttribute("data-clone")))}handleClick(e){if(this.dragging&&this.dragStartPosition[0]>0&&this.dragStartPosition[1]>0){const t=Math.abs(this.dragStartPosition[0]-e.clientX),o=Math.abs(this.dragStartPosition[1]-e.clientY);Math.sqrt(t*t+o*o)>=10&&e.preventDefault()}}handleKeyDown(e){if(["ArrowLeft","ArrowRight","ArrowUp","ArrowDown","Home","End"].includes(e.key)){const t=e.target,o=this.localize.dir()==="rtl",i=t.closest('[part~="pagination-item"]')!==null,s=e.key==="ArrowDown"||!o&&e.key==="ArrowRight"||o&&e.key==="ArrowLeft",r=e.key==="ArrowUp"||!o&&e.key==="ArrowLeft"||o&&e.key==="ArrowRight";e.preventDefault(),r&&this.previous(),s&&this.next(),e.key==="Home"&&this.goToSlide(0),e.key==="End"&&this.goToSlide(this.getSlides().length-1),i&&this.updateComplete.then(()=>{var a;const n=(a=this.shadowRoot)==null?void 0:a.querySelector('[part~="pagination-item--active"]');n&&n.focus()})}}handleMouseDragStart(e){this.mouseDragging&&e.button===0&&(e.preventDefault(),document.addEventListener("pointermove",this.handleMouseDrag,{capture:!0,passive:!0}),document.addEventListener("pointerup",this.handleMouseDragEnd,{capture:!0,once:!0}))}handleScroll(){this.scrolling=!0,this.pendingSlideChange||this.synchronizeSlides()}synchronizeSlides(){const e=new IntersectionObserver(t=>{e.disconnect();for(const n of t){const c=n.target;c.toggleAttribute("inert",!n.isIntersecting),c.classList.toggle("--in-view",n.isIntersecting),c.setAttribute("aria-hidden",n.isIntersecting?"false":"true")}const o=t.find(n=>n.isIntersecting);if(!o)return;const i=this.getSlides({excludeClones:!1}),s=this.getSlides().length,r=i.indexOf(o.target),a=this.loop?r-this.slidesPerPage:r;if(this.activeSlide=(Math.ceil(a/this.slidesPerMove)*this.slidesPerMove+s)%s,!this.scrolling&&this.loop&&o.target.hasAttribute("data-clone")){const n=Number(o.target.getAttribute("data-clone"));this.goToSlide(n,"instant")}},{root:this.scrollContainer,threshold:.6});this.getSlides({excludeClones:!1}).forEach(t=>{e.observe(t)})}handleScrollEnd(){!this.scrolling||this.dragging||(this.scrolling=!1,this.pendingSlideChange=!1,this.synchronizeSlides())}isCarouselItem(e){return e instanceof Element&&e.tagName.toLowerCase()==="sl-carousel-item"}initializeSlides(){this.getSlides({excludeClones:!1}).forEach((e,t)=>{e.classList.remove("--in-view"),e.classList.remove("--is-active"),e.setAttribute("role","group"),e.setAttribute("aria-label",this.localize.term("slideNum",t+1)),this.pagination&&(e.setAttribute("id",`slide-${t+1}`),e.setAttribute("role","tabpanel"),e.removeAttribute("aria-label"),e.setAttribute("aria-labelledby",`tab-${t+1}`)),e.hasAttribute("data-clone")&&e.remove()}),this.updateSlidesSnap(),this.loop&&this.createClones(),this.goToSlide(this.activeSlide,"auto"),this.synchronizeSlides()}createClones(){const e=this.getSlides(),t=this.slidesPerPage,o=e.slice(-t),i=e.slice(0,t);o.reverse().forEach((s,r)=>{const a=s.cloneNode(!0);a.setAttribute("data-clone",String(e.length-r-1)),this.prepend(a)}),i.forEach((s,r)=>{const a=s.cloneNode(!0);a.setAttribute("data-clone",String(r)),this.append(a)})}handleSlideChange(){const e=this.getSlides();e.forEach((t,o)=>{t.classList.toggle("--is-active",o===this.activeSlide)}),this.hasUpdated&&this.emit("sl-slide-change",{detail:{index:this.activeSlide,slide:e[this.activeSlide]}})}updateSlidesSnap(){const e=this.getSlides(),t=this.slidesPerMove;e.forEach((o,i)=>{(i+t)%t===0?o.style.removeProperty("scroll-snap-align"):o.style.setProperty("scroll-snap-align","none")})}handleAutoplayChange(){this.autoplayController.stop(),this.autoplay&&this.autoplayController.start(this.autoplayInterval)}previous(e="smooth"){this.goToSlide(this.activeSlide-this.slidesPerMove,e)}next(e="smooth"){this.goToSlide(this.activeSlide+this.slidesPerMove,e)}goToSlide(e,t="smooth"){const{slidesPerPage:o,loop:i}=this,s=this.getSlides(),r=this.getSlides({excludeClones:!1});if(!s.length)return;const a=i?(e+s.length)%s.length:ht(e,0,s.length-o);this.activeSlide=a;const n=this.localize.dir()==="rtl",c=ht(e+(i?o:0)+(n?o-1:0),0,r.length-1),d=r[c];this.scrollToSlide(d,vr()?"auto":t)}scrollToSlide(e,t="smooth"){this.pendingSlideChange=!0,window.requestAnimationFrame(()=>{if(!this.scrollContainer)return;const o=this.scrollContainer,i=o.getBoundingClientRect(),s=e.getBoundingClientRect(),r=s.left-i.left,a=s.top-i.top;r||a?(this.pendingSlideChange=!0,o.scrollTo({left:r+o.scrollLeft,top:a+o.scrollTop,behavior:t})):this.pendingSlideChange=!1})}render(){const{slidesPerMove:e,scrolling:t}=this,o=this.getPageCount(),i=this.getCurrentPage(),s=this.canScrollPrev(),r=this.canScrollNext(),a=this.localize.dir()==="ltr";return x`
      <div part="base" class="carousel">
        <div
          id="scroll-container"
          part="scroll-container"
          class="${R({carousel__slides:!0,"carousel__slides--horizontal":this.orientation==="horizontal","carousel__slides--vertical":this.orientation==="vertical","carousel__slides--dragging":this.dragging})}"
          style="--slides-per-page: ${this.slidesPerPage};"
          aria-busy="${t?"true":"false"}"
          aria-atomic="true"
          tabindex="0"
          @keydown=${this.handleKeyDown}
          @mousedown="${this.handleMouseDragStart}"
          @scroll="${this.handleScroll}"
          @scrollend=${this.handleScrollEnd}
          @click=${this.handleClick}
        >
          <slot></slot>
        </div>

        ${this.navigation?x`
              <div part="navigation" class="carousel__navigation">
                <button
                  part="navigation-button navigation-button--previous"
                  class="${R({"carousel__navigation-button":!0,"carousel__navigation-button--previous":!0,"carousel__navigation-button--disabled":!s})}"
                  aria-label="${this.localize.term("previousSlide")}"
                  aria-controls="scroll-container"
                  aria-disabled="${s?"false":"true"}"
                  @click=${s?()=>this.previous():null}
                >
                  <slot name="previous-icon">
                    <sl-icon library="system" name="${a?"chevron-left":"chevron-right"}"></sl-icon>
                  </slot>
                </button>

                <button
                  part="navigation-button navigation-button--next"
                  class=${R({"carousel__navigation-button":!0,"carousel__navigation-button--next":!0,"carousel__navigation-button--disabled":!r})}
                  aria-label="${this.localize.term("nextSlide")}"
                  aria-controls="scroll-container"
                  aria-disabled="${r?"false":"true"}"
                  @click=${r?()=>this.next():null}
                >
                  <slot name="next-icon">
                    <sl-icon library="system" name="${a?"chevron-right":"chevron-left"}"></sl-icon>
                  </slot>
                </button>
              </div>
            `:""}
        ${this.pagination?x`
              <div part="pagination" role="tablist" class="carousel__pagination">
                ${nm(lm(o),n=>{const c=n===i;return x`
                    <button
                      part="pagination-item ${c?"pagination-item--active":""}"
                      class="${R({"carousel__pagination-item":!0,"carousel__pagination-item--active":c})}"
                      role="tab"
                      id="tab-${n+1}"
                      aria-controls="slide-${n+1}"
                      aria-selected="${c?"true":"false"}"
                      aria-label="${c?this.localize.term("slideNum",n+1):this.localize.term("goToSlide",n+1,o)}"
                      tabindex=${c?"0":"-1"}
                      @click=${()=>this.goToSlide(n*e)}
                      @keydown=${this.handleKeyDown}
                    ></button>
                  `})}
              </div>
            `:""}
      </div>
    `}};ct.styles=[D,am];ct.dependencies={"sl-icon":J};l([p({type:Boolean,reflect:!0})],ct.prototype,"loop",2);l([p({type:Boolean,reflect:!0})],ct.prototype,"navigation",2);l([p({type:Boolean,reflect:!0})],ct.prototype,"pagination",2);l([p({type:Boolean,reflect:!0})],ct.prototype,"autoplay",2);l([p({type:Number,attribute:"autoplay-interval"})],ct.prototype,"autoplayInterval",2);l([p({type:Number,attribute:"slides-per-page"})],ct.prototype,"slidesPerPage",2);l([p({type:Number,attribute:"slides-per-move"})],ct.prototype,"slidesPerMove",2);l([p()],ct.prototype,"orientation",2);l([p({type:Boolean,reflect:!0,attribute:"mouse-dragging"})],ct.prototype,"mouseDragging",2);l([S(".carousel__slides")],ct.prototype,"scrollContainer",2);l([S(".carousel__pagination")],ct.prototype,"paginationContainer",2);l([z()],ct.prototype,"activeSlide",2);l([z()],ct.prototype,"scrolling",2);l([z()],ct.prototype,"dragging",2);l([Ai({passive:!0})],ct.prototype,"handleScroll",1);l([C("loop",{waitUntilFirstUpdate:!0}),C("slidesPerPage",{waitUntilFirstUpdate:!0})],ct.prototype,"initializeSlides",1);l([C("activeSlide")],ct.prototype,"handleSlideChange",1);l([C("slidesPerMove")],ct.prototype,"updateSlidesSnap",1);l([C("autoplay")],ct.prototype,"handleAutoplayChange",1);ct.define("sl-carousel");var cm=(e,t)=>{let o=0;return function(...i){window.clearTimeout(o),o=window.setTimeout(()=>{e.call(this,...i)},t)}},Pn=(e,t,o)=>{const i=e[t];e[t]=function(...s){i.call(this,...s),o.call(this,i,...s)}};(()=>{if(typeof window>"u")return;if(!("onscrollend"in window)){const t=new Set,o=new WeakMap,i=r=>{for(const a of r.changedTouches)t.add(a.identifier)},s=r=>{for(const a of r.changedTouches)t.delete(a.identifier)};document.addEventListener("touchstart",i,!0),document.addEventListener("touchend",s,!0),document.addEventListener("touchcancel",s,!0),Pn(EventTarget.prototype,"addEventListener",function(r,a){if(a!=="scrollend")return;const n=cm(()=>{t.size?n():this.dispatchEvent(new Event("scrollend"))},100);r.call(this,"scroll",n,{passive:!0}),o.set(this,n)}),Pn(EventTarget.prototype,"removeEventListener",function(r,a){if(a!=="scrollend")return;const n=o.get(this);n&&r.call(this,"scroll",n,{passive:!0})})}})();var dm=O`
  :host {
    --aspect-ratio: inherit;

    display: flex;
    align-items: center;
    justify-content: center;
    flex-direction: column;
    width: 100%;
    max-height: 100%;
    aspect-ratio: var(--aspect-ratio);
    scroll-snap-align: start;
    scroll-snap-stop: always;
  }

  ::slotted(img) {
    width: 100% !important;
    height: 100% !important;
    object-fit: cover;
  }
`,ac=class extends T{connectedCallback(){super.connectedCallback()}render(){return x` <slot></slot> `}};ac.styles=[D,dm];ac.define("sl-carousel-item");var hm=O`
  :host {
    display: inline-flex;
  }

  .breadcrumb-item {
    display: inline-flex;
    align-items: center;
    font-family: var(--sl-font-sans);
    font-size: var(--sl-font-size-small);
    font-weight: var(--sl-font-weight-semibold);
    color: var(--sl-color-neutral-600);
    line-height: var(--sl-line-height-normal);
    white-space: nowrap;
  }

  .breadcrumb-item__label {
    display: inline-block;
    font-family: inherit;
    font-size: inherit;
    font-weight: inherit;
    line-height: inherit;
    text-decoration: none;
    color: inherit;
    background: none;
    border: none;
    border-radius: var(--sl-border-radius-medium);
    padding: 0;
    margin: 0;
    cursor: pointer;
    transition: var(--sl-transition-fast) --color;
  }

  :host(:not(:last-of-type)) .breadcrumb-item__label {
    color: var(--sl-color-primary-600);
  }

  :host(:not(:last-of-type)) .breadcrumb-item__label:hover {
    color: var(--sl-color-primary-500);
  }

  :host(:not(:last-of-type)) .breadcrumb-item__label:active {
    color: var(--sl-color-primary-600);
  }

  .breadcrumb-item__label:focus {
    outline: none;
  }

  .breadcrumb-item__label:focus-visible {
    outline: var(--sl-focus-ring);
    outline-offset: var(--sl-focus-ring-offset);
  }

  .breadcrumb-item__prefix,
  .breadcrumb-item__suffix {
    display: none;
    flex: 0 0 auto;
    display: flex;
    align-items: center;
  }

  .breadcrumb-item--has-prefix .breadcrumb-item__prefix {
    display: inline-flex;
    margin-inline-end: var(--sl-spacing-x-small);
  }

  .breadcrumb-item--has-suffix .breadcrumb-item__suffix {
    display: inline-flex;
    margin-inline-start: var(--sl-spacing-x-small);
  }

  :host(:last-of-type) .breadcrumb-item__separator {
    display: none;
  }

  .breadcrumb-item__separator {
    display: inline-flex;
    align-items: center;
    margin: 0 var(--sl-spacing-x-small);
    user-select: none;
    -webkit-user-select: none;
  }
`,Ne=class extends T{constructor(){super(...arguments),this.hasSlotController=new Ot(this,"prefix","suffix"),this.renderType="button",this.rel="noreferrer noopener"}setRenderType(){const e=this.defaultSlot.assignedElements({flatten:!0}).filter(t=>t.tagName.toLowerCase()==="sl-dropdown").length>0;if(this.href){this.renderType="link";return}if(e){this.renderType="dropdown";return}this.renderType="button"}hrefChanged(){this.setRenderType()}handleSlotChange(){this.setRenderType()}render(){return x`
      <div
        part="base"
        class=${R({"breadcrumb-item":!0,"breadcrumb-item--has-prefix":this.hasSlotController.test("prefix"),"breadcrumb-item--has-suffix":this.hasSlotController.test("suffix")})}
      >
        <span part="prefix" class="breadcrumb-item__prefix">
          <slot name="prefix"></slot>
        </span>

        ${this.renderType==="link"?x`
              <a
                part="label"
                class="breadcrumb-item__label breadcrumb-item__label--link"
                href="${this.href}"
                target="${E(this.target?this.target:void 0)}"
                rel=${E(this.target?this.rel:void 0)}
              >
                <slot @slotchange=${this.handleSlotChange}></slot>
              </a>
            `:""}
        ${this.renderType==="button"?x`
              <button part="label" type="button" class="breadcrumb-item__label breadcrumb-item__label--button">
                <slot @slotchange=${this.handleSlotChange}></slot>
              </button>
            `:""}
        ${this.renderType==="dropdown"?x`
              <div part="label" class="breadcrumb-item__label breadcrumb-item__label--drop-down">
                <slot @slotchange=${this.handleSlotChange}></slot>
              </div>
            `:""}

        <span part="suffix" class="breadcrumb-item__suffix">
          <slot name="suffix"></slot>
        </span>

        <span part="separator" class="breadcrumb-item__separator" aria-hidden="true">
          <slot name="separator"></slot>
        </span>
      </div>
    `}};Ne.styles=[D,hm];l([S("slot:not([name])")],Ne.prototype,"defaultSlot",2);l([z()],Ne.prototype,"renderType",2);l([p()],Ne.prototype,"href",2);l([p()],Ne.prototype,"target",2);l([p()],Ne.prototype,"rel",2);l([C("href",{waitUntilFirstUpdate:!0})],Ne.prototype,"hrefChanged",1);Ne.define("sl-breadcrumb-item");mo.define("sl-button-group");var um=O`
  :host {
    display: inline-block;

    --size: 3rem;
  }

  .avatar {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    position: relative;
    width: var(--size);
    height: var(--size);
    background-color: var(--sl-color-neutral-400);
    font-family: var(--sl-font-sans);
    font-size: calc(var(--size) * 0.5);
    font-weight: var(--sl-font-weight-normal);
    color: var(--sl-color-neutral-0);
    user-select: none;
    -webkit-user-select: none;
    vertical-align: middle;
  }

  .avatar--circle,
  .avatar--circle .avatar__image {
    border-radius: var(--sl-border-radius-circle);
  }

  .avatar--rounded,
  .avatar--rounded .avatar__image {
    border-radius: var(--sl-border-radius-medium);
  }

  .avatar--square {
    border-radius: 0;
  }

  .avatar__icon {
    display: flex;
    align-items: center;
    justify-content: center;
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
  }

  .avatar__initials {
    line-height: 1;
    text-transform: uppercase;
  }

  .avatar__image {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    object-fit: cover;
    overflow: hidden;
  }
`,we=class extends T{constructor(){super(...arguments),this.hasError=!1,this.image="",this.label="",this.initials="",this.loading="eager",this.shape="circle"}handleImageChange(){this.hasError=!1}handleImageLoadError(){this.hasError=!0,this.emit("sl-error")}render(){const e=x`
      <img
        part="image"
        class="avatar__image"
        src="${this.image}"
        loading="${this.loading}"
        alt=""
        @error="${this.handleImageLoadError}"
      />
    `;let t=x``;return this.initials?t=x`<div part="initials" class="avatar__initials">${this.initials}</div>`:t=x`
        <div part="icon" class="avatar__icon" aria-hidden="true">
          <slot name="icon">
            <sl-icon name="person-fill" library="system"></sl-icon>
          </slot>
        </div>
      `,x`
      <div
        part="base"
        class=${R({avatar:!0,"avatar--circle":this.shape==="circle","avatar--rounded":this.shape==="rounded","avatar--square":this.shape==="square"})}
        role="img"
        aria-label=${this.label}
      >
        ${this.image&&!this.hasError?e:t}
      </div>
    `}};we.styles=[D,um];we.dependencies={"sl-icon":J};l([z()],we.prototype,"hasError",2);l([p()],we.prototype,"image",2);l([p()],we.prototype,"label",2);l([p()],we.prototype,"initials",2);l([p()],we.prototype,"loading",2);l([p({reflect:!0})],we.prototype,"shape",2);l([C("image")],we.prototype,"handleImageChange",1);we.define("sl-avatar");var pm=O`
  .breadcrumb {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
  }
`,Ho=class extends T{constructor(){super(...arguments),this.localize=new j(this),this.separatorDir=this.localize.dir(),this.label=""}getSeparator(){const t=this.separatorSlot.assignedElements({flatten:!0})[0].cloneNode(!0);return[t,...t.querySelectorAll("[id]")].forEach(o=>o.removeAttribute("id")),t.setAttribute("data-default",""),t.slot="separator",t}handleSlotChange(){const e=[...this.defaultSlot.assignedElements({flatten:!0})].filter(t=>t.tagName.toLowerCase()==="sl-breadcrumb-item");e.forEach((t,o)=>{const i=t.querySelector('[slot="separator"]');i===null?t.append(this.getSeparator()):i.hasAttribute("data-default")&&i.replaceWith(this.getSeparator()),o===e.length-1?t.setAttribute("aria-current","page"):t.removeAttribute("aria-current")})}render(){return this.separatorDir!==this.localize.dir()&&(this.separatorDir=this.localize.dir(),this.updateComplete.then(()=>this.handleSlotChange())),x`
      <nav part="base" class="breadcrumb" aria-label=${this.label}>
        <slot @slotchange=${this.handleSlotChange}></slot>
      </nav>

      <span hidden aria-hidden="true">
        <slot name="separator">
          <sl-icon name=${this.localize.dir()==="rtl"?"chevron-left":"chevron-right"} library="system"></sl-icon>
        </slot>
      </span>
    `}};Ho.styles=[D,pm];Ho.dependencies={"sl-icon":J};l([S("slot")],Ho.prototype,"defaultSlot",2);l([S('slot[name="separator"]')],Ho.prototype,"separatorSlot",2);l([p()],Ho.prototype,"label",2);Ho.define("sl-breadcrumb");X.define("sl-button");var fm=O`
  :host {
    --control-box-size: 3rem;
    --icon-size: calc(var(--control-box-size) * 0.625);

    display: inline-flex;
    position: relative;
    cursor: pointer;
  }

  img {
    display: block;
    width: 100%;
    height: 100%;
  }

  img[aria-hidden='true'] {
    display: none;
  }

  .animated-image__control-box {
    display: flex;
    position: absolute;
    align-items: center;
    justify-content: center;
    top: calc(50% - var(--control-box-size) / 2);
    right: calc(50% - var(--control-box-size) / 2);
    width: var(--control-box-size);
    height: var(--control-box-size);
    font-size: var(--icon-size);
    background: none;
    border: solid 2px currentColor;
    background-color: rgb(0 0 0 /50%);
    border-radius: var(--sl-border-radius-circle);
    color: white;
    pointer-events: none;
    transition: var(--sl-transition-fast) opacity;
  }

  :host([play]:hover) .animated-image__control-box {
    opacity: 1;
  }

  :host([play]:not(:hover)) .animated-image__control-box {
    opacity: 0;
  }

  :host([play]) slot[name='play-icon'],
  :host(:not([play])) slot[name='pause-icon'] {
    display: none;
  }
`,ae=class extends T{constructor(){super(...arguments),this.isLoaded=!1}handleClick(){this.play=!this.play}handleLoad(){const e=document.createElement("canvas"),{width:t,height:o}=this.animatedImage;e.width=t,e.height=o,e.getContext("2d").drawImage(this.animatedImage,0,0,t,o),this.frozenFrame=e.toDataURL("image/gif"),this.isLoaded||(this.emit("sl-load"),this.isLoaded=!0)}handleError(){this.emit("sl-error")}handlePlayChange(){this.play&&(this.animatedImage.src="",this.animatedImage.src=this.src)}handleSrcChange(){this.isLoaded=!1}render(){return x`
      <div class="animated-image">
        <img
          class="animated-image__animated"
          src=${this.src}
          alt=${this.alt}
          crossorigin="anonymous"
          aria-hidden=${this.play?"false":"true"}
          @click=${this.handleClick}
          @load=${this.handleLoad}
          @error=${this.handleError}
        />

        ${this.isLoaded?x`
              <img
                class="animated-image__frozen"
                src=${this.frozenFrame}
                alt=${this.alt}
                aria-hidden=${this.play?"true":"false"}
                @click=${this.handleClick}
              />

              <div part="control-box" class="animated-image__control-box">
                <slot name="play-icon"><sl-icon name="play-fill" library="system"></sl-icon></slot>
                <slot name="pause-icon"><sl-icon name="pause-fill" library="system"></sl-icon></slot>
              </div>
            `:""}
      </div>
    `}};ae.styles=[D,fm];ae.dependencies={"sl-icon":J};l([S(".animated-image__animated")],ae.prototype,"animatedImage",2);l([z()],ae.prototype,"frozenFrame",2);l([z()],ae.prototype,"isLoaded",2);l([p()],ae.prototype,"src",2);l([p()],ae.prototype,"alt",2);l([p({type:Boolean,reflect:!0})],ae.prototype,"play",2);l([C("play",{waitUntilFirstUpdate:!0})],ae.prototype,"handlePlayChange",1);l([C("src")],ae.prototype,"handleSrcChange",1);ae.define("sl-animated-image");var mm=O`
  :host {
    display: inline-flex;
  }

  .badge {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-size: max(12px, 0.75em);
    font-weight: var(--sl-font-weight-semibold);
    letter-spacing: var(--sl-letter-spacing-normal);
    line-height: 1;
    border-radius: var(--sl-border-radius-small);
    border: solid 1px var(--sl-color-neutral-0);
    white-space: nowrap;
    padding: 0.35em 0.6em;
    user-select: none;
    -webkit-user-select: none;
    cursor: inherit;
  }

  /* Variant modifiers */
  .badge--primary {
    background-color: var(--sl-color-primary-600);
    color: var(--sl-color-neutral-0);
  }

  .badge--success {
    background-color: var(--sl-color-success-600);
    color: var(--sl-color-neutral-0);
  }

  .badge--neutral {
    background-color: var(--sl-color-neutral-600);
    color: var(--sl-color-neutral-0);
  }

  .badge--warning {
    background-color: var(--sl-color-warning-600);
    color: var(--sl-color-neutral-0);
  }

  .badge--danger {
    background-color: var(--sl-color-danger-600);
    color: var(--sl-color-neutral-0);
  }

  /* Pill modifier */
  .badge--pill {
    border-radius: var(--sl-border-radius-pill);
  }

  /* Pulse modifier */
  .badge--pulse {
    animation: pulse 1.5s infinite;
  }

  .badge--pulse.badge--primary {
    --pulse-color: var(--sl-color-primary-600);
  }

  .badge--pulse.badge--success {
    --pulse-color: var(--sl-color-success-600);
  }

  .badge--pulse.badge--neutral {
    --pulse-color: var(--sl-color-neutral-600);
  }

  .badge--pulse.badge--warning {
    --pulse-color: var(--sl-color-warning-600);
  }

  .badge--pulse.badge--danger {
    --pulse-color: var(--sl-color-danger-600);
  }

  @keyframes pulse {
    0% {
      box-shadow: 0 0 0 0 var(--pulse-color);
    }
    70% {
      box-shadow: 0 0 0 0.5rem transparent;
    }
    100% {
      box-shadow: 0 0 0 0 transparent;
    }
  }
`,Vi=class extends T{constructor(){super(...arguments),this.variant="primary",this.pill=!1,this.pulse=!1}render(){return x`
      <span
        part="base"
        class=${R({badge:!0,"badge--primary":this.variant==="primary","badge--success":this.variant==="success","badge--neutral":this.variant==="neutral","badge--warning":this.variant==="warning","badge--danger":this.variant==="danger","badge--pill":this.pill,"badge--pulse":this.pulse})}
        role="status"
      >
        <slot></slot>
      </span>
    `}};Vi.styles=[D,mm];l([p({reflect:!0})],Vi.prototype,"variant",2);l([p({type:Boolean,reflect:!0})],Vi.prototype,"pill",2);l([p({type:Boolean,reflect:!0})],Vi.prototype,"pulse",2);Vi.define("sl-badge");var gm=O`
  :host {
    display: contents;

    /* For better DX, we'll reset the margin here so the base part can inherit it */
    margin: 0;
  }

  .alert {
    position: relative;
    display: flex;
    align-items: stretch;
    background-color: var(--sl-panel-background-color);
    border: solid var(--sl-panel-border-width) var(--sl-panel-border-color);
    border-top-width: calc(var(--sl-panel-border-width) * 3);
    border-radius: var(--sl-border-radius-medium);
    font-family: var(--sl-font-sans);
    font-size: var(--sl-font-size-small);
    font-weight: var(--sl-font-weight-normal);
    line-height: 1.6;
    color: var(--sl-color-neutral-700);
    margin: inherit;
    overflow: hidden;
  }

  .alert:not(.alert--has-icon) .alert__icon,
  .alert:not(.alert--closable) .alert__close-button {
    display: none;
  }

  .alert__icon {
    flex: 0 0 auto;
    display: flex;
    align-items: center;
    font-size: var(--sl-font-size-large);
    padding-inline-start: var(--sl-spacing-large);
  }

  .alert--has-countdown {
    border-bottom: none;
  }

  .alert--primary {
    border-top-color: var(--sl-color-primary-600);
  }

  .alert--primary .alert__icon {
    color: var(--sl-color-primary-600);
  }

  .alert--success {
    border-top-color: var(--sl-color-success-600);
  }

  .alert--success .alert__icon {
    color: var(--sl-color-success-600);
  }

  .alert--neutral {
    border-top-color: var(--sl-color-neutral-600);
  }

  .alert--neutral .alert__icon {
    color: var(--sl-color-neutral-600);
  }

  .alert--warning {
    border-top-color: var(--sl-color-warning-600);
  }

  .alert--warning .alert__icon {
    color: var(--sl-color-warning-600);
  }

  .alert--danger {
    border-top-color: var(--sl-color-danger-600);
  }

  .alert--danger .alert__icon {
    color: var(--sl-color-danger-600);
  }

  .alert__message {
    flex: 1 1 auto;
    display: block;
    padding: var(--sl-spacing-large);
    overflow: hidden;
  }

  .alert__close-button {
    flex: 0 0 auto;
    display: flex;
    align-items: center;
    font-size: var(--sl-font-size-medium);
    margin-inline-end: var(--sl-spacing-medium);
    align-self: center;
  }

  .alert__countdown {
    position: absolute;
    bottom: 0;
    left: 0;
    width: 100%;
    height: calc(var(--sl-panel-border-width) * 3);
    background-color: var(--sl-panel-border-color);
    display: flex;
  }

  .alert__countdown--ltr {
    justify-content: flex-end;
  }

  .alert__countdown .alert__countdown-elapsed {
    height: 100%;
    width: 0;
  }

  .alert--primary .alert__countdown-elapsed {
    background-color: var(--sl-color-primary-600);
  }

  .alert--success .alert__countdown-elapsed {
    background-color: var(--sl-color-success-600);
  }

  .alert--neutral .alert__countdown-elapsed {
    background-color: var(--sl-color-neutral-600);
  }

  .alert--warning .alert__countdown-elapsed {
    background-color: var(--sl-color-warning-600);
  }

  .alert--danger .alert__countdown-elapsed {
    background-color: var(--sl-color-danger-600);
  }

  .alert__timer {
    display: none;
  }
`,qt=class We extends T{constructor(){super(...arguments),this.hasSlotController=new Ot(this,"icon","suffix"),this.localize=new j(this),this.open=!1,this.closable=!1,this.variant="primary",this.duration=1/0,this.remainingTime=this.duration}static get toastStack(){return this.currentToastStack||(this.currentToastStack=Object.assign(document.createElement("div"),{className:"sl-toast-stack"})),this.currentToastStack}firstUpdated(){this.base.hidden=!this.open}restartAutoHide(){this.handleCountdownChange(),clearTimeout(this.autoHideTimeout),clearInterval(this.remainingTimeInterval),this.open&&this.duration<1/0&&(this.autoHideTimeout=window.setTimeout(()=>this.hide(),this.duration),this.remainingTime=this.duration,this.remainingTimeInterval=window.setInterval(()=>{this.remainingTime-=100},100))}pauseAutoHide(){var t;(t=this.countdownAnimation)==null||t.pause(),clearTimeout(this.autoHideTimeout),clearInterval(this.remainingTimeInterval)}resumeAutoHide(){var t;this.duration<1/0&&(this.autoHideTimeout=window.setTimeout(()=>this.hide(),this.remainingTime),this.remainingTimeInterval=window.setInterval(()=>{this.remainingTime-=100},100),(t=this.countdownAnimation)==null||t.play())}handleCountdownChange(){if(this.open&&this.duration<1/0&&this.countdown){const{countdownElement:t}=this,o="100%",i="0";this.countdownAnimation=t.animate([{width:o},{width:i}],{duration:this.duration,easing:"linear"})}}handleCloseClick(){this.hide()}async handleOpenChange(){if(this.open){this.emit("sl-show"),this.duration<1/0&&this.restartAutoHide(),await ut(this.base),this.base.hidden=!1;const{keyframes:t,options:o}=ot(this,"alert.show",{dir:this.localize.dir()});await at(this.base,t,o),this.emit("sl-after-show")}else{ba(this),this.emit("sl-hide"),clearTimeout(this.autoHideTimeout),clearInterval(this.remainingTimeInterval),await ut(this.base);const{keyframes:t,options:o}=ot(this,"alert.hide",{dir:this.localize.dir()});await at(this.base,t,o),this.base.hidden=!0,this.emit("sl-after-hide")}}handleDurationChange(){this.restartAutoHide()}async show(){if(!this.open)return this.open=!0,Pt(this,"sl-after-show")}async hide(){if(this.open)return this.open=!1,Pt(this,"sl-after-hide")}async toast(){return new Promise(t=>{this.handleCountdownChange(),We.toastStack.parentElement===null&&document.body.append(We.toastStack),We.toastStack.appendChild(this),requestAnimationFrame(()=>{this.clientWidth,this.show()}),this.addEventListener("sl-after-hide",()=>{We.toastStack.removeChild(this),t(),We.toastStack.querySelector("sl-alert")===null&&We.toastStack.remove()},{once:!0})})}render(){return x`
      <div
        part="base"
        class=${R({alert:!0,"alert--open":this.open,"alert--closable":this.closable,"alert--has-countdown":!!this.countdown,"alert--has-icon":this.hasSlotController.test("icon"),"alert--primary":this.variant==="primary","alert--success":this.variant==="success","alert--neutral":this.variant==="neutral","alert--warning":this.variant==="warning","alert--danger":this.variant==="danger"})}
        role="alert"
        aria-hidden=${this.open?"false":"true"}
        @mouseenter=${this.pauseAutoHide}
        @mouseleave=${this.resumeAutoHide}
      >
        <div part="icon" class="alert__icon">
          <slot name="icon"></slot>
        </div>

        <div part="message" class="alert__message" aria-live="polite">
          <slot></slot>
        </div>

        ${this.closable?x`
              <sl-icon-button
                part="close-button"
                exportparts="base:close-button__base"
                class="alert__close-button"
                name="x-lg"
                library="system"
                label=${this.localize.term("close")}
                @click=${this.handleCloseClick}
              ></sl-icon-button>
            `:""}

        <div role="timer" class="alert__timer">${this.remainingTime}</div>

        ${this.countdown?x`
              <div
                class=${R({alert__countdown:!0,"alert__countdown--ltr":this.countdown==="ltr"})}
              >
                <div class="alert__countdown-elapsed"></div>
              </div>
            `:""}
      </div>
    `}};qt.styles=[D,gm];qt.dependencies={"sl-icon-button":gt};l([S('[part~="base"]')],qt.prototype,"base",2);l([S(".alert__countdown-elapsed")],qt.prototype,"countdownElement",2);l([p({type:Boolean,reflect:!0})],qt.prototype,"open",2);l([p({type:Boolean,reflect:!0})],qt.prototype,"closable",2);l([p({reflect:!0})],qt.prototype,"variant",2);l([p({type:Number})],qt.prototype,"duration",2);l([p({type:String,reflect:!0})],qt.prototype,"countdown",2);l([z()],qt.prototype,"remainingTime",2);l([C("open",{waitUntilFirstUpdate:!0})],qt.prototype,"handleOpenChange",1);l([C("duration")],qt.prototype,"handleDurationChange",1);var bm=qt;K("alert.show",{keyframes:[{opacity:0,scale:.8},{opacity:1,scale:1}],options:{duration:250,easing:"ease"}});K("alert.hide",{keyframes:[{opacity:1,scale:1},{opacity:0,scale:.8}],options:{duration:250,easing:"ease"}});bm.define("sl-alert");const vm=[{offset:0,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)",transform:"translate3d(0, 0, 0)"},{offset:.2,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)",transform:"translate3d(0, 0, 0)"},{offset:.4,easing:"cubic-bezier(0.755, 0.05, 0.855, 0.06)",transform:"translate3d(0, -30px, 0) scaleY(1.1)"},{offset:.43,easing:"cubic-bezier(0.755, 0.05, 0.855, 0.06)",transform:"translate3d(0, -30px, 0) scaleY(1.1)"},{offset:.53,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)",transform:"translate3d(0, 0, 0)"},{offset:.7,easing:"cubic-bezier(0.755, 0.05, 0.855, 0.06)",transform:"translate3d(0, -15px, 0) scaleY(1.05)"},{offset:.8,"transition-timing-function":"cubic-bezier(0.215, 0.61, 0.355, 1)",transform:"translate3d(0, 0, 0) scaleY(0.95)"},{offset:.9,transform:"translate3d(0, -4px, 0) scaleY(1.02)"},{offset:1,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)",transform:"translate3d(0, 0, 0)"}],ym=[{offset:0,opacity:"1"},{offset:.25,opacity:"0"},{offset:.5,opacity:"1"},{offset:.75,opacity:"0"},{offset:1,opacity:"1"}],wm=[{offset:0,transform:"translateX(0)"},{offset:.065,transform:"translateX(-6px) rotateY(-9deg)"},{offset:.185,transform:"translateX(5px) rotateY(7deg)"},{offset:.315,transform:"translateX(-3px) rotateY(-5deg)"},{offset:.435,transform:"translateX(2px) rotateY(3deg)"},{offset:.5,transform:"translateX(0)"}],xm=[{offset:0,transform:"scale(1)"},{offset:.14,transform:"scale(1.3)"},{offset:.28,transform:"scale(1)"},{offset:.42,transform:"scale(1.3)"},{offset:.7,transform:"scale(1)"}],km=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:.111,transform:"translate3d(0, 0, 0)"},{offset:.222,transform:"skewX(-12.5deg) skewY(-12.5deg)"},{offset:.33299999999999996,transform:"skewX(6.25deg) skewY(6.25deg)"},{offset:.444,transform:"skewX(-3.125deg) skewY(-3.125deg)"},{offset:.555,transform:"skewX(1.5625deg) skewY(1.5625deg)"},{offset:.6659999999999999,transform:"skewX(-0.78125deg) skewY(-0.78125deg)"},{offset:.777,transform:"skewX(0.390625deg) skewY(0.390625deg)"},{offset:.888,transform:"skewX(-0.1953125deg) skewY(-0.1953125deg)"},{offset:1,transform:"translate3d(0, 0, 0)"}],_m=[{offset:0,transform:"scale3d(1, 1, 1)"},{offset:.5,transform:"scale3d(1.05, 1.05, 1.05)"},{offset:1,transform:"scale3d(1, 1, 1)"}],$m=[{offset:0,transform:"scale3d(1, 1, 1)"},{offset:.3,transform:"scale3d(1.25, 0.75, 1)"},{offset:.4,transform:"scale3d(0.75, 1.25, 1)"},{offset:.5,transform:"scale3d(1.15, 0.85, 1)"},{offset:.65,transform:"scale3d(0.95, 1.05, 1)"},{offset:.75,transform:"scale3d(1.05, 0.95, 1)"},{offset:1,transform:"scale3d(1, 1, 1)"}],Cm=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:.1,transform:"translate3d(-10px, 0, 0)"},{offset:.2,transform:"translate3d(10px, 0, 0)"},{offset:.3,transform:"translate3d(-10px, 0, 0)"},{offset:.4,transform:"translate3d(10px, 0, 0)"},{offset:.5,transform:"translate3d(-10px, 0, 0)"},{offset:.6,transform:"translate3d(10px, 0, 0)"},{offset:.7,transform:"translate3d(-10px, 0, 0)"},{offset:.8,transform:"translate3d(10px, 0, 0)"},{offset:.9,transform:"translate3d(-10px, 0, 0)"},{offset:1,transform:"translate3d(0, 0, 0)"}],Sm=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:.1,transform:"translate3d(-10px, 0, 0)"},{offset:.2,transform:"translate3d(10px, 0, 0)"},{offset:.3,transform:"translate3d(-10px, 0, 0)"},{offset:.4,transform:"translate3d(10px, 0, 0)"},{offset:.5,transform:"translate3d(-10px, 0, 0)"},{offset:.6,transform:"translate3d(10px, 0, 0)"},{offset:.7,transform:"translate3d(-10px, 0, 0)"},{offset:.8,transform:"translate3d(10px, 0, 0)"},{offset:.9,transform:"translate3d(-10px, 0, 0)"},{offset:1,transform:"translate3d(0, 0, 0)"}],Am=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:.1,transform:"translate3d(0, -10px, 0)"},{offset:.2,transform:"translate3d(0, 10px, 0)"},{offset:.3,transform:"translate3d(0, -10px, 0)"},{offset:.4,transform:"translate3d(0, 10px, 0)"},{offset:.5,transform:"translate3d(0, -10px, 0)"},{offset:.6,transform:"translate3d(0, 10px, 0)"},{offset:.7,transform:"translate3d(0, -10px, 0)"},{offset:.8,transform:"translate3d(0, 10px, 0)"},{offset:.9,transform:"translate3d(0, -10px, 0)"},{offset:1,transform:"translate3d(0, 0, 0)"}],zm=[{offset:.2,transform:"rotate3d(0, 0, 1, 15deg)"},{offset:.4,transform:"rotate3d(0, 0, 1, -10deg)"},{offset:.6,transform:"rotate3d(0, 0, 1, 5deg)"},{offset:.8,transform:"rotate3d(0, 0, 1, -5deg)"},{offset:1,transform:"rotate3d(0, 0, 1, 0deg)"}],Em=[{offset:0,transform:"scale3d(1, 1, 1)"},{offset:.1,transform:"scale3d(0.9, 0.9, 0.9) rotate3d(0, 0, 1, -3deg)"},{offset:.2,transform:"scale3d(0.9, 0.9, 0.9) rotate3d(0, 0, 1, -3deg)"},{offset:.3,transform:"scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, 3deg)"},{offset:.4,transform:"scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, -3deg)"},{offset:.5,transform:"scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, 3deg)"},{offset:.6,transform:"scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, -3deg)"},{offset:.7,transform:"scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, 3deg)"},{offset:.8,transform:"scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, -3deg)"},{offset:.9,transform:"scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, 3deg)"},{offset:1,transform:"scale3d(1, 1, 1)"}],Tm=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:.15,transform:"translate3d(-25%, 0, 0) rotate3d(0, 0, 1, -5deg)"},{offset:.3,transform:"translate3d(20%, 0, 0) rotate3d(0, 0, 1, 3deg)"},{offset:.45,transform:"translate3d(-15%, 0, 0) rotate3d(0, 0, 1, -3deg)"},{offset:.6,transform:"translate3d(10%, 0, 0) rotate3d(0, 0, 1, 2deg)"},{offset:.75,transform:"translate3d(-5%, 0, 0) rotate3d(0, 0, 1, -1deg)"},{offset:1,transform:"translate3d(0, 0, 0)"}],Pm=[{offset:0,transform:"translateY(-1200px) scale(0.7)",opacity:"0.7"},{offset:.8,transform:"translateY(0px) scale(0.7)",opacity:"0.7"},{offset:1,transform:"scale(1)",opacity:"1"}],Om=[{offset:0,transform:"translateX(-2000px) scale(0.7)",opacity:"0.7"},{offset:.8,transform:"translateX(0px) scale(0.7)",opacity:"0.7"},{offset:1,transform:"scale(1)",opacity:"1"}],Lm=[{offset:0,transform:"translateX(2000px) scale(0.7)",opacity:"0.7"},{offset:.8,transform:"translateX(0px) scale(0.7)",opacity:"0.7"},{offset:1,transform:"scale(1)",opacity:"1"}],Rm=[{offset:0,transform:"translateY(1200px) scale(0.7)",opacity:"0.7"},{offset:.8,transform:"translateY(0px) scale(0.7)",opacity:"0.7"},{offset:1,transform:"scale(1)",opacity:"1"}],Dm=[{offset:0,transform:"scale(1)",opacity:"1"},{offset:.2,transform:"translateY(0px) scale(0.7)",opacity:"0.7"},{offset:1,transform:"translateY(700px) scale(0.7)",opacity:"0.7"}],Mm=[{offset:0,transform:"scale(1)",opacity:"1"},{offset:.2,transform:"translateX(0px) scale(0.7)",opacity:"0.7"},{offset:1,transform:"translateX(-2000px) scale(0.7)",opacity:"0.7"}],Im=[{offset:0,transform:"scale(1)",opacity:"1"},{offset:.2,transform:"translateX(0px) scale(0.7)",opacity:"0.7"},{offset:1,transform:"translateX(2000px) scale(0.7)",opacity:"0.7"}],Bm=[{offset:0,transform:"scale(1)",opacity:"1"},{offset:.2,transform:"translateY(0px) scale(0.7)",opacity:"0.7"},{offset:1,transform:"translateY(-700px) scale(0.7)",opacity:"0.7"}],Fm=[{offset:0,opacity:"0",transform:"scale3d(0.3, 0.3, 0.3)"},{offset:0,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.2,transform:"scale3d(1.1, 1.1, 1.1)"},{offset:.2,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.4,transform:"scale3d(0.9, 0.9, 0.9)"},{offset:.4,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.6,opacity:"1",transform:"scale3d(1.03, 1.03, 1.03)"},{offset:.6,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.8,transform:"scale3d(0.97, 0.97, 0.97)"},{offset:.8,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:1,opacity:"1",transform:"scale3d(1, 1, 1)"},{offset:1,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"}],Vm=[{offset:0,opacity:"0",transform:"translate3d(0, -3000px, 0) scaleY(3)"},{offset:0,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.6,opacity:"1",transform:"translate3d(0, 25px, 0) scaleY(0.9)"},{offset:.6,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.75,transform:"translate3d(0, -10px, 0) scaleY(0.95)"},{offset:.75,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.9,transform:"translate3d(0, 5px, 0) scaleY(0.985)"},{offset:.9,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:1,transform:"translate3d(0, 0, 0)"},{offset:1,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"}],Um=[{offset:0,opacity:"0",transform:"translate3d(-3000px, 0, 0) scaleX(3)"},{offset:0,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.6,opacity:"1",transform:"translate3d(25px, 0, 0) scaleX(1)"},{offset:.6,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.75,transform:"translate3d(-10px, 0, 0) scaleX(0.98)"},{offset:.75,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.9,transform:"translate3d(5px, 0, 0) scaleX(0.995)"},{offset:.9,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:1,transform:"translate3d(0, 0, 0)"},{offset:1,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"}],Nm=[{offset:0,opacity:"0",transform:"translate3d(3000px, 0, 0) scaleX(3)"},{offset:0,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.6,opacity:"1",transform:"translate3d(-25px, 0, 0) scaleX(1)"},{offset:.6,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.75,transform:"translate3d(10px, 0, 0) scaleX(0.98)"},{offset:.75,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.9,transform:"translate3d(-5px, 0, 0) scaleX(0.995)"},{offset:.9,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:1,transform:"translate3d(0, 0, 0)"},{offset:1,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"}],Hm=[{offset:0,opacity:"0",transform:"translate3d(0, 3000px, 0) scaleY(5)"},{offset:0,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.6,opacity:"1",transform:"translate3d(0, -20px, 0) scaleY(0.9)"},{offset:.6,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.75,transform:"translate3d(0, 10px, 0) scaleY(0.95)"},{offset:.75,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.9,transform:"translate3d(0, -5px, 0) scaleY(0.985)"},{offset:.9,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:1,transform:"translate3d(0, 0, 0)"},{offset:1,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"}],jm=[{offset:.2,transform:"scale3d(0.9, 0.9, 0.9)"},{offset:.5,opacity:"1",transform:"scale3d(1.1, 1.1, 1.1)"},{offset:.55,opacity:"1",transform:"scale3d(1.1, 1.1, 1.1)"},{offset:1,opacity:"0",transform:"scale3d(0.3, 0.3, 0.3)"}],qm=[{offset:.2,transform:"translate3d(0, 10px, 0) scaleY(0.985)"},{offset:.4,opacity:"1",transform:"translate3d(0, -20px, 0) scaleY(0.9)"},{offset:.45,opacity:"1",transform:"translate3d(0, -20px, 0) scaleY(0.9)"},{offset:1,opacity:"0",transform:"translate3d(0, 2000px, 0) scaleY(3)"}],Wm=[{offset:.2,opacity:"1",transform:"translate3d(20px, 0, 0) scaleX(0.9)"},{offset:1,opacity:"0",transform:"translate3d(-2000px, 0, 0) scaleX(2)"}],Km=[{offset:.2,opacity:"1",transform:"translate3d(-20px, 0, 0) scaleX(0.9)"},{offset:1,opacity:"0",transform:"translate3d(2000px, 0, 0) scaleX(2)"}],Ym=[{offset:.2,transform:"translate3d(0, -10px, 0) scaleY(0.985)"},{offset:.4,opacity:"1",transform:"translate3d(0, 20px, 0) scaleY(0.9)"},{offset:.45,opacity:"1",transform:"translate3d(0, 20px, 0) scaleY(0.9)"},{offset:1,opacity:"0",transform:"translate3d(0, -2000px, 0) scaleY(3)"}],Xm=[{offset:0,opacity:"0"},{offset:1,opacity:"1"}],Qm=[{offset:0,opacity:"0",transform:"translate3d(-100%, 100%, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],Zm=[{offset:0,opacity:"0",transform:"translate3d(100%, 100%, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],Gm=[{offset:0,opacity:"0",transform:"translate3d(0, -100%, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],Jm=[{offset:0,opacity:"0",transform:"translate3d(0, -2000px, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],tg=[{offset:0,opacity:"0",transform:"translate3d(-100%, 0, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],eg=[{offset:0,opacity:"0",transform:"translate3d(-2000px, 0, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],og=[{offset:0,opacity:"0",transform:"translate3d(100%, 0, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],ig=[{offset:0,opacity:"0",transform:"translate3d(2000px, 0, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],sg=[{offset:0,opacity:"0",transform:"translate3d(-100%, -100%, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],rg=[{offset:0,opacity:"0",transform:"translate3d(100%, -100%, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],ag=[{offset:0,opacity:"0",transform:"translate3d(0, 100%, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],ng=[{offset:0,opacity:"0",transform:"translate3d(0, 2000px, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],lg=[{offset:0,opacity:"1"},{offset:1,opacity:"0"}],cg=[{offset:0,opacity:"1",transform:"translate3d(0, 0, 0)"},{offset:1,opacity:"0",transform:"translate3d(-100%, 100%, 0)"}],dg=[{offset:0,opacity:"1",transform:"translate3d(0, 0, 0)"},{offset:1,opacity:"0",transform:"translate3d(100%, 100%, 0)"}],hg=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(0, 100%, 0)"}],ug=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(0, 2000px, 0)"}],pg=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(-100%, 0, 0)"}],fg=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(-2000px, 0, 0)"}],mg=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(100%, 0, 0)"}],gg=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(2000px, 0, 0)"}],bg=[{offset:0,opacity:"1",transform:"translate3d(0, 0, 0)"},{offset:1,opacity:"0",transform:"translate3d(-100%, -100%, 0)"}],vg=[{offset:0,opacity:"1",transform:"translate3d(0, 0, 0)"},{offset:1,opacity:"0",transform:"translate3d(100%, -100%, 0)"}],yg=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(0, -100%, 0)"}],wg=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(0, -2000px, 0)"}],xg=[{offset:0,transform:"perspective(400px) scale3d(1, 1, 1) translate3d(0, 0, 0) rotate3d(0, 1, 0, -360deg)",easing:"ease-out"},{offset:.4,transform:`perspective(400px) scale3d(1, 1, 1) translate3d(0, 0, 150px)
      rotate3d(0, 1, 0, -190deg)`,easing:"ease-out"},{offset:.5,transform:`perspective(400px) scale3d(1, 1, 1) translate3d(0, 0, 150px)
      rotate3d(0, 1, 0, -170deg)`,easing:"ease-in"},{offset:.8,transform:`perspective(400px) scale3d(0.95, 0.95, 0.95) translate3d(0, 0, 0)
      rotate3d(0, 1, 0, 0deg)`,easing:"ease-in"},{offset:1,transform:"perspective(400px) scale3d(1, 1, 1) translate3d(0, 0, 0) rotate3d(0, 1, 0, 0deg)",easing:"ease-in"}],kg=[{offset:0,transform:"perspective(400px) rotate3d(1, 0, 0, 90deg)",easing:"ease-in",opacity:"0"},{offset:.4,transform:"perspective(400px) rotate3d(1, 0, 0, -20deg)",easing:"ease-in"},{offset:.6,transform:"perspective(400px) rotate3d(1, 0, 0, 10deg)",opacity:"1"},{offset:.8,transform:"perspective(400px) rotate3d(1, 0, 0, -5deg)"},{offset:1,transform:"perspective(400px)"}],_g=[{offset:0,transform:"perspective(400px) rotate3d(0, 1, 0, 90deg)",easing:"ease-in",opacity:"0"},{offset:.4,transform:"perspective(400px) rotate3d(0, 1, 0, -20deg)",easing:"ease-in"},{offset:.6,transform:"perspective(400px) rotate3d(0, 1, 0, 10deg)",opacity:"1"},{offset:.8,transform:"perspective(400px) rotate3d(0, 1, 0, -5deg)"},{offset:1,transform:"perspective(400px)"}],$g=[{offset:0,transform:"perspective(400px)"},{offset:.3,transform:"perspective(400px) rotate3d(1, 0, 0, -20deg)",opacity:"1"},{offset:1,transform:"perspective(400px) rotate3d(1, 0, 0, 90deg)",opacity:"0"}],Cg=[{offset:0,transform:"perspective(400px)"},{offset:.3,transform:"perspective(400px) rotate3d(0, 1, 0, -15deg)",opacity:"1"},{offset:1,transform:"perspective(400px) rotate3d(0, 1, 0, 90deg)",opacity:"0"}],Sg=[{offset:0,transform:"translate3d(-100%, 0, 0) skewX(30deg)",opacity:"0"},{offset:.6,transform:"skewX(-20deg)",opacity:"1"},{offset:.8,transform:"skewX(5deg)"},{offset:1,transform:"translate3d(0, 0, 0)"}],Ag=[{offset:0,transform:"translate3d(100%, 0, 0) skewX(-30deg)",opacity:"0"},{offset:.6,transform:"skewX(20deg)",opacity:"1"},{offset:.8,transform:"skewX(-5deg)"},{offset:1,transform:"translate3d(0, 0, 0)"}],zg=[{offset:0,opacity:"1"},{offset:1,transform:"translate3d(-100%, 0, 0) skewX(-30deg)",opacity:"0"}],Eg=[{offset:0,opacity:"1"},{offset:1,transform:"translate3d(100%, 0, 0) skewX(30deg)",opacity:"0"}],Tg=[{offset:0,transform:"rotate3d(0, 0, 1, -200deg)",opacity:"0"},{offset:1,transform:"translate3d(0, 0, 0)",opacity:"1"}],Pg=[{offset:0,transform:"rotate3d(0, 0, 1, -45deg)",opacity:"0"},{offset:1,transform:"translate3d(0, 0, 0)",opacity:"1"}],Og=[{offset:0,transform:"rotate3d(0, 0, 1, 45deg)",opacity:"0"},{offset:1,transform:"translate3d(0, 0, 0)",opacity:"1"}],Lg=[{offset:0,transform:"rotate3d(0, 0, 1, 45deg)",opacity:"0"},{offset:1,transform:"translate3d(0, 0, 0)",opacity:"1"}],Rg=[{offset:0,transform:"rotate3d(0, 0, 1, -90deg)",opacity:"0"},{offset:1,transform:"translate3d(0, 0, 0)",opacity:"1"}],Dg=[{offset:0,opacity:"1"},{offset:1,transform:"rotate3d(0, 0, 1, 200deg)",opacity:"0"}],Mg=[{offset:0,opacity:"1"},{offset:1,transform:"rotate3d(0, 0, 1, 45deg)",opacity:"0"}],Ig=[{offset:0,opacity:"1"},{offset:1,transform:"rotate3d(0, 0, 1, -45deg)",opacity:"0"}],Bg=[{offset:0,opacity:"1"},{offset:1,transform:"rotate3d(0, 0, 1, -45deg)",opacity:"0"}],Fg=[{offset:0,opacity:"1"},{offset:1,transform:"rotate3d(0, 0, 1, 90deg)",opacity:"0"}],Vg=[{offset:0,transform:"translate3d(0, -100%, 0)",visibility:"visible"},{offset:1,transform:"translate3d(0, 0, 0)"}],Ug=[{offset:0,transform:"translate3d(-100%, 0, 0)",visibility:"visible"},{offset:1,transform:"translate3d(0, 0, 0)"}],Ng=[{offset:0,transform:"translate3d(100%, 0, 0)",visibility:"visible"},{offset:1,transform:"translate3d(0, 0, 0)"}],Hg=[{offset:0,transform:"translate3d(0, 100%, 0)",visibility:"visible"},{offset:1,transform:"translate3d(0, 0, 0)"}],jg=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:1,visibility:"hidden",transform:"translate3d(0, 100%, 0)"}],qg=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:1,visibility:"hidden",transform:"translate3d(-100%, 0, 0)"}],Wg=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:1,visibility:"hidden",transform:"translate3d(100%, 0, 0)"}],Kg=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:1,visibility:"hidden",transform:"translate3d(0, -100%, 0)"}],Yg=[{offset:0,easing:"ease-in-out"},{offset:.2,transform:"rotate3d(0, 0, 1, 80deg)",easing:"ease-in-out"},{offset:.4,transform:"rotate3d(0, 0, 1, 60deg)",easing:"ease-in-out",opacity:"1"},{offset:.6,transform:"rotate3d(0, 0, 1, 80deg)",easing:"ease-in-out"},{offset:.8,transform:"rotate3d(0, 0, 1, 60deg)",easing:"ease-in-out",opacity:"1"},{offset:1,transform:"translate3d(0, 700px, 0)",opacity:"0"}],Xg=[{offset:0,opacity:"0",transform:"scale(0.1) rotate(30deg)","transform-origin":"center bottom"},{offset:.5,transform:"rotate(-10deg)"},{offset:.7,transform:"rotate(3deg)"},{offset:1,opacity:"1",transform:"scale(1)"}],Qg=[{offset:0,opacity:"0",transform:"translate3d(-100%, 0, 0) rotate3d(0, 0, 1, -120deg)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],Zg=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(100%, 0, 0) rotate3d(0, 0, 1, 120deg)"}],Gg=[{offset:0,opacity:"0",transform:"scale3d(0.3, 0.3, 0.3)"},{offset:.5,opacity:"1"}],Jg=[{offset:0,opacity:"0",transform:"scale3d(0.1, 0.1, 0.1) translate3d(0, -1000px, 0)",easing:"cubic-bezier(0.55, 0.055, 0.675, 0.19)"},{offset:.6,opacity:"1",transform:"scale3d(0.475, 0.475, 0.475) translate3d(0, 60px, 0)",easing:"cubic-bezier(0.175, 0.885, 0.32, 1)"}],tb=[{offset:0,opacity:"0",transform:"scale3d(0.1, 0.1, 0.1) translate3d(-1000px, 0, 0)",easing:"cubic-bezier(0.55, 0.055, 0.675, 0.19)"},{offset:.6,opacity:"1",transform:"scale3d(0.475, 0.475, 0.475) translate3d(10px, 0, 0)",easing:"cubic-bezier(0.175, 0.885, 0.32, 1)"}],eb=[{offset:0,opacity:"0",transform:"scale3d(0.1, 0.1, 0.1) translate3d(1000px, 0, 0)",easing:"cubic-bezier(0.55, 0.055, 0.675, 0.19)"},{offset:.6,opacity:"1",transform:"scale3d(0.475, 0.475, 0.475) translate3d(-10px, 0, 0)",easing:"cubic-bezier(0.175, 0.885, 0.32, 1)"}],ob=[{offset:0,opacity:"0",transform:"scale3d(0.1, 0.1, 0.1) translate3d(0, 1000px, 0)",easing:"cubic-bezier(0.55, 0.055, 0.675, 0.19)"},{offset:.6,opacity:"1",transform:"scale3d(0.475, 0.475, 0.475) translate3d(0, -60px, 0)",easing:"cubic-bezier(0.175, 0.885, 0.32, 1)"}],ib=[{offset:0,opacity:"1"},{offset:.5,opacity:"0",transform:"scale3d(0.3, 0.3, 0.3)"},{offset:1,opacity:"0"}],sb=[{offset:.4,opacity:"1",transform:"scale3d(0.475, 0.475, 0.475) translate3d(0, -60px, 0)",easing:"cubic-bezier(0.55, 0.055, 0.675, 0.19)"},{offset:1,opacity:"0",transform:"scale3d(0.1, 0.1, 0.1) translate3d(0, 2000px, 0)",easing:"cubic-bezier(0.175, 0.885, 0.32, 1)"}],rb=[{offset:.4,opacity:"1",transform:"scale3d(0.475, 0.475, 0.475) translate3d(42px, 0, 0)"},{offset:1,opacity:"0",transform:"scale(0.1) translate3d(-2000px, 0, 0)"}],ab=[{offset:.4,opacity:"1",transform:"scale3d(0.475, 0.475, 0.475) translate3d(-42px, 0, 0)"},{offset:1,opacity:"0",transform:"scale(0.1) translate3d(2000px, 0, 0)"}],nb=[{offset:.4,opacity:"1",transform:"scale3d(0.475, 0.475, 0.475) translate3d(0, 60px, 0)",easing:"cubic-bezier(0.55, 0.055, 0.675, 0.19)"},{offset:1,opacity:"0",transform:"scale3d(0.1, 0.1, 0.1) translate3d(0, -2000px, 0)",easing:"cubic-bezier(0.175, 0.885, 0.32, 1)"}],nc={linear:"linear",ease:"ease",easeIn:"ease-in",easeOut:"ease-out",easeInOut:"ease-in-out",easeInSine:"cubic-bezier(0.47, 0, 0.745, 0.715)",easeOutSine:"cubic-bezier(0.39, 0.575, 0.565, 1)",easeInOutSine:"cubic-bezier(0.445, 0.05, 0.55, 0.95)",easeInQuad:"cubic-bezier(0.55, 0.085, 0.68, 0.53)",easeOutQuad:"cubic-bezier(0.25, 0.46, 0.45, 0.94)",easeInOutQuad:"cubic-bezier(0.455, 0.03, 0.515, 0.955)",easeInCubic:"cubic-bezier(0.55, 0.055, 0.675, 0.19)",easeOutCubic:"cubic-bezier(0.215, 0.61, 0.355, 1)",easeInOutCubic:"cubic-bezier(0.645, 0.045, 0.355, 1)",easeInQuart:"cubic-bezier(0.895, 0.03, 0.685, 0.22)",easeOutQuart:"cubic-bezier(0.165, 0.84, 0.44, 1)",easeInOutQuart:"cubic-bezier(0.77, 0, 0.175, 1)",easeInQuint:"cubic-bezier(0.755, 0.05, 0.855, 0.06)",easeOutQuint:"cubic-bezier(0.23, 1, 0.32, 1)",easeInOutQuint:"cubic-bezier(0.86, 0, 0.07, 1)",easeInExpo:"cubic-bezier(0.95, 0.05, 0.795, 0.035)",easeOutExpo:"cubic-bezier(0.19, 1, 0.22, 1)",easeInOutExpo:"cubic-bezier(1, 0, 0, 1)",easeInCirc:"cubic-bezier(0.6, 0.04, 0.98, 0.335)",easeOutCirc:"cubic-bezier(0.075, 0.82, 0.165, 1)",easeInOutCirc:"cubic-bezier(0.785, 0.135, 0.15, 0.86)",easeInBack:"cubic-bezier(0.6, -0.28, 0.735, 0.045)",easeOutBack:"cubic-bezier(0.175, 0.885, 0.32, 1.275)",easeInOutBack:"cubic-bezier(0.68, -0.55, 0.265, 1.55)"},lb=Object.freeze(Object.defineProperty({__proto__:null,backInDown:Pm,backInLeft:Om,backInRight:Lm,backInUp:Rm,backOutDown:Dm,backOutLeft:Mm,backOutRight:Im,backOutUp:Bm,bounce:vm,bounceIn:Fm,bounceInDown:Vm,bounceInLeft:Um,bounceInRight:Nm,bounceInUp:Hm,bounceOut:jm,bounceOutDown:qm,bounceOutLeft:Wm,bounceOutRight:Km,bounceOutUp:Ym,easings:nc,fadeIn:Xm,fadeInBottomLeft:Qm,fadeInBottomRight:Zm,fadeInDown:Gm,fadeInDownBig:Jm,fadeInLeft:tg,fadeInLeftBig:eg,fadeInRight:og,fadeInRightBig:ig,fadeInTopLeft:sg,fadeInTopRight:rg,fadeInUp:ag,fadeInUpBig:ng,fadeOut:lg,fadeOutBottomLeft:cg,fadeOutBottomRight:dg,fadeOutDown:hg,fadeOutDownBig:ug,fadeOutLeft:pg,fadeOutLeftBig:fg,fadeOutRight:mg,fadeOutRightBig:gg,fadeOutTopLeft:bg,fadeOutTopRight:vg,fadeOutUp:yg,fadeOutUpBig:wg,flash:ym,flip:xg,flipInX:kg,flipInY:_g,flipOutX:$g,flipOutY:Cg,headShake:wm,heartBeat:xm,hinge:Yg,jackInTheBox:Xg,jello:km,lightSpeedInLeft:Sg,lightSpeedInRight:Ag,lightSpeedOutLeft:zg,lightSpeedOutRight:Eg,pulse:_m,rollIn:Qg,rollOut:Zg,rotateIn:Tg,rotateInDownLeft:Pg,rotateInDownRight:Og,rotateInUpLeft:Lg,rotateInUpRight:Rg,rotateOut:Dg,rotateOutDownLeft:Mg,rotateOutDownRight:Ig,rotateOutUpLeft:Bg,rotateOutUpRight:Fg,rubberBand:$m,shake:Cm,shakeX:Sm,shakeY:Am,slideInDown:Vg,slideInLeft:Ug,slideInRight:Ng,slideInUp:Hg,slideOutDown:jg,slideOutLeft:qg,slideOutRight:Wg,slideOutUp:Kg,swing:zm,tada:Em,wobble:Tm,zoomIn:Gg,zoomInDown:Jg,zoomInLeft:tb,zoomInRight:eb,zoomInUp:ob,zoomOut:ib,zoomOutDown:sb,zoomOutLeft:rb,zoomOutRight:ab,zoomOutUp:nb},Symbol.toStringTag,{value:"Module"}));var cb=O`
  :host {
    display: contents;
  }
`,xt=class extends T{constructor(){super(...arguments),this.hasStarted=!1,this.name="none",this.play=!1,this.delay=0,this.direction="normal",this.duration=1e3,this.easing="linear",this.endDelay=0,this.fill="auto",this.iterations=1/0,this.iterationStart=0,this.playbackRate=1,this.handleAnimationFinish=()=>{this.play=!1,this.hasStarted=!1,this.emit("sl-finish")},this.handleAnimationCancel=()=>{this.play=!1,this.hasStarted=!1,this.emit("sl-cancel")}}get currentTime(){var e,t;return(t=(e=this.animation)==null?void 0:e.currentTime)!=null?t:0}set currentTime(e){this.animation&&(this.animation.currentTime=e)}connectedCallback(){super.connectedCallback(),this.createAnimation()}disconnectedCallback(){super.disconnectedCallback(),this.destroyAnimation()}handleSlotChange(){this.destroyAnimation(),this.createAnimation()}async createAnimation(){var e,t;const o=(e=nc[this.easing])!=null?e:this.easing,i=(t=this.keyframes)!=null?t:lb[this.name],r=(await this.defaultSlot).assignedElements()[0];return!r||!i?!1:(this.destroyAnimation(),this.animation=r.animate(i,{delay:this.delay,direction:this.direction,duration:this.duration,easing:o,endDelay:this.endDelay,fill:this.fill,iterationStart:this.iterationStart,iterations:this.iterations}),this.animation.playbackRate=this.playbackRate,this.animation.addEventListener("cancel",this.handleAnimationCancel),this.animation.addEventListener("finish",this.handleAnimationFinish),this.play?(this.hasStarted=!0,this.emit("sl-start")):this.animation.pause(),!0)}destroyAnimation(){this.animation&&(this.animation.cancel(),this.animation.removeEventListener("cancel",this.handleAnimationCancel),this.animation.removeEventListener("finish",this.handleAnimationFinish),this.hasStarted=!1)}handleAnimationChange(){this.hasUpdated&&this.createAnimation()}handlePlayChange(){return this.animation?(this.play&&!this.hasStarted&&(this.hasStarted=!0,this.emit("sl-start")),this.play?this.animation.play():this.animation.pause(),!0):!1}handlePlaybackRateChange(){this.animation&&(this.animation.playbackRate=this.playbackRate)}cancel(){var e;(e=this.animation)==null||e.cancel()}finish(){var e;(e=this.animation)==null||e.finish()}render(){return x` <slot @slotchange=${this.handleSlotChange}></slot> `}};xt.styles=[D,cb];l([Lc("slot")],xt.prototype,"defaultSlot",2);l([p()],xt.prototype,"name",2);l([p({type:Boolean,reflect:!0})],xt.prototype,"play",2);l([p({type:Number})],xt.prototype,"delay",2);l([p()],xt.prototype,"direction",2);l([p({type:Number})],xt.prototype,"duration",2);l([p()],xt.prototype,"easing",2);l([p({attribute:"end-delay",type:Number})],xt.prototype,"endDelay",2);l([p()],xt.prototype,"fill",2);l([p({type:Number})],xt.prototype,"iterations",2);l([p({attribute:"iteration-start",type:Number})],xt.prototype,"iterationStart",2);l([p({attribute:!1})],xt.prototype,"keyframes",2);l([p({attribute:"playback-rate",type:Number})],xt.prototype,"playbackRate",2);l([C(["name","delay","direction","duration","easing","endDelay","fill","iterations","iterationsStart","keyframes"])],xt.prototype,"handleAnimationChange",1);l([C("play")],xt.prototype,"handlePlayChange",1);l([C("playbackRate")],xt.prototype,"handlePlaybackRateChange",1);xt.define("sl-animation");var db=Object.defineProperty,hb=Object.getOwnPropertyDescriptor,Fs=(e,t,o,i)=>{for(var s=i>1?void 0:i?hb(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&db(t,o,s),s};let Po=class extends de{constructor(){super(...arguments),this.open=!1,this.loading=!1,this.disabled=!1,this.clicked=async e=>{this.loading=!this.loading,this.open=!this.open}}connectedCallback(){super.connectedCallback()}render(){return x` 
      <div class="container">
        <uc-button 
          ?loading=${this.loading} 
          ?disabled=${this.disabled} 
          @click=${this.clicked}>
          Yama
        </uc-button>
      </div>
    `}};Po.styles=O`
    :host {
      width: 100%;
      height: 100%;
    }

    .container {
      position: relative;
      display: flex;
      width: 50%;
      height: 50%;
      align-items: center;
      justify-content: center;
      border: 1px dashed red;
      box-sizing: border-box;
    }

    .container > div {
      overflow-wrap: anywhere;
    }
  `;Fs([z()],Po.prototype,"open",2);Fs([z()],Po.prototype,"loading",2);Fs([z()],Po.prototype,"disabled",2);Po=Fs([Si("home-page")],Po);var ub=Object.defineProperty,pb=Object.getOwnPropertyDescriptor,va=(e,t,o,i)=>{for(var s=i>1?void 0:i?pb(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&ub(t,o,s),s};let $i=class extends de{constructor(){super(...arguments),this.status=404,this.message="Page Not Found"}render(){return x`
      <div class="container">
        <h1>Error ${this.status}</h1>
        <p>${this.message}</p>
      </div>
    `}};$i.styles=O`
    .container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100%;
      text-align: center;
      color: #bbbbbb;
    }

    h1 {
      font-size: 48px;
      margin: 0;
    }

    p {
      font-size: 24px;
      margin: 16px 0 0;
    }
  `;va([p({type:Number})],$i.prototype,"status",2);va([p({type:String})],$i.prototype,"message",2);$i=va([Si("error-page")],$i);class Oo extends Error{constructor(t){super(),this.name="CanceledError",t instanceof Error?(this.message=t.message,this.cause=t.cause,this.stack=t.stack):t instanceof ProgressEvent?(this.message="Request was cancelled",this.cause=t):this.message=typeof t=="string"?t:"Request was cancelled"}}class zr{constructor(){this._isCancelled=!1,this.controller=new AbortController,this.callbacks=[]}get signal(){return this.controller.signal}get isCancelled(){return this._isCancelled}register(t){this.callbacks.push(t)}cancel(t){if(!this._isCancelled){this._isCancelled=!0,this.controller.abort();for(const o of this.callbacks)try{o(t)}catch(i){console.error("CancelToken callback error:",i)}}}throwIfCancelled(){if(this._isCancelled)throw new Oo("Operation has been cancelled")}}class fb{constructor(t){this._response=t}get ok(){return this._response.ok}get status(){return this._response.status}get statusText(){return this._response.statusText}get headers(){return this._response.headers}get url(){return this._response.url}get redirected(){return this._response.redirected}text(){return this._response.text()}json(){return this._response.json()}arrayBuffer(){return this._response.arrayBuffer()}async bytes(){const t=await this._response.arrayBuffer();return new Uint8Array(t)}blob(){return this._response.blob()}formData(){return this._response.formData()}async*stream(){var t;try{const o=(t=this._response.body)==null?void 0:t.getReader(),i=new TextDecoder("utf-8");if(!o)throw new Error("Response body is not available for streaming.");let s=!1,r="";const a=/\r?\n\r?\n/;for(;!s;){const{value:n,done:c}=await o.read();if(s=c,n){r+=i.decode(n,{stream:!0});const d=r.split(a);if(d.length>1){for(let u=0;u<d.length-1;u++){const h=this.parse(d[u].trim());h&&(yield h)}r=d[d.length-1]}}}if(r){const n=this.parse(r.trim());n&&(yield n)}}catch(o){throw o instanceof Error&&o.name==="AbortError"?new Oo(o):o}}parse(t){if(!t)return;const o=t.split(/\r?\n/).filter(s=>s.trim()!=="");if(o.length===0)return;const i={event:"message",data:""};for(const s of o){const r=s.indexOf(":");if(r===-1)continue;const a=s.slice(0,r).trim(),n=s.slice(r+1).trim();if(a==="event")i.event=n;else if(a==="data")i.data=i.data?`${i.data}
${n}`:n;else if(a==="id")i.id=n;else if(a==="retry"){const c=parseInt(n,10);isNaN(c)||(i.retry=c)}}return i.data?i:void 0}}class mb{constructor(t){this.baseUrl=t.baseUrl,this.headers=t.headers,this.timeout=t.timeout,this.credentials=t.credentials,this.mode=t.mode,this.cache=t.cache,this.keepalive=t.keepalive}async head(t,o){const{baseUrl:i,path:s,query:r}=this.parseUrl(t);return this.send({method:"HEAD",baseUrl:i,path:s,query:r},o)}async get(t,o){const{baseUrl:i,path:s,query:r}=this.parseUrl(t);return this.send({method:"GET",baseUrl:i,path:s,query:r},o)}async post(t,o,i){const{baseUrl:s,path:r,query:a}=this.parseUrl(t);return this.send({method:"POST",baseUrl:s,path:r,query:a,body:o},i)}async put(t,o,i){const{baseUrl:s,path:r,query:a}=this.parseUrl(t);return this.send({method:"PUT",baseUrl:s,path:r,query:a,body:o},i)}async patch(t,o,i){const{baseUrl:s,path:r,query:a}=this.parseUrl(t);return this.send({method:"PATCH",baseUrl:s,path:r,query:a,body:o},i)}async delete(t,o){const{baseUrl:i,path:s,query:r}=this.parseUrl(t);return this.send({method:"DELETE",baseUrl:i,path:s,query:r},o)}async send(t,o){const i=this.buildUrl(t.baseUrl??this.baseUrl,t.path,t.query),s=new Headers(t.headers);this.headers&&Object.entries(this.headers).forEach(([d,u])=>{s.append(d,u)});let r=t.body;typeof r=="string"?s.set("Content-Type","text/plain;charset=UTF-8"):typeof r=="object"&&(r instanceof Blob?s.set("Content-Type",r.type||"application/octet-stream"):r instanceof ArrayBuffer?s.set("Content-Type","application/octet-stream"):(s.set("Content-Type","application/json;charset=UTF-8"),r=JSON.stringify(r)));const a=o||new zr,n=t.timeout??this.timeout,c=n?setTimeout(()=>a.cancel(),n):null;try{const d=await fetch(i.toString(),{method:t.method,headers:s,body:r,cache:t.cache??this.cache,credentials:t.credentials??this.credentials,mode:t.mode??this.mode,keepalive:t.keepalive??this.keepalive,signal:a.signal});return new fb(d)}catch(d){throw d instanceof Error&&d.name==="AbortError"?new Oo(d):d}finally{c&&clearTimeout(c)}}async*upload(t,o){const i=this.buildUrl(t.baseUrl??this.baseUrl,t.path,t.query),s=new XMLHttpRequest;s.open(t.method,i,!0);const r=t.timeout??this.timeout;r&&(s.timeout=r);const a=t.credentials??this.credentials;let n;if(a&&(s.withCredentials=a==="include"||a==="same-origin"),t.headers&&Object.entries(t.headers).forEach(([d,u])=>{s.setRequestHeader(d,u)}),this.headers&&Object.entries(this.headers).forEach(([d,u])=>{s.setRequestHeader(d,u)}),t.body instanceof FormData)n=t.body;else if(Array.isArray(t.body)){const d=new FormData;for(let u=0;u<t.body.length;u++)d.append("files",t.body[u]);n=d}else{const d=new FormData;d.append("file",t.body),n=d}const c=[];for(s.upload.onprogress=d=>{var u;if(d.lengthComputable){const h=Math.round(d.loaded/d.total*100);(u=c.shift())==null||u({type:"progress",loaded:d.loaded,total:d.total,progress:h})}},s.onload=()=>{var d,u;if(s.status>=200&&s.status<300){const h={},f=s.getAllResponseHeaders().split(`\r
`);for(const m of f){const g=m.indexOf(": ");if(g!==-1){const b=m.substring(0,g).trim(),k=m.substring(g+2).trim();h[b]?h[b]=`${h[b]}, ${k}`:h[b]=k}}(d=c.shift())==null||d({type:"success",status:s.status,headers:h,body:s.response})}else(u=c.shift())==null||u({type:"failure",status:s.status,message:`Upload failed with status ${s.status}`})},s.onerror=()=>{var d;(d=c.shift())==null||d({type:"failure",message:"Network error occurred"})},s.ontimeout=()=>{var d;(d=c.shift())==null||d({type:"failure",message:"Request timed out"})},s.onabort=d=>{throw new Oo(d)},o&&o.register(()=>{s.abort()}),s.send(n);;){const d=await new Promise(u=>{c.push(u)});if(yield d,d.type==="success"||d.type==="failure")break}}download(t){const o=this.buildUrl(t.baseUrl??this.baseUrl,t.path,t.query),i=document.createElement("a");i.style.display="none",i.href=o.toString(),i.download="",document.body.appendChild(i),i.click(),document.body.removeChild(i)}buildUrl(t,o,i){if(!t)throw new Error("Base URL is required for building the request URL.");const s=o?new URL(t.endsWith("/")&&o.startsWith("/")?t+o.slice(1):t+o):new URL(t);return i&&Object.entries(i).forEach(([r,a])=>{a===null&&a===void 0||(Array.isArray(a)?a:[a]).forEach(n=>s.searchParams.append(r,n))}),s}parseUrl(t){if(t.startsWith("http://")||t.startsWith("https://"))return{baseUrl:t};{if(!this.baseUrl)throw new Error("Base URL is required for relative URLs.");const[o,i]=t.split("?",2),s=i?Object.fromEntries(new URLSearchParams(i)):void 0;return{baseUrl:this.baseUrl,path:o,query:s}}}}console.log(!1);const ya=class ya{static async*conversation(t,o){const i=await this.controller.send({method:"POST",path:"/api/conversation",body:t},o);if(!i.ok){const s=await i.json();throw new Error(s.message)}for await(const s of i.stream()){if(s.event==="delta"&&(yield JSON.parse(s.data)),s.event==="cancelled")throw new Oo(s.data);if(s.event==="error")throw new Error(s.data)}}static async*upload(t){for await(const o of this.controller.upload({method:"POST",path:"/api/upload",body:t}))yield o}static async download(t){this.controller.download({path:`/api/download/${t}`})}};ya.controller=new mb({baseUrl:window.location.origin});let Ci=ya;var gb=Object.defineProperty,bb=Object.getOwnPropertyDescriptor,bo=(e,t,o,i)=>{for(var s=i>1?void 0:i?bb(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&gb(t,o,s),s};let Ce=class extends de{constructor(){super(...arguments),this.canceller=new zr,this.loading=!1,this.models=[],this.messages=[],this.tokenUsage=0,this.submit=async e=>{const o={role:"user",content:[{type:"text",value:e.detail}],timestamp:new Date().toISOString()};this.messages=[...this.messages,o],await this.generate()},this.change=async e=>{this.messages=e.detail;const t=this.messages[this.messages.length-1];if(t.role==="assistant"){for(const o of t.content||[])if(o.type==="tool"&&o.approvalStatus==="requires")return;this.generate()}},this.generate=async()=>{var e,t;try{if(this.error=void 0,this.loading=!0,!this.selectedModel){this.error={status:"warning",message:"모델을 선택하세요."};return}const o={provider:this.selectedModel.provider,model:this.selectedModel.modelId,messages:this.messages,instructions:"유저가 요구하지 않는 한 너무 길게 대답하지 마세요."};for await(const i of Ci.conversation(o,this.canceller)){let s=this.messages[this.messages.length-1];s.role!=="assistant"&&(this.messages=[...this.messages,{role:"assistant",content:[]}],s=this.messages[this.messages.length-1]);const r=i.data;if(r){const a=r.index||0;s.content||(s.content=[]);const n=(e=s.content)==null?void 0:e.at(a);n?n.type==="text"&&r.type==="text"?(n.value||(n.value=""),n.value+=r.value||""):n.type==="thinking"&&r.type==="thinking"?(n.id||(n.id=r.id),n.value||(n.value=""),n.value+=r.value||""):s.content[a]=r:(t=s.content)==null||t.push(r),this.messages=[...this.messages]}i.timestamp&&(s.timestamp=i.timestamp,this.messages=[...this.messages]),i.tokenUsage&&(this.tokenUsage=i.tokenUsage.totalTokens)}localStorage.setItem("messages",JSON.stringify(this.messages))}catch(o){o instanceof Oo?this.error={status:"info",message:"요청이 취소되었습니다."}:this.error=o instanceof Error?{status:"danger",message:o.message}:{status:"danger",message:"알 수 없는 오류가 발생했습니다."}}finally{this.loading=!1,this.canceller=new zr,this.canceller.register(()=>{console.warn("등록된 취소 요청")})}}}firstUpdated(e){super.firstUpdated(e),fetch("/models.json").then(t=>t.text()).then(t=>JSON.parse(t)).then(t=>{this.models=t.models,this.selectedModel=this.models.at(0)||void 0}),this.messages=JSON.parse(localStorage.getItem("messages")||"[]")}render(){var e,t,o;return x`
      <div class="container">
        <status-panel
          .status=${this.loading?"processing":"pending"}
        ></status-panel>

        <token-panel
          .value=${this.tokenUsage}
          .maxValue=${((e=this.selectedModel)==null?void 0:e.contextWindow)||0}
        ></token-panel>

        <message-alert
          ?open=${this.error!==void 0}
          timeout="5000"
          status=${((t=this.error)==null?void 0:t.status)||"danger"}
          .value=${(o=this.error)==null?void 0:o.message}
        ></message-alert>

        <message-box
          .messages=${this.messages}
          @tool-change=${this.change}
        ></message-box>

        <message-input
          .loading=${this.loading}
          placeholder="Type a message..."
          @send=${this.submit}
          @stop=${()=>this.canceller.cancel()}>
          <uc-model-select
            .models=${this.models}
            .selectedModel=${this.selectedModel}
            @select=${i=>this.selectedModel=i.detail}
          ></uc-model-select>
          <div style="flex: 1"></div>
          <uc-clear-button
            @click=${this.clear}
          ></uc-clear-button>
        </message-input>
      </div>
    `}clear(){this.messages=[],localStorage.removeItem("messages")}};Ce.styles=O`
    :host {
      width: 100%;
      height: 100%;
    }

    .container {
      position: relative;
      width: 100%;
      height: 100%;
      min-width: 320px;
      min-height: 480px;
      background-color: var(--uc-background-color-0);
      overflow: hidden;
    }

    status-panel {
      position: absolute;
      z-index: 100;
      top: 16px;
      left: 16px;
    }

    token-panel {
      position: absolute;
      z-index: 100;
      top: 120px;
      left: 16px;
    }

    message-alert {
      position: absolute;
      z-index: 100;
      max-width: 60%;
      top: 16px;
      left: 50%;
      transform: translateX(-50%);
    }

    message-box {
      position: relative;
      width: 100%;
      height: 100%;

      --messages-padding: 10px 20% 160px 20%;
    }

    message-input {
      position: absolute;
      z-index: 100;
      width: 60%;
      bottom: 16px;
      left: 50%;
      transform: translateX(-50%);
    }
  `;bo([z()],Ce.prototype,"loading",2);bo([z()],Ce.prototype,"models",2);bo([z()],Ce.prototype,"selectedModel",2);bo([z()],Ce.prototype,"messages",2);bo([z()],Ce.prototype,"tokenUsage",2);bo([z()],Ce.prototype,"error",2);Ce=bo([Si("chat-page")],Ce);var vb=Object.defineProperty,yb=Object.getOwnPropertyDescriptor,Ui=(e,t,o,i)=>{for(var s=i>1?void 0:i?yb(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&vb(t,o,s),s};let ao=class extends de{constructor(){super(...arguments),this.files=[],this.uploading=!1,this.message="",this.progress=0}render(){return x` 
      <div class="container">
        <h2>File Upload & Download Test</h2>
        
        <div class="upload-section">
          <input 
            type="file" 
            id="fileInput" 
            @change=${this.handleFileChange} 
            ?disabled=${this.uploading}
            multiple
          />
          <button 
            @click=${this.uploadFiles} 
            ?disabled=${this.uploading}>
            ${this.uploading?"Uploading...":"Upload Files"}
          </button>
        </div>

        ${this.uploading?x`
          <div class="progress-bar">
            <div class="progress" style="width: ${this.progress}%"></div>
          </div>
        `:""}
        
        ${this.message?x`<div class="message">${this.message}</div>`:""}
        
        <div class="file-list">
          <h3>Uploaded Files</h3>
          ${this.files.length===0?x`<p>No files uploaded yet.</p>`:x`
              <table>
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Type</th>
                    <th>Size</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  ${this.files.map(e=>x`
                    <tr>
                      <td>${e.name}</td>
                      <td>${e.type}</td>
                      <td>${this.formatFileSize(e.size)}</td>
                      <td>
                        <button @click=${()=>this.downloadFile(e)}>Download</button>
                        <button @click=${()=>this.deleteFile(e)}>Delete</button>
                      </td>
                    </tr>
                  `)}
                </tbody>
              </table>
            `}
        </div>
      </div>
    `}handleFileChange(e){const t=e.target;this.message=t.files?`${t.files.length} file(s) selected`:""}async uploadFiles(){var t;const e=(t=this.shadowRoot)==null?void 0:t.querySelector("#fileInput");if(!e.files||e.files.length===0){this.message="Please select files to upload";return}this.uploading=!0,this.progress=0,this.message="Uploading files...";try{const o=new FormData;Array.from(e.files).forEach(s=>{o.append("files",s)});for await(const s of Ci.upload(o))s.type==="progress"?this.progress=s.progress:s.type==="success"?this.message=`Uploaded ${s.body} successfully`:s.type==="failure"&&(this.message=`Upload failed: ${s.message}`);const i=Array.from(e.files).map(s=>({name:s.name,size:s.size,type:s.type,uploadTime:new Date().toISOString(),id:Math.random().toString(36).substr(2,9)}));this.files=[...this.files,...i],this.message=`${e.files.length} file(s) uploaded successfully`,e.value=""}catch(o){this.message=`Upload failed: ${o instanceof Error?o.message:"Unknown error"}`}finally{setTimeout(()=>{this.uploading=!1},500)}}async downloadFile(e){try{this.message=`Downloading ${e.name}...`,Ci.download(e.name),this.message=`Downloaded ${e.name} successfully`}catch(t){this.message=`Download failed: ${t instanceof Error?t.message:"Unknown error"}`}}async deleteFile(e){try{this.message=`Deleting ${e.name}...`,await new Promise(t=>setTimeout(t,1e3)),this.files=this.files.filter(t=>t.id!==e.id),this.message=`Deleted ${e.name} successfully`}catch(t){this.message=`Delete failed: ${t instanceof Error?t.message:"Unknown error"}`}}formatFileSize(e){if(e===0)return"0 Bytes";const t=1024,o=["Bytes","KB","MB","GB","TB"],i=Math.floor(Math.log(e)/Math.log(t));return parseFloat((e/Math.pow(t,i)).toFixed(2))+" "+o[i]}};ao.styles=O`
    :host {
      width: 100%;
      height: 100%;
    }

    .container {
      position: relative;
      display: flex;
      flex-direction: column;
      width: 100%;
      height: 100%;
      min-width: 320px;
      min-height: 480px;
      color: var(--uc-twxt-color-high);
      background-color: var(--uc-background-color-0);
      overflow: hidden;
      padding: 20px;
      box-sizing: border-box;
    }

    h2, h3 {
      margin-top: 0;
      margin-bottom: 20px;
    }

    .upload-section {
      display: flex;
      margin-bottom: 20px;
      gap: 10px;
    }

    button {
      padding: 8px 16px;
      cursor: pointer;
      background-color: #4285f4;
      color: white;
      border: none;
      border-radius: 4px;
    }

    button:hover {
      background-color: #3367d6;
    }

    button:disabled {
      background-color: #cccccc;
      cursor: not-allowed;
    }

    .progress-bar {
      width: 100%;
      height: 10px;
      background-color: #e0e0e0;
      border-radius: 5px;
      margin-bottom: 20px;
      overflow: hidden;
    }

    .progress {
      height: 100%;
      background-color: #4285f4;
      transition: width 0.3s ease;
    }

    .message {
      margin-bottom: 20px;
      padding: 10px;
      border-radius: 4px;
      background-color: #f8f9fa;
      border-left: 4px solid #4285f4;
    }

    table {
      width: 100%;
      border-collapse: collapse;
    }

    th, td {
      padding: 8px 12px;
      text-align: left;
      border-bottom: 1px solid #e0e0e0;
    }

    th {
      background-color: #f8f9fa;
      font-weight: 500;
    }

    .file-list {
      flex: 1;
      overflow: auto;
    }
  `;Ui([z()],ao.prototype,"files",2);Ui([z()],ao.prototype,"uploading",2);Ui([z()],ao.prototype,"message",2);Ui([z()],ao.prototype,"progress",2);ao=Ui([Si("file-page")],ao);var wb=Object.defineProperty,xb=Object.getOwnPropertyDescriptor,lc=(e,t,o,i)=>{for(var s=i>1?void 0:i?xb(t,o):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(s=(i?a(t,o,s):a(s))||s);return i&&s&&wb(t,o,s),s};let _s=class extends de{constructor(){super(...arguments),this.router=new Mc(this,[{path:"/",render:()=>x`<home-page></home-page>`},{path:"/chat",render:()=>x`<chat-page></chat-page>`},{path:"/file",render:()=>x`<file-page></file-page>`}],{fallback:{render:()=>x`<error-page></error-page>`}}),this.mode="fixed",this.toggle=async()=>{const e=document.documentElement;e.hasAttribute("theme")?e.removeAttribute("theme"):e.setAttribute("theme","dark")}}render(){return x`
      <div class="flexible-box">
        ${this.router.outlet()}
      </div>
      <div class="toggler theme" @click=${this.toggle}>T</div>
    `}};_s.styles=O`
    :host {
      position: relative;
      display: flex;
      align-items: center;
      justify-content: center;
      width: 100vw;
      height: 100vh;
    }
    :host([mode="fixed"]) .flexible-box {
      width: 100%;
      height: 100%;
      resize: none;
    }

    .flexible-box {
      position: relative;
      display: block;
      width: 60%;
      height: 80%;
      padding: 20px;
      border: 1px dashed gray;
      resize: both;
      overflow: auto;
      box-sizing: border-box;
    }

    .toggler {
      position: absolute;
      width: 24px;
      height: 24px;
      background-color: gray;
      color: #fff;
      text-align: center;
      line-height: 24px;
      user-select: none;
      cursor: pointer;
    }
    .toggler.theme {
      top: 10px;
      right: 10px;
    }
    .toggler:hover {
      background-color: #888;
    }
    .toggler:active {
      background-color: #555;
    }
  `;lc([p({type:String,reflect:!0})],_s.prototype,"mode",2);_s=lc([Si("main-app")],_s);

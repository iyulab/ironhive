var Xl=Object.defineProperty;var Zl=(e,t,s)=>t in e?Xl(e,t,{enumerable:!0,configurable:!0,writable:!0,value:s}):e[t]=s;var st=(e,t,s)=>Zl(e,typeof t!="symbol"?t+"":t,s);(function(){const t=document.createElement("link").relList;if(t&&t.supports&&t.supports("modulepreload"))return;for(const o of document.querySelectorAll('link[rel="modulepreload"]'))i(o);new MutationObserver(o=>{for(const r of o)if(r.type==="childList")for(const a of r.addedNodes)a.tagName==="LINK"&&a.rel==="modulepreload"&&i(a)}).observe(document,{childList:!0,subtree:!0});function s(o){const r={};return o.integrity&&(r.integrity=o.integrity),o.referrerPolicy&&(r.referrerPolicy=o.referrerPolicy),o.crossOrigin==="use-credentials"?r.credentials="include":o.crossOrigin==="anonymous"?r.credentials="omit":r.credentials="same-origin",r}function i(o){if(o.ep)return;o.ep=!0;const r=s(o);fetch(o.href,r)}})();/**
 * @license
 * Copyright 2019 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const to=globalThis,zr=to.ShadowRoot&&(to.ShadyCSS===void 0||to.ShadyCSS.nativeShadow)&&"adoptedStyleSheets"in Document.prototype&&"replace"in CSSStyleSheet.prototype,Er=Symbol(),Ea=new WeakMap;let Pn=class{constructor(t,s,i){if(this._$cssResult$=!0,i!==Er)throw Error("CSSResult is not constructable. Use `unsafeCSS` or `css` instead.");this.cssText=t,this.t=s}get styleSheet(){let t=this.o;const s=this.t;if(zr&&t===void 0){const i=s!==void 0&&s.length===1;i&&(t=Ea.get(s)),t===void 0&&((this.o=t=new CSSStyleSheet).replaceSync(this.cssText),i&&Ea.set(s,t))}return t}toString(){return this.cssText}};const Ql=e=>new Pn(typeof e=="string"?e:e+"",void 0,Er),A=(e,...t)=>{const s=e.length===1?e[0]:t.reduce((i,o,r)=>i+(a=>{if(a._$cssResult$===!0)return a.cssText;if(typeof a=="number")return a;throw Error("Value passed to 'css' function must be a 'css' function result: "+a+". Use 'unsafeCSS' to pass non-literal values, but take care to ensure page security.")})(o)+e[r+1],e[0]);return new Pn(s,e,Er)},Jl=(e,t)=>{if(zr)e.adoptedStyleSheets=t.map(s=>s instanceof CSSStyleSheet?s:s.styleSheet);else for(const s of t){const i=document.createElement("style"),o=to.litNonce;o!==void 0&&i.setAttribute("nonce",o),i.textContent=s.cssText,e.appendChild(i)}},Oa=zr?e=>e:e=>e instanceof CSSStyleSheet?(t=>{let s="";for(const i of t.cssRules)s+=i.cssText;return Ql(s)})(e):e;/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const{is:tc,defineProperty:ec,getOwnPropertyDescriptor:sc,getOwnPropertyNames:ic,getOwnPropertySymbols:oc,getPrototypeOf:rc}=Object,Ve=globalThis,Pa=Ve.trustedTypes,ac=Pa?Pa.emptyScript:"",Wo=Ve.reactiveElementPolyfillSupport,ai=(e,t)=>e,Ts={toAttribute(e,t){switch(t){case Boolean:e=e?ac:null;break;case Object:case Array:e=e==null?e:JSON.stringify(e)}return e},fromAttribute(e,t){let s=e;switch(t){case Boolean:s=e!==null;break;case Number:s=e===null?null:Number(e);break;case Object:case Array:try{s=JSON.parse(e)}catch{s=null}}return s}},Or=(e,t)=>!tc(e,t),Da={attribute:!0,type:String,converter:Ts,reflect:!1,hasChanged:Or};Symbol.metadata??(Symbol.metadata=Symbol("metadata")),Ve.litPropertyMetadata??(Ve.litPropertyMetadata=new WeakMap);class Cs extends HTMLElement{static addInitializer(t){this._$Ei(),(this.l??(this.l=[])).push(t)}static get observedAttributes(){return this.finalize(),this._$Eh&&[...this._$Eh.keys()]}static createProperty(t,s=Da){if(s.state&&(s.attribute=!1),this._$Ei(),this.elementProperties.set(t,s),!s.noAccessor){const i=Symbol(),o=this.getPropertyDescriptor(t,i,s);o!==void 0&&ec(this.prototype,t,o)}}static getPropertyDescriptor(t,s,i){const{get:o,set:r}=sc(this.prototype,t)??{get(){return this[s]},set(a){this[s]=a}};return{get(){return o==null?void 0:o.call(this)},set(a){const l=o==null?void 0:o.call(this);r.call(this,a),this.requestUpdate(t,l,i)},configurable:!0,enumerable:!0}}static getPropertyOptions(t){return this.elementProperties.get(t)??Da}static _$Ei(){if(this.hasOwnProperty(ai("elementProperties")))return;const t=rc(this);t.finalize(),t.l!==void 0&&(this.l=[...t.l]),this.elementProperties=new Map(t.elementProperties)}static finalize(){if(this.hasOwnProperty(ai("finalized")))return;if(this.finalized=!0,this._$Ei(),this.hasOwnProperty(ai("properties"))){const s=this.properties,i=[...ic(s),...oc(s)];for(const o of i)this.createProperty(o,s[o])}const t=this[Symbol.metadata];if(t!==null){const s=litPropertyMetadata.get(t);if(s!==void 0)for(const[i,o]of s)this.elementProperties.set(i,o)}this._$Eh=new Map;for(const[s,i]of this.elementProperties){const o=this._$Eu(s,i);o!==void 0&&this._$Eh.set(o,s)}this.elementStyles=this.finalizeStyles(this.styles)}static finalizeStyles(t){const s=[];if(Array.isArray(t)){const i=new Set(t.flat(1/0).reverse());for(const o of i)s.unshift(Oa(o))}else t!==void 0&&s.push(Oa(t));return s}static _$Eu(t,s){const i=s.attribute;return i===!1?void 0:typeof i=="string"?i:typeof t=="string"?t.toLowerCase():void 0}constructor(){super(),this._$Ep=void 0,this.isUpdatePending=!1,this.hasUpdated=!1,this._$Em=null,this._$Ev()}_$Ev(){var t;this._$ES=new Promise(s=>this.enableUpdating=s),this._$AL=new Map,this._$E_(),this.requestUpdate(),(t=this.constructor.l)==null||t.forEach(s=>s(this))}addController(t){var s;(this._$EO??(this._$EO=new Set)).add(t),this.renderRoot!==void 0&&this.isConnected&&((s=t.hostConnected)==null||s.call(t))}removeController(t){var s;(s=this._$EO)==null||s.delete(t)}_$E_(){const t=new Map,s=this.constructor.elementProperties;for(const i of s.keys())this.hasOwnProperty(i)&&(t.set(i,this[i]),delete this[i]);t.size>0&&(this._$Ep=t)}createRenderRoot(){const t=this.shadowRoot??this.attachShadow(this.constructor.shadowRootOptions);return Jl(t,this.constructor.elementStyles),t}connectedCallback(){var t;this.renderRoot??(this.renderRoot=this.createRenderRoot()),this.enableUpdating(!0),(t=this._$EO)==null||t.forEach(s=>{var i;return(i=s.hostConnected)==null?void 0:i.call(s)})}enableUpdating(t){}disconnectedCallback(){var t;(t=this._$EO)==null||t.forEach(s=>{var i;return(i=s.hostDisconnected)==null?void 0:i.call(s)})}attributeChangedCallback(t,s,i){this._$AK(t,i)}_$EC(t,s){var r;const i=this.constructor.elementProperties.get(t),o=this.constructor._$Eu(t,i);if(o!==void 0&&i.reflect===!0){const a=(((r=i.converter)==null?void 0:r.toAttribute)!==void 0?i.converter:Ts).toAttribute(s,i.type);this._$Em=t,a==null?this.removeAttribute(o):this.setAttribute(o,a),this._$Em=null}}_$AK(t,s){var r;const i=this.constructor,o=i._$Eh.get(t);if(o!==void 0&&this._$Em!==o){const a=i.getPropertyOptions(o),l=typeof a.converter=="function"?{fromAttribute:a.converter}:((r=a.converter)==null?void 0:r.fromAttribute)!==void 0?a.converter:Ts;this._$Em=o,this[o]=l.fromAttribute(s,a.type),this._$Em=null}}requestUpdate(t,s,i){if(t!==void 0){if(i??(i=this.constructor.getPropertyOptions(t)),!(i.hasChanged??Or)(this[t],s))return;this.P(t,s,i)}this.isUpdatePending===!1&&(this._$ES=this._$ET())}P(t,s,i){this._$AL.has(t)||this._$AL.set(t,s),i.reflect===!0&&this._$Em!==t&&(this._$Ej??(this._$Ej=new Set)).add(t)}async _$ET(){this.isUpdatePending=!0;try{await this._$ES}catch(s){Promise.reject(s)}const t=this.scheduleUpdate();return t!=null&&await t,!this.isUpdatePending}scheduleUpdate(){return this.performUpdate()}performUpdate(){var i;if(!this.isUpdatePending)return;if(!this.hasUpdated){if(this.renderRoot??(this.renderRoot=this.createRenderRoot()),this._$Ep){for(const[r,a]of this._$Ep)this[r]=a;this._$Ep=void 0}const o=this.constructor.elementProperties;if(o.size>0)for(const[r,a]of o)a.wrapped!==!0||this._$AL.has(r)||this[r]===void 0||this.P(r,this[r],a)}let t=!1;const s=this._$AL;try{t=this.shouldUpdate(s),t?(this.willUpdate(s),(i=this._$EO)==null||i.forEach(o=>{var r;return(r=o.hostUpdate)==null?void 0:r.call(o)}),this.update(s)):this._$EU()}catch(o){throw t=!1,this._$EU(),o}t&&this._$AE(s)}willUpdate(t){}_$AE(t){var s;(s=this._$EO)==null||s.forEach(i=>{var o;return(o=i.hostUpdated)==null?void 0:o.call(i)}),this.hasUpdated||(this.hasUpdated=!0,this.firstUpdated(t)),this.updated(t)}_$EU(){this._$AL=new Map,this.isUpdatePending=!1}get updateComplete(){return this.getUpdateComplete()}getUpdateComplete(){return this._$ES}shouldUpdate(t){return!0}update(t){this._$Ej&&(this._$Ej=this._$Ej.forEach(s=>this._$EC(s,this[s]))),this._$EU()}updated(t){}firstUpdated(t){}}Cs.elementStyles=[],Cs.shadowRootOptions={mode:"open"},Cs[ai("elementProperties")]=new Map,Cs[ai("finalized")]=new Map,Wo==null||Wo({ReactiveElement:Cs}),(Ve.reactiveElementVersions??(Ve.reactiveElementVersions=[])).push("2.0.4");/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const ni=globalThis,oo=ni.trustedTypes,Ia=oo?oo.createPolicy("lit-html",{createHTML:e=>e}):void 0,Dn="$lit$",Ue=`lit$${Math.random().toFixed(9).slice(2)}$`,In="?"+Ue,nc=`<${In}>`,is=document,bi=()=>is.createComment(""),vi=e=>e===null||typeof e!="object"&&typeof e!="function",Pr=Array.isArray,lc=e=>Pr(e)||typeof(e==null?void 0:e[Symbol.iterator])=="function",qo=`[ 	
\f\r]`,qs=/<(?:(!--|\/[^a-zA-Z])|(\/?[a-zA-Z][^>\s]*)|(\/?$))/g,Ra=/-->/g,La=/>/g,Qe=RegExp(`>|${qo}(?:([^\\s"'>=/]+)(${qo}*=${qo}*(?:[^ 	
\f\r"'\`<>=]|("|')|))|$)`,"g"),Ma=/'/g,Na=/"/g,Rn=/^(?:script|style|textarea|title)$/i,cc=e=>(t,...s)=>({_$litType$:e,strings:t,values:s}),v=cc(1),Nt=Symbol.for("lit-noChange"),G=Symbol.for("lit-nothing"),Fa=new WeakMap,es=is.createTreeWalker(is,129);function Ln(e,t){if(!Pr(e)||!e.hasOwnProperty("raw"))throw Error("invalid template strings array");return Ia!==void 0?Ia.createHTML(t):t}const dc=(e,t)=>{const s=e.length-1,i=[];let o,r=t===2?"<svg>":t===3?"<math>":"",a=qs;for(let l=0;l<s;l++){const c=e[l];let h,u,p=-1,f=0;for(;f<c.length&&(a.lastIndex=f,u=a.exec(c),u!==null);)f=a.lastIndex,a===qs?u[1]==="!--"?a=Ra:u[1]!==void 0?a=La:u[2]!==void 0?(Rn.test(u[2])&&(o=RegExp("</"+u[2],"g")),a=Qe):u[3]!==void 0&&(a=Qe):a===Qe?u[0]===">"?(a=o??qs,p=-1):u[1]===void 0?p=-2:(p=a.lastIndex-u[2].length,h=u[1],a=u[3]===void 0?Qe:u[3]==='"'?Na:Ma):a===Na||a===Ma?a=Qe:a===Ra||a===La?a=qs:(a=Qe,o=void 0);const m=a===Qe&&e[l+1].startsWith("/>")?" ":"";r+=a===qs?c+nc:p>=0?(i.push(h),c.slice(0,p)+Dn+c.slice(p)+Ue+m):c+Ue+(p===-2?l:m)}return[Ln(e,r+(e[s]||"<?>")+(t===2?"</svg>":t===3?"</math>":"")),i]};class yi{constructor({strings:t,_$litType$:s},i){let o;this.parts=[];let r=0,a=0;const l=t.length-1,c=this.parts,[h,u]=dc(t,s);if(this.el=yi.createElement(h,i),es.currentNode=this.el.content,s===2||s===3){const p=this.el.content.firstChild;p.replaceWith(...p.childNodes)}for(;(o=es.nextNode())!==null&&c.length<l;){if(o.nodeType===1){if(o.hasAttributes())for(const p of o.getAttributeNames())if(p.endsWith(Dn)){const f=u[a++],m=o.getAttribute(p).split(Ue),b=/([.?@])?(.*)/.exec(f);c.push({type:1,index:r,name:b[2],strings:m,ctor:b[1]==="."?uc:b[1]==="?"?pc:b[1]==="@"?fc:Co}),o.removeAttribute(p)}else p.startsWith(Ue)&&(c.push({type:6,index:r}),o.removeAttribute(p));if(Rn.test(o.tagName)){const p=o.textContent.split(Ue),f=p.length-1;if(f>0){o.textContent=oo?oo.emptyScript:"";for(let m=0;m<f;m++)o.append(p[m],bi()),es.nextNode(),c.push({type:2,index:++r});o.append(p[f],bi())}}}else if(o.nodeType===8)if(o.data===In)c.push({type:2,index:r});else{let p=-1;for(;(p=o.data.indexOf(Ue,p+1))!==-1;)c.push({type:7,index:r}),p+=Ue.length-1}r++}}static createElement(t,s){const i=is.createElement("template");return i.innerHTML=t,i}}function zs(e,t,s=e,i){var a,l;if(t===Nt)return t;let o=i!==void 0?(a=s._$Co)==null?void 0:a[i]:s._$Cl;const r=vi(t)?void 0:t._$litDirective$;return(o==null?void 0:o.constructor)!==r&&((l=o==null?void 0:o._$AO)==null||l.call(o,!1),r===void 0?o=void 0:(o=new r(e),o._$AT(e,s,i)),i!==void 0?(s._$Co??(s._$Co=[]))[i]=o:s._$Cl=o),o!==void 0&&(t=zs(e,o._$AS(e,t.values),o,i)),t}class hc{constructor(t,s){this._$AV=[],this._$AN=void 0,this._$AD=t,this._$AM=s}get parentNode(){return this._$AM.parentNode}get _$AU(){return this._$AM._$AU}u(t){const{el:{content:s},parts:i}=this._$AD,o=((t==null?void 0:t.creationScope)??is).importNode(s,!0);es.currentNode=o;let r=es.nextNode(),a=0,l=0,c=i[0];for(;c!==void 0;){if(a===c.index){let h;c.type===2?h=new $i(r,r.nextSibling,this,t):c.type===1?h=new c.ctor(r,c.name,c.strings,this,t):c.type===6&&(h=new mc(r,this,t)),this._$AV.push(h),c=i[++l]}a!==(c==null?void 0:c.index)&&(r=es.nextNode(),a++)}return es.currentNode=is,o}p(t){let s=0;for(const i of this._$AV)i!==void 0&&(i.strings!==void 0?(i._$AI(t,i,s),s+=i.strings.length-2):i._$AI(t[s])),s++}}class $i{get _$AU(){var t;return((t=this._$AM)==null?void 0:t._$AU)??this._$Cv}constructor(t,s,i,o){this.type=2,this._$AH=G,this._$AN=void 0,this._$AA=t,this._$AB=s,this._$AM=i,this.options=o,this._$Cv=(o==null?void 0:o.isConnected)??!0}get parentNode(){let t=this._$AA.parentNode;const s=this._$AM;return s!==void 0&&(t==null?void 0:t.nodeType)===11&&(t=s.parentNode),t}get startNode(){return this._$AA}get endNode(){return this._$AB}_$AI(t,s=this){t=zs(this,t,s),vi(t)?t===G||t==null||t===""?(this._$AH!==G&&this._$AR(),this._$AH=G):t!==this._$AH&&t!==Nt&&this._(t):t._$litType$!==void 0?this.$(t):t.nodeType!==void 0?this.T(t):lc(t)?this.k(t):this._(t)}O(t){return this._$AA.parentNode.insertBefore(t,this._$AB)}T(t){this._$AH!==t&&(this._$AR(),this._$AH=this.O(t))}_(t){this._$AH!==G&&vi(this._$AH)?this._$AA.nextSibling.data=t:this.T(is.createTextNode(t)),this._$AH=t}$(t){var r;const{values:s,_$litType$:i}=t,o=typeof i=="number"?this._$AC(t):(i.el===void 0&&(i.el=yi.createElement(Ln(i.h,i.h[0]),this.options)),i);if(((r=this._$AH)==null?void 0:r._$AD)===o)this._$AH.p(s);else{const a=new hc(o,this),l=a.u(this.options);a.p(s),this.T(l),this._$AH=a}}_$AC(t){let s=Fa.get(t.strings);return s===void 0&&Fa.set(t.strings,s=new yi(t)),s}k(t){Pr(this._$AH)||(this._$AH=[],this._$AR());const s=this._$AH;let i,o=0;for(const r of t)o===s.length?s.push(i=new $i(this.O(bi()),this.O(bi()),this,this.options)):i=s[o],i._$AI(r),o++;o<s.length&&(this._$AR(i&&i._$AB.nextSibling,o),s.length=o)}_$AR(t=this._$AA.nextSibling,s){var i;for((i=this._$AP)==null?void 0:i.call(this,!1,!0,s);t&&t!==this._$AB;){const o=t.nextSibling;t.remove(),t=o}}setConnected(t){var s;this._$AM===void 0&&(this._$Cv=t,(s=this._$AP)==null||s.call(this,t))}}class Co{get tagName(){return this.element.tagName}get _$AU(){return this._$AM._$AU}constructor(t,s,i,o,r){this.type=1,this._$AH=G,this._$AN=void 0,this.element=t,this.name=s,this._$AM=o,this.options=r,i.length>2||i[0]!==""||i[1]!==""?(this._$AH=Array(i.length-1).fill(new String),this.strings=i):this._$AH=G}_$AI(t,s=this,i,o){const r=this.strings;let a=!1;if(r===void 0)t=zs(this,t,s,0),a=!vi(t)||t!==this._$AH&&t!==Nt,a&&(this._$AH=t);else{const l=t;let c,h;for(t=r[0],c=0;c<r.length-1;c++)h=zs(this,l[i+c],s,c),h===Nt&&(h=this._$AH[c]),a||(a=!vi(h)||h!==this._$AH[c]),h===G?t=G:t!==G&&(t+=(h??"")+r[c+1]),this._$AH[c]=h}a&&!o&&this.j(t)}j(t){t===G?this.element.removeAttribute(this.name):this.element.setAttribute(this.name,t??"")}}let uc=class extends Co{constructor(){super(...arguments),this.type=3}j(t){this.element[this.name]=t===G?void 0:t}};class pc extends Co{constructor(){super(...arguments),this.type=4}j(t){this.element.toggleAttribute(this.name,!!t&&t!==G)}}class fc extends Co{constructor(t,s,i,o,r){super(t,s,i,o,r),this.type=5}_$AI(t,s=this){if((t=zs(this,t,s,0)??G)===Nt)return;const i=this._$AH,o=t===G&&i!==G||t.capture!==i.capture||t.once!==i.once||t.passive!==i.passive,r=t!==G&&(i===G||o);o&&this.element.removeEventListener(this.name,this,i),r&&this.element.addEventListener(this.name,this,t),this._$AH=t}handleEvent(t){var s;typeof this._$AH=="function"?this._$AH.call(((s=this.options)==null?void 0:s.host)??this.element,t):this._$AH.handleEvent(t)}}class mc{constructor(t,s,i){this.element=t,this.type=6,this._$AN=void 0,this._$AM=s,this.options=i}get _$AU(){return this._$AM._$AU}_$AI(t){zs(this,t)}}const Ko=ni.litHtmlPolyfillSupport;Ko==null||Ko(yi,$i),(ni.litHtmlVersions??(ni.litHtmlVersions=[])).push("3.2.1");const gc=(e,t,s)=>{const i=(s==null?void 0:s.renderBefore)??t;let o=i._$litPart$;if(o===void 0){const r=(s==null?void 0:s.renderBefore)??null;i._$litPart$=o=new $i(t.insertBefore(bi(),r),r,void 0,s??{})}return o._$AI(e),o};/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */let tt=class extends Cs{constructor(){super(...arguments),this.renderOptions={host:this},this._$Do=void 0}createRenderRoot(){var s;const t=super.createRenderRoot();return(s=this.renderOptions).renderBefore??(s.renderBefore=t.firstChild),t}update(t){const s=this.render();this.hasUpdated||(this.renderOptions.isConnected=this.isConnected),super.update(t),this._$Do=gc(s,this.renderRoot,this.renderOptions)}connectedCallback(){var t;super.connectedCallback(),(t=this._$Do)==null||t.setConnected(!0)}disconnectedCallback(){var t;super.disconnectedCallback(),(t=this._$Do)==null||t.setConnected(!1)}render(){return Nt}};var On;tt._$litElement$=!0,tt.finalized=!0,(On=globalThis.litElementHydrateSupport)==null||On.call(globalThis,{LitElement:tt});const Yo=globalThis.litElementPolyfillSupport;Yo==null||Yo({LitElement:tt});(globalThis.litElementVersions??(globalThis.litElementVersions=[])).push("4.1.1");/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const lt=e=>(t,s)=>{s!==void 0?s.addInitializer(()=>{customElements.define(e,t)}):customElements.define(e,t)};/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const bc={attribute:!0,type:String,converter:Ts,reflect:!1,hasChanged:Or},vc=(e=bc,t,s)=>{const{kind:i,metadata:o}=s;let r=globalThis.litPropertyMetadata.get(o);if(r===void 0&&globalThis.litPropertyMetadata.set(o,r=new Map),r.set(s.name,e),i==="accessor"){const{name:a}=s;return{set(l){const c=t.get.call(this);t.set.call(this,l),this.requestUpdate(a,c,e)},init(l){return l!==void 0&&this.P(a,void 0,e),l}}}if(i==="setter"){const{name:a}=s;return function(l){const c=this[a];t.call(this,l),this.requestUpdate(a,c,e)}}throw Error("Unsupported decorator location: "+i)};function d(e){return(t,s)=>typeof s=="object"?vc(e,t,s):((i,o,r)=>{const a=o.hasOwnProperty(r);return o.constructor.createProperty(r,a?{...i,wrapped:!0}:i),a?Object.getOwnPropertyDescriptor(o,r):void 0})(e,t,s)}/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function E(e){return d({...e,state:!0,attribute:!1})}/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function Ci(e){return(t,s)=>{const i=typeof t=="function"?t:t[s];Object.assign(i,e)}}/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Dr=(e,t,s)=>(s.configurable=!0,s.enumerable=!0,Reflect.decorate&&typeof t!="object"&&Object.defineProperty(e,t,s),s);/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function T(e,t){return(s,i,o)=>{const r=a=>{var l;return((l=a.renderRoot)==null?void 0:l.querySelector(e))??null};return Dr(s,i,{get(){return r(this)}})}}/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */let yc;function Mn(e){return(t,s)=>Dr(t,s,{get(){return(this.renderRoot??yc??(yc=document.createDocumentFragment())).querySelectorAll(e)}})}/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function wc(e){return(t,s)=>Dr(t,s,{async get(){var i;return await this.updateComplete,((i=this.renderRoot)==null?void 0:i.querySelector(e))??null}})}/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Ba=new WeakMap,Ua=e=>{if((s=>s.pattern!==void 0)(e))return e.pattern;let t=Ba.get(e);return t===void 0&&Ba.set(e,t=new URLPattern({pathname:e.path})),t};let xc=class{constructor(t,s,i){this.routes=[],this.o=[],this.t={},this.i=o=>{if(o.routes===this)return;const r=o.routes;this.o.push(r),r.h=this,o.stopImmediatePropagation(),o.onDisconnect=()=>{var l;(l=this.o)==null||l.splice(this.o.indexOf(r)>>>0,1)};const a=Ha(this.t);a!==void 0&&r.goto(a)},(this.l=t).addController(this),this.routes=[...s],this.fallback=i==null?void 0:i.fallback}link(t){var s;if(t!=null&&t.startsWith("/"))return t;if(t!=null&&t.startsWith("."))throw Error("Not implemented");return t??(t=this.u),(((s=this.h)==null?void 0:s.link())??"")+t}async goto(t){let s;if(this.routes.length===0&&this.fallback===void 0)s=t,this.u="",this.t={0:s};else{const i=this.p(t);if(i===void 0)throw Error("No route found for "+t);const o=Ua(i).exec({pathname:t}),r=(o==null?void 0:o.pathname.groups)??{};if(s=Ha(r),typeof i.enter=="function"&&await i.enter(r)===!1)return;this.v=i,this.t=r,this.u=s===void 0?t:t.substring(0,t.length-s.length)}if(s!==void 0)for(const i of this.o)i.goto(s);this.l.requestUpdate()}outlet(){var t,s;return(s=(t=this.v)==null?void 0:t.render)==null?void 0:s.call(t,this.t)}get params(){return this.t}p(t){const s=this.routes.find(i=>Ua(i).test({pathname:t}));return s||this.fallback===void 0?s:this.fallback?{...this.fallback,path:"/*"}:void 0}hostConnected(){this.l.addEventListener(dr.eventName,this.i);const t=new dr(this);this.l.dispatchEvent(t),this._=t.onDisconnect}hostDisconnected(){var t;(t=this._)==null||t.call(this),this.h=void 0}};const Ha=e=>{let t;for(const s of Object.keys(e))/\d+/.test(s)&&(t===void 0||s>t)&&(t=s);return t&&e[t]};let dr=class Nn extends Event{constructor(t){super(Nn.eventName,{bubbles:!0,composed:!0,cancelable:!1}),this.routes=t}};dr.eventName="lit-routes-connected";/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const _c=location.origin||location.protocol+"//"+location.host;let kc=class extends xc{constructor(){super(...arguments),this.m=t=>{const s=t.button!==0||t.metaKey||t.ctrlKey||t.shiftKey;if(t.defaultPrevented||s)return;const i=t.composedPath().find(a=>a.tagName==="A");if(i===void 0||i.target!==""||i.hasAttribute("download")||i.getAttribute("rel")==="external")return;const o=i.href;if(o===""||o.startsWith("mailto:"))return;const r=window.location;i.origin===_c&&(t.preventDefault(),o!==r.href&&(window.history.pushState({},"",o),this.goto(i.pathname)))},this.R=t=>{this.goto(window.location.pathname)}}hostConnected(){super.hostConnected(),window.addEventListener("click",this.m),window.addEventListener("popstate",this.R),this.goto(window.location.pathname)}hostDisconnected(){super.hostDisconnected(),window.removeEventListener("click",this.m),window.removeEventListener("popstate",this.R)}};var Fn=Object.defineProperty,$c=Object.defineProperties,Cc=Object.getOwnPropertyDescriptor,Sc=Object.getOwnPropertyDescriptors,Va=Object.getOwnPropertySymbols,Ac=Object.prototype.hasOwnProperty,Tc=Object.prototype.propertyIsEnumerable,Go=(e,t)=>(t=Symbol[e])?t:Symbol.for("Symbol."+e),ja=(e,t,s)=>t in e?Fn(e,t,{enumerable:!0,configurable:!0,writable:!0,value:s}):e[t]=s,Ne=(e,t)=>{for(var s in t||(t={}))Ac.call(t,s)&&ja(e,s,t[s]);if(Va)for(var s of Va(t))Tc.call(t,s)&&ja(e,s,t[s]);return e},Si=(e,t)=>$c(e,Sc(t)),n=(e,t,s,i)=>{for(var o=i>1?void 0:i?Cc(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&Fn(t,s,o),o},Bn=(e,t,s)=>{if(!t.has(e))throw TypeError("Cannot "+s)},zc=(e,t,s)=>(Bn(e,t,"read from private field"),t.get(e)),Ec=(e,t,s)=>{if(t.has(e))throw TypeError("Cannot add the same private member more than once");t instanceof WeakSet?t.add(e):t.set(e,s)},Oc=(e,t,s,i)=>(Bn(e,t,"write to private field"),t.set(e,s),s),Pc=function(e,t){this[0]=e,this[1]=t},Dc=e=>{var t=e[Go("asyncIterator")],s=!1,i,o={};return t==null?(t=e[Go("iterator")](),i=r=>o[r]=a=>t[r](a)):(t=t.call(e),i=r=>o[r]=a=>{if(s){if(s=!1,r==="throw")throw a;return a}return s=!0,{done:!1,value:new Pc(new Promise(l=>{var c=t[r](a);if(!(c instanceof Object))throw TypeError("Object expected");l(c)}),1)}}),o[Go("iterator")]=()=>o,i("next"),"throw"in t?i("throw"):o.throw=r=>{throw r},"return"in t&&i("return"),o},Ks=new WeakMap,Ys=new WeakMap,Gs=new WeakMap,Xo=new WeakSet,ji=new WeakMap,Fe=class{constructor(e,t){this.handleFormData=s=>{const i=this.options.disabled(this.host),o=this.options.name(this.host),r=this.options.value(this.host),a=this.host.tagName.toLowerCase()==="sl-button";this.host.isConnected&&!i&&!a&&typeof o=="string"&&o.length>0&&typeof r<"u"&&(Array.isArray(r)?r.forEach(l=>{s.formData.append(o,l.toString())}):s.formData.append(o,r.toString()))},this.handleFormSubmit=s=>{var i;const o=this.options.disabled(this.host),r=this.options.reportValidity;this.form&&!this.form.noValidate&&((i=Ks.get(this.form))==null||i.forEach(a=>{this.setUserInteracted(a,!0)})),this.form&&!this.form.noValidate&&!o&&!r(this.host)&&(s.preventDefault(),s.stopImmediatePropagation())},this.handleFormReset=()=>{this.options.setValue(this.host,this.options.defaultValue(this.host)),this.setUserInteracted(this.host,!1),ji.set(this.host,[])},this.handleInteraction=s=>{const i=ji.get(this.host);i.includes(s.type)||i.push(s.type),i.length===this.options.assumeInteractionOn.length&&this.setUserInteracted(this.host,!0)},this.checkFormValidity=()=>{if(this.form&&!this.form.noValidate){const s=this.form.querySelectorAll("*");for(const i of s)if(typeof i.checkValidity=="function"&&!i.checkValidity())return!1}return!0},this.reportFormValidity=()=>{if(this.form&&!this.form.noValidate){const s=this.form.querySelectorAll("*");for(const i of s)if(typeof i.reportValidity=="function"&&!i.reportValidity())return!1}return!0},(this.host=e).addController(this),this.options=Ne({form:s=>{const i=s.form;if(i){const r=s.getRootNode().querySelector(`#${i}`);if(r)return r}return s.closest("form")},name:s=>s.name,value:s=>s.value,defaultValue:s=>s.defaultValue,disabled:s=>{var i;return(i=s.disabled)!=null?i:!1},reportValidity:s=>typeof s.reportValidity=="function"?s.reportValidity():!0,checkValidity:s=>typeof s.checkValidity=="function"?s.checkValidity():!0,setValue:(s,i)=>s.value=i,assumeInteractionOn:["sl-input"]},t)}hostConnected(){const e=this.options.form(this.host);e&&this.attachForm(e),ji.set(this.host,[]),this.options.assumeInteractionOn.forEach(t=>{this.host.addEventListener(t,this.handleInteraction)})}hostDisconnected(){this.detachForm(),ji.delete(this.host),this.options.assumeInteractionOn.forEach(e=>{this.host.removeEventListener(e,this.handleInteraction)})}hostUpdated(){const e=this.options.form(this.host);e||this.detachForm(),e&&this.form!==e&&(this.detachForm(),this.attachForm(e)),this.host.hasUpdated&&this.setValidity(this.host.validity.valid)}attachForm(e){e?(this.form=e,Ks.has(this.form)?Ks.get(this.form).add(this.host):Ks.set(this.form,new Set([this.host])),this.form.addEventListener("formdata",this.handleFormData),this.form.addEventListener("submit",this.handleFormSubmit),this.form.addEventListener("reset",this.handleFormReset),Ys.has(this.form)||(Ys.set(this.form,this.form.reportValidity),this.form.reportValidity=()=>this.reportFormValidity()),Gs.has(this.form)||(Gs.set(this.form,this.form.checkValidity),this.form.checkValidity=()=>this.checkFormValidity())):this.form=void 0}detachForm(){if(!this.form)return;const e=Ks.get(this.form);e&&(e.delete(this.host),e.size<=0&&(this.form.removeEventListener("formdata",this.handleFormData),this.form.removeEventListener("submit",this.handleFormSubmit),this.form.removeEventListener("reset",this.handleFormReset),Ys.has(this.form)&&(this.form.reportValidity=Ys.get(this.form),Ys.delete(this.form)),Gs.has(this.form)&&(this.form.checkValidity=Gs.get(this.form),Gs.delete(this.form)),this.form=void 0))}setUserInteracted(e,t){t?Xo.add(e):Xo.delete(e),e.requestUpdate()}doAction(e,t){if(this.form){const s=document.createElement("button");s.type=e,s.style.position="absolute",s.style.width="0",s.style.height="0",s.style.clipPath="inset(50%)",s.style.overflow="hidden",s.style.whiteSpace="nowrap",t&&(s.name=t.name,s.value=t.value,["formaction","formenctype","formmethod","formnovalidate","formtarget"].forEach(i=>{t.hasAttribute(i)&&s.setAttribute(i,t.getAttribute(i))})),this.form.append(s),s.click(),s.remove()}}getForm(){var e;return(e=this.form)!=null?e:null}reset(e){this.doAction("reset",e)}submit(e){this.doAction("submit",e)}setValidity(e){const t=this.host,s=!!Xo.has(t),i=!!t.required;t.toggleAttribute("data-required",i),t.toggleAttribute("data-optional",!i),t.toggleAttribute("data-invalid",!e),t.toggleAttribute("data-valid",e),t.toggleAttribute("data-user-invalid",!e&&s),t.toggleAttribute("data-user-valid",e&&s)}updateValidity(){const e=this.host;this.setValidity(e.validity.valid)}emitInvalidEvent(e){const t=new CustomEvent("sl-invalid",{bubbles:!1,composed:!1,cancelable:!0,detail:{}});e||t.preventDefault(),this.host.dispatchEvent(t)||e==null||e.preventDefault()}},So=Object.freeze({badInput:!1,customError:!1,patternMismatch:!1,rangeOverflow:!1,rangeUnderflow:!1,stepMismatch:!1,tooLong:!1,tooShort:!1,typeMismatch:!1,valid:!0,valueMissing:!1}),Ic=Object.freeze(Si(Ne({},So),{valid:!1,valueMissing:!0})),Rc=Object.freeze(Si(Ne({},So),{valid:!1,customError:!0})),Lc=A`
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
`,M=A`
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
`,eo,P=class extends tt{constructor(){super(),Ec(this,eo,!1),this.initialReflectedProperties=new Map,Object.entries(this.constructor.dependencies).forEach(([e,t])=>{this.constructor.define(e,t)})}emit(e,t){const s=new CustomEvent(e,Ne({bubbles:!0,cancelable:!1,composed:!0,detail:{}},t));return this.dispatchEvent(s),s}static define(e,t=this,s={}){const i=customElements.get(e);if(!i){try{customElements.define(e,t,s)}catch{customElements.define(e,class extends t{},s)}return}let o=" (unknown version)",r=o;"version"in t&&t.version&&(o=" v"+t.version),"version"in i&&i.version&&(r=" v"+i.version),!(o&&r&&o===r)&&console.warn(`Attempted to register <${e}>${o}, but <${e}>${r} has already been registered.`)}attributeChangedCallback(e,t,s){zc(this,eo)||(this.constructor.elementProperties.forEach((i,o)=>{i.reflect&&this[o]!=null&&this.initialReflectedProperties.set(o,this[o])}),Oc(this,eo,!0)),super.attributeChangedCallback(e,t,s)}willUpdate(e){super.willUpdate(e),this.initialReflectedProperties.forEach((t,s)=>{e.has(s)&&this[s]==null&&(this[s]=t)})}};eo=new WeakMap;P.version="2.18.0";P.dependencies={};n([d()],P.prototype,"dir",2);n([d()],P.prototype,"lang",2);var Ir=class extends P{render(){return v` <slot></slot> `}};Ir.styles=[M,Lc];Ir.define("sl-visually-hidden");var Mc=A`
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
`,Nc=A`
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
`;const hr=new Set,Ss=new Map;let ts,Rr="ltr",Lr="en";const Un=typeof MutationObserver<"u"&&typeof document<"u"&&typeof document.documentElement<"u";if(Un){const e=new MutationObserver(Vn);Rr=document.documentElement.dir||"ltr",Lr=document.documentElement.lang||navigator.language,e.observe(document.documentElement,{attributes:!0,attributeFilter:["dir","lang"]})}function Hn(...e){e.map(t=>{const s=t.$code.toLowerCase();Ss.has(s)?Ss.set(s,Object.assign(Object.assign({},Ss.get(s)),t)):Ss.set(s,t),ts||(ts=t)}),Vn()}function Vn(){Un&&(Rr=document.documentElement.dir||"ltr",Lr=document.documentElement.lang||navigator.language),[...hr.keys()].map(e=>{typeof e.requestUpdate=="function"&&e.requestUpdate()})}let Fc=class{constructor(t){this.host=t,this.host.addController(this)}hostConnected(){hr.add(this.host)}hostDisconnected(){hr.delete(this.host)}dir(){return`${this.host.dir||Rr}`.toLowerCase()}lang(){return`${this.host.lang||Lr}`.toLowerCase()}getTranslationData(t){var s,i;const o=new Intl.Locale(t.replace(/_/g,"-")),r=o==null?void 0:o.language.toLowerCase(),a=(i=(s=o==null?void 0:o.region)===null||s===void 0?void 0:s.toLowerCase())!==null&&i!==void 0?i:"",l=Ss.get(`${r}-${a}`),c=Ss.get(r);return{locale:o,language:r,region:a,primary:l,secondary:c}}exists(t,s){var i;const{primary:o,secondary:r}=this.getTranslationData((i=s.lang)!==null&&i!==void 0?i:this.lang());return s=Object.assign({includeFallback:!1},s),!!(o&&o[t]||r&&r[t]||s.includeFallback&&ts&&ts[t])}term(t,...s){const{primary:i,secondary:o}=this.getTranslationData(this.lang());let r;if(i&&i[t])r=i[t];else if(o&&o[t])r=o[t];else if(ts&&ts[t])r=ts[t];else return console.error(`No translation found for: ${String(t)}`),String(t);return typeof r=="function"?r(...s):r}date(t,s){return t=new Date(t),new Intl.DateTimeFormat(this.lang(),s).format(t)}number(t,s){return t=Number(t),isNaN(t)?"":new Intl.NumberFormat(this.lang(),s).format(t)}relativeTime(t,s,i){return new Intl.RelativeTimeFormat(this.lang(),i).format(t,s)}};var jn={$code:"en",$name:"English",$dir:"ltr",carousel:"Carousel",clearEntry:"Clear entry",close:"Close",copied:"Copied",copy:"Copy",currentValue:"Current value",error:"Error",goToSlide:(e,t)=>`Go to slide ${e} of ${t}`,hidePassword:"Hide password",loading:"Loading",nextSlide:"Next slide",numOptionsSelected:e=>e===0?"No options selected":e===1?"1 option selected":`${e} options selected`,previousSlide:"Previous slide",progress:"Progress",remove:"Remove",resize:"Resize",scrollToEnd:"Scroll to end",scrollToStart:"Scroll to start",selectAColorFromTheScreen:"Select a color from the screen",showPassword:"Show password",slideNum:e=>`Slide ${e}`,toggleColorFormat:"Toggle color format"};Hn(jn);var Bc=jn,K=class extends Fc{};Hn(Bc);const je=Math.min,Gt=Math.max,ro=Math.round,Wi=Math.floor,Se=e=>({x:e,y:e}),Uc={left:"right",right:"left",bottom:"top",top:"bottom"},Hc={start:"end",end:"start"};function ur(e,t,s){return Gt(e,je(t,s))}function Ds(e,t){return typeof e=="function"?e(t):e}function We(e){return e.split("-")[0]}function Is(e){return e.split("-")[1]}function Wn(e){return e==="x"?"y":"x"}function Mr(e){return e==="y"?"height":"width"}function os(e){return["top","bottom"].includes(We(e))?"y":"x"}function Nr(e){return Wn(os(e))}function Vc(e,t,s){s===void 0&&(s=!1);const i=Is(e),o=Nr(e),r=Mr(o);let a=o==="x"?i===(s?"end":"start")?"right":"left":i==="start"?"bottom":"top";return t.reference[r]>t.floating[r]&&(a=ao(a)),[a,ao(a)]}function jc(e){const t=ao(e);return[pr(e),t,pr(t)]}function pr(e){return e.replace(/start|end/g,t=>Hc[t])}function Wc(e,t,s){const i=["left","right"],o=["right","left"],r=["top","bottom"],a=["bottom","top"];switch(e){case"top":case"bottom":return s?t?o:i:t?i:o;case"left":case"right":return t?r:a;default:return[]}}function qc(e,t,s,i){const o=Is(e);let r=Wc(We(e),s==="start",i);return o&&(r=r.map(a=>a+"-"+o),t&&(r=r.concat(r.map(pr)))),r}function ao(e){return e.replace(/left|right|bottom|top/g,t=>Uc[t])}function Kc(e){return{top:0,right:0,bottom:0,left:0,...e}}function qn(e){return typeof e!="number"?Kc(e):{top:e,right:e,bottom:e,left:e}}function no(e){const{x:t,y:s,width:i,height:o}=e;return{width:i,height:o,top:s,left:t,right:t+i,bottom:s+o,x:t,y:s}}function Wa(e,t,s){let{reference:i,floating:o}=e;const r=os(t),a=Nr(t),l=Mr(a),c=We(t),h=r==="y",u=i.x+i.width/2-o.width/2,p=i.y+i.height/2-o.height/2,f=i[l]/2-o[l]/2;let m;switch(c){case"top":m={x:u,y:i.y-o.height};break;case"bottom":m={x:u,y:i.y+i.height};break;case"right":m={x:i.x+i.width,y:p};break;case"left":m={x:i.x-o.width,y:p};break;default:m={x:i.x,y:i.y}}switch(Is(t)){case"start":m[a]-=f*(s&&h?-1:1);break;case"end":m[a]+=f*(s&&h?-1:1);break}return m}const Yc=async(e,t,s)=>{const{placement:i="bottom",strategy:o="absolute",middleware:r=[],platform:a}=s,l=r.filter(Boolean),c=await(a.isRTL==null?void 0:a.isRTL(t));let h=await a.getElementRects({reference:e,floating:t,strategy:o}),{x:u,y:p}=Wa(h,i,c),f=i,m={},b=0;for(let y=0;y<l.length;y++){const{name:k,fn:z}=l[y],{x:_,y:S,data:w,reset:x}=await z({x:u,y:p,initialPlacement:i,placement:f,strategy:o,middlewareData:m,rects:h,platform:a,elements:{reference:e,floating:t}});u=_??u,p=S??p,m={...m,[k]:{...m[k],...w}},x&&b<=50&&(b++,typeof x=="object"&&(x.placement&&(f=x.placement),x.rects&&(h=x.rects===!0?await a.getElementRects({reference:e,floating:t,strategy:o}):x.rects),{x:u,y:p}=Wa(h,f,c)),y=-1)}return{x:u,y:p,placement:f,strategy:o,middlewareData:m}};async function Fr(e,t){var s;t===void 0&&(t={});const{x:i,y:o,platform:r,rects:a,elements:l,strategy:c}=e,{boundary:h="clippingAncestors",rootBoundary:u="viewport",elementContext:p="floating",altBoundary:f=!1,padding:m=0}=Ds(t,e),b=qn(m),k=l[f?p==="floating"?"reference":"floating":p],z=no(await r.getClippingRect({element:(s=await(r.isElement==null?void 0:r.isElement(k)))==null||s?k:k.contextElement||await(r.getDocumentElement==null?void 0:r.getDocumentElement(l.floating)),boundary:h,rootBoundary:u,strategy:c})),_=p==="floating"?{x:i,y:o,width:a.floating.width,height:a.floating.height}:a.reference,S=await(r.getOffsetParent==null?void 0:r.getOffsetParent(l.floating)),w=await(r.isElement==null?void 0:r.isElement(S))?await(r.getScale==null?void 0:r.getScale(S))||{x:1,y:1}:{x:1,y:1},x=no(r.convertOffsetParentRelativeRectToViewportRelativeRect?await r.convertOffsetParentRelativeRectToViewportRelativeRect({elements:l,rect:_,offsetParent:S,strategy:c}):_);return{top:(z.top-x.top+b.top)/w.y,bottom:(x.bottom-z.bottom+b.bottom)/w.y,left:(z.left-x.left+b.left)/w.x,right:(x.right-z.right+b.right)/w.x}}const Gc=e=>({name:"arrow",options:e,async fn(t){const{x:s,y:i,placement:o,rects:r,platform:a,elements:l,middlewareData:c}=t,{element:h,padding:u=0}=Ds(e,t)||{};if(h==null)return{};const p=qn(u),f={x:s,y:i},m=Nr(o),b=Mr(m),y=await a.getDimensions(h),k=m==="y",z=k?"top":"left",_=k?"bottom":"right",S=k?"clientHeight":"clientWidth",w=r.reference[b]+r.reference[m]-f[m]-r.floating[b],x=f[m]-r.reference[m],D=await(a.getOffsetParent==null?void 0:a.getOffsetParent(h));let B=D?D[S]:0;(!B||!await(a.isElement==null?void 0:a.isElement(D)))&&(B=l.floating[S]||r.floating[b]);const U=w/2-x/2,F=B/2-y[b]/2-1,R=je(p[z],F),q=je(p[_],F),ot=R,mt=B-y[b]-q,rt=B/2-y[b]/2+U,qt=ur(ot,rt,mt),ie=!c.arrow&&Is(o)!=null&&rt!==qt&&r.reference[b]/2-(rt<ot?R:q)-y[b]/2<0,oe=ie?rt<ot?rt-ot:rt-mt:0;return{[m]:f[m]+oe,data:{[m]:qt,centerOffset:rt-qt-oe,...ie&&{alignmentOffset:oe}},reset:ie}}}),Xc=function(e){return e===void 0&&(e={}),{name:"flip",options:e,async fn(t){var s,i;const{placement:o,middlewareData:r,rects:a,initialPlacement:l,platform:c,elements:h}=t,{mainAxis:u=!0,crossAxis:p=!0,fallbackPlacements:f,fallbackStrategy:m="bestFit",fallbackAxisSideDirection:b="none",flipAlignment:y=!0,...k}=Ds(e,t);if((s=r.arrow)!=null&&s.alignmentOffset)return{};const z=We(o),_=os(l),S=We(l)===l,w=await(c.isRTL==null?void 0:c.isRTL(h.floating)),x=f||(S||!y?[ao(l)]:jc(l)),D=b!=="none";!f&&D&&x.push(...qc(l,y,b,w));const B=[l,...x],U=await Fr(t,k),F=[];let R=((i=r.flip)==null?void 0:i.overflows)||[];if(u&&F.push(U[z]),p){const rt=Vc(o,a,w);F.push(U[rt[0]],U[rt[1]])}if(R=[...R,{placement:o,overflows:F}],!F.every(rt=>rt<=0)){var q,ot;const rt=(((q=r.flip)==null?void 0:q.index)||0)+1,qt=B[rt];if(qt)return{data:{index:rt,overflows:R},reset:{placement:qt}};let ie=(ot=R.filter(oe=>oe.overflows[0]<=0).sort((oe,de)=>oe.overflows[1]-de.overflows[1])[0])==null?void 0:ot.placement;if(!ie)switch(m){case"bestFit":{var mt;const oe=(mt=R.filter(de=>{if(D){const we=os(de.placement);return we===_||we==="y"}return!0}).map(de=>[de.placement,de.overflows.filter(we=>we>0).reduce((we,Li)=>we+Li,0)]).sort((de,we)=>de[1]-we[1])[0])==null?void 0:mt[0];oe&&(ie=oe);break}case"initialPlacement":ie=l;break}if(o!==ie)return{reset:{placement:ie}}}return{}}}};async function Zc(e,t){const{placement:s,platform:i,elements:o}=e,r=await(i.isRTL==null?void 0:i.isRTL(o.floating)),a=We(s),l=Is(s),c=os(s)==="y",h=["left","top"].includes(a)?-1:1,u=r&&c?-1:1,p=Ds(t,e);let{mainAxis:f,crossAxis:m,alignmentAxis:b}=typeof p=="number"?{mainAxis:p,crossAxis:0,alignmentAxis:null}:{mainAxis:p.mainAxis||0,crossAxis:p.crossAxis||0,alignmentAxis:p.alignmentAxis};return l&&typeof b=="number"&&(m=l==="end"?b*-1:b),c?{x:m*u,y:f*h}:{x:f*h,y:m*u}}const Qc=function(e){return e===void 0&&(e=0),{name:"offset",options:e,async fn(t){var s,i;const{x:o,y:r,placement:a,middlewareData:l}=t,c=await Zc(t,e);return a===((s=l.offset)==null?void 0:s.placement)&&(i=l.arrow)!=null&&i.alignmentOffset?{}:{x:o+c.x,y:r+c.y,data:{...c,placement:a}}}}},Jc=function(e){return e===void 0&&(e={}),{name:"shift",options:e,async fn(t){const{x:s,y:i,placement:o}=t,{mainAxis:r=!0,crossAxis:a=!1,limiter:l={fn:k=>{let{x:z,y:_}=k;return{x:z,y:_}}},...c}=Ds(e,t),h={x:s,y:i},u=await Fr(t,c),p=os(We(o)),f=Wn(p);let m=h[f],b=h[p];if(r){const k=f==="y"?"top":"left",z=f==="y"?"bottom":"right",_=m+u[k],S=m-u[z];m=ur(_,m,S)}if(a){const k=p==="y"?"top":"left",z=p==="y"?"bottom":"right",_=b+u[k],S=b-u[z];b=ur(_,b,S)}const y=l.fn({...t,[f]:m,[p]:b});return{...y,data:{x:y.x-s,y:y.y-i,enabled:{[f]:r,[p]:a}}}}}},td=function(e){return e===void 0&&(e={}),{name:"size",options:e,async fn(t){var s,i;const{placement:o,rects:r,platform:a,elements:l}=t,{apply:c=()=>{},...h}=Ds(e,t),u=await Fr(t,h),p=We(o),f=Is(o),m=os(o)==="y",{width:b,height:y}=r.floating;let k,z;p==="top"||p==="bottom"?(k=p,z=f===(await(a.isRTL==null?void 0:a.isRTL(l.floating))?"start":"end")?"left":"right"):(z=p,k=f==="end"?"top":"bottom");const _=y-u.top-u.bottom,S=b-u.left-u.right,w=je(y-u[k],_),x=je(b-u[z],S),D=!t.middlewareData.shift;let B=w,U=x;if((s=t.middlewareData.shift)!=null&&s.enabled.x&&(U=S),(i=t.middlewareData.shift)!=null&&i.enabled.y&&(B=_),D&&!f){const R=Gt(u.left,0),q=Gt(u.right,0),ot=Gt(u.top,0),mt=Gt(u.bottom,0);m?U=b-2*(R!==0||q!==0?R+q:Gt(u.left,u.right)):B=y-2*(ot!==0||mt!==0?ot+mt:Gt(u.top,u.bottom))}await c({...t,availableWidth:U,availableHeight:B});const F=await a.getDimensions(l.floating);return b!==F.width||y!==F.height?{reset:{rects:!0}}:{}}}};function Ao(){return typeof window<"u"}function Rs(e){return Kn(e)?(e.nodeName||"").toLowerCase():"#document"}function Xt(e){var t;return(e==null||(t=e.ownerDocument)==null?void 0:t.defaultView)||window}function ze(e){var t;return(t=(Kn(e)?e.ownerDocument:e.document)||window.document)==null?void 0:t.documentElement}function Kn(e){return Ao()?e instanceof Node||e instanceof Xt(e).Node:!1}function ue(e){return Ao()?e instanceof Element||e instanceof Xt(e).Element:!1}function Ae(e){return Ao()?e instanceof HTMLElement||e instanceof Xt(e).HTMLElement:!1}function qa(e){return!Ao()||typeof ShadowRoot>"u"?!1:e instanceof ShadowRoot||e instanceof Xt(e).ShadowRoot}function Ai(e){const{overflow:t,overflowX:s,overflowY:i,display:o}=pe(e);return/auto|scroll|overlay|hidden|clip/.test(t+i+s)&&!["inline","contents"].includes(o)}function ed(e){return["table","td","th"].includes(Rs(e))}function To(e){return[":popover-open",":modal"].some(t=>{try{return e.matches(t)}catch{return!1}})}function Br(e){const t=Ur(),s=ue(e)?pe(e):e;return s.transform!=="none"||s.perspective!=="none"||(s.containerType?s.containerType!=="normal":!1)||!t&&(s.backdropFilter?s.backdropFilter!=="none":!1)||!t&&(s.filter?s.filter!=="none":!1)||["transform","perspective","filter"].some(i=>(s.willChange||"").includes(i))||["paint","layout","strict","content"].some(i=>(s.contain||"").includes(i))}function sd(e){let t=qe(e);for(;Ae(t)&&!Es(t);){if(Br(t))return t;if(To(t))return null;t=qe(t)}return null}function Ur(){return typeof CSS>"u"||!CSS.supports?!1:CSS.supports("-webkit-backdrop-filter","none")}function Es(e){return["html","body","#document"].includes(Rs(e))}function pe(e){return Xt(e).getComputedStyle(e)}function zo(e){return ue(e)?{scrollLeft:e.scrollLeft,scrollTop:e.scrollTop}:{scrollLeft:e.scrollX,scrollTop:e.scrollY}}function qe(e){if(Rs(e)==="html")return e;const t=e.assignedSlot||e.parentNode||qa(e)&&e.host||ze(e);return qa(t)?t.host:t}function Yn(e){const t=qe(e);return Es(t)?e.ownerDocument?e.ownerDocument.body:e.body:Ae(t)&&Ai(t)?t:Yn(t)}function wi(e,t,s){var i;t===void 0&&(t=[]),s===void 0&&(s=!0);const o=Yn(e),r=o===((i=e.ownerDocument)==null?void 0:i.body),a=Xt(o);if(r){const l=fr(a);return t.concat(a,a.visualViewport||[],Ai(o)?o:[],l&&s?wi(l):[])}return t.concat(o,wi(o,[],s))}function fr(e){return e.parent&&Object.getPrototypeOf(e.parent)?e.frameElement:null}function Gn(e){const t=pe(e);let s=parseFloat(t.width)||0,i=parseFloat(t.height)||0;const o=Ae(e),r=o?e.offsetWidth:s,a=o?e.offsetHeight:i,l=ro(s)!==r||ro(i)!==a;return l&&(s=r,i=a),{width:s,height:i,$:l}}function Hr(e){return ue(e)?e:e.contextElement}function As(e){const t=Hr(e);if(!Ae(t))return Se(1);const s=t.getBoundingClientRect(),{width:i,height:o,$:r}=Gn(t);let a=(r?ro(s.width):s.width)/i,l=(r?ro(s.height):s.height)/o;return(!a||!Number.isFinite(a))&&(a=1),(!l||!Number.isFinite(l))&&(l=1),{x:a,y:l}}const id=Se(0);function Xn(e){const t=Xt(e);return!Ur()||!t.visualViewport?id:{x:t.visualViewport.offsetLeft,y:t.visualViewport.offsetTop}}function od(e,t,s){return t===void 0&&(t=!1),!s||t&&s!==Xt(e)?!1:t}function rs(e,t,s,i){t===void 0&&(t=!1),s===void 0&&(s=!1);const o=e.getBoundingClientRect(),r=Hr(e);let a=Se(1);t&&(i?ue(i)&&(a=As(i)):a=As(e));const l=od(r,s,i)?Xn(r):Se(0);let c=(o.left+l.x)/a.x,h=(o.top+l.y)/a.y,u=o.width/a.x,p=o.height/a.y;if(r){const f=Xt(r),m=i&&ue(i)?Xt(i):i;let b=f,y=fr(b);for(;y&&i&&m!==b;){const k=As(y),z=y.getBoundingClientRect(),_=pe(y),S=z.left+(y.clientLeft+parseFloat(_.paddingLeft))*k.x,w=z.top+(y.clientTop+parseFloat(_.paddingTop))*k.y;c*=k.x,h*=k.y,u*=k.x,p*=k.y,c+=S,h+=w,b=Xt(y),y=fr(b)}}return no({width:u,height:p,x:c,y:h})}function Vr(e,t){const s=zo(e).scrollLeft;return t?t.left+s:rs(ze(e)).left+s}function Zn(e,t,s){s===void 0&&(s=!1);const i=e.getBoundingClientRect(),o=i.left+t.scrollLeft-(s?0:Vr(e,i)),r=i.top+t.scrollTop;return{x:o,y:r}}function rd(e){let{elements:t,rect:s,offsetParent:i,strategy:o}=e;const r=o==="fixed",a=ze(i),l=t?To(t.floating):!1;if(i===a||l&&r)return s;let c={scrollLeft:0,scrollTop:0},h=Se(1);const u=Se(0),p=Ae(i);if((p||!p&&!r)&&((Rs(i)!=="body"||Ai(a))&&(c=zo(i)),Ae(i))){const m=rs(i);h=As(i),u.x=m.x+i.clientLeft,u.y=m.y+i.clientTop}const f=a&&!p&&!r?Zn(a,c,!0):Se(0);return{width:s.width*h.x,height:s.height*h.y,x:s.x*h.x-c.scrollLeft*h.x+u.x+f.x,y:s.y*h.y-c.scrollTop*h.y+u.y+f.y}}function ad(e){return Array.from(e.getClientRects())}function nd(e){const t=ze(e),s=zo(e),i=e.ownerDocument.body,o=Gt(t.scrollWidth,t.clientWidth,i.scrollWidth,i.clientWidth),r=Gt(t.scrollHeight,t.clientHeight,i.scrollHeight,i.clientHeight);let a=-s.scrollLeft+Vr(e);const l=-s.scrollTop;return pe(i).direction==="rtl"&&(a+=Gt(t.clientWidth,i.clientWidth)-o),{width:o,height:r,x:a,y:l}}function ld(e,t){const s=Xt(e),i=ze(e),o=s.visualViewport;let r=i.clientWidth,a=i.clientHeight,l=0,c=0;if(o){r=o.width,a=o.height;const h=Ur();(!h||h&&t==="fixed")&&(l=o.offsetLeft,c=o.offsetTop)}return{width:r,height:a,x:l,y:c}}function cd(e,t){const s=rs(e,!0,t==="fixed"),i=s.top+e.clientTop,o=s.left+e.clientLeft,r=Ae(e)?As(e):Se(1),a=e.clientWidth*r.x,l=e.clientHeight*r.y,c=o*r.x,h=i*r.y;return{width:a,height:l,x:c,y:h}}function Ka(e,t,s){let i;if(t==="viewport")i=ld(e,s);else if(t==="document")i=nd(ze(e));else if(ue(t))i=cd(t,s);else{const o=Xn(e);i={x:t.x-o.x,y:t.y-o.y,width:t.width,height:t.height}}return no(i)}function Qn(e,t){const s=qe(e);return s===t||!ue(s)||Es(s)?!1:pe(s).position==="fixed"||Qn(s,t)}function dd(e,t){const s=t.get(e);if(s)return s;let i=wi(e,[],!1).filter(l=>ue(l)&&Rs(l)!=="body"),o=null;const r=pe(e).position==="fixed";let a=r?qe(e):e;for(;ue(a)&&!Es(a);){const l=pe(a),c=Br(a);!c&&l.position==="fixed"&&(o=null),(r?!c&&!o:!c&&l.position==="static"&&!!o&&["absolute","fixed"].includes(o.position)||Ai(a)&&!c&&Qn(e,a))?i=i.filter(u=>u!==a):o=l,a=qe(a)}return t.set(e,i),i}function hd(e){let{element:t,boundary:s,rootBoundary:i,strategy:o}=e;const a=[...s==="clippingAncestors"?To(t)?[]:dd(t,this._c):[].concat(s),i],l=a[0],c=a.reduce((h,u)=>{const p=Ka(t,u,o);return h.top=Gt(p.top,h.top),h.right=je(p.right,h.right),h.bottom=je(p.bottom,h.bottom),h.left=Gt(p.left,h.left),h},Ka(t,l,o));return{width:c.right-c.left,height:c.bottom-c.top,x:c.left,y:c.top}}function ud(e){const{width:t,height:s}=Gn(e);return{width:t,height:s}}function pd(e,t,s){const i=Ae(t),o=ze(t),r=s==="fixed",a=rs(e,!0,r,t);let l={scrollLeft:0,scrollTop:0};const c=Se(0);if(i||!i&&!r)if((Rs(t)!=="body"||Ai(o))&&(l=zo(t)),i){const f=rs(t,!0,r,t);c.x=f.x+t.clientLeft,c.y=f.y+t.clientTop}else o&&(c.x=Vr(o));const h=o&&!i&&!r?Zn(o,l):Se(0),u=a.left+l.scrollLeft-c.x-h.x,p=a.top+l.scrollTop-c.y-h.y;return{x:u,y:p,width:a.width,height:a.height}}function Zo(e){return pe(e).position==="static"}function Ya(e,t){if(!Ae(e)||pe(e).position==="fixed")return null;if(t)return t(e);let s=e.offsetParent;return ze(e)===s&&(s=s.ownerDocument.body),s}function Jn(e,t){const s=Xt(e);if(To(e))return s;if(!Ae(e)){let o=qe(e);for(;o&&!Es(o);){if(ue(o)&&!Zo(o))return o;o=qe(o)}return s}let i=Ya(e,t);for(;i&&ed(i)&&Zo(i);)i=Ya(i,t);return i&&Es(i)&&Zo(i)&&!Br(i)?s:i||sd(e)||s}const fd=async function(e){const t=this.getOffsetParent||Jn,s=this.getDimensions,i=await s(e.floating);return{reference:pd(e.reference,await t(e.floating),e.strategy),floating:{x:0,y:0,width:i.width,height:i.height}}};function md(e){return pe(e).direction==="rtl"}const so={convertOffsetParentRelativeRectToViewportRelativeRect:rd,getDocumentElement:ze,getClippingRect:hd,getOffsetParent:Jn,getElementRects:fd,getClientRects:ad,getDimensions:ud,getScale:As,isElement:ue,isRTL:md};function gd(e,t){let s=null,i;const o=ze(e);function r(){var l;clearTimeout(i),(l=s)==null||l.disconnect(),s=null}function a(l,c){l===void 0&&(l=!1),c===void 0&&(c=1),r();const{left:h,top:u,width:p,height:f}=e.getBoundingClientRect();if(l||t(),!p||!f)return;const m=Wi(u),b=Wi(o.clientWidth-(h+p)),y=Wi(o.clientHeight-(u+f)),k=Wi(h),_={rootMargin:-m+"px "+-b+"px "+-y+"px "+-k+"px",threshold:Gt(0,je(1,c))||1};let S=!0;function w(x){const D=x[0].intersectionRatio;if(D!==c){if(!S)return a();D?a(!1,D):i=setTimeout(()=>{a(!1,1e-7)},1e3)}S=!1}try{s=new IntersectionObserver(w,{..._,root:o.ownerDocument})}catch{s=new IntersectionObserver(w,_)}s.observe(e)}return a(!0),r}function bd(e,t,s,i){i===void 0&&(i={});const{ancestorScroll:o=!0,ancestorResize:r=!0,elementResize:a=typeof ResizeObserver=="function",layoutShift:l=typeof IntersectionObserver=="function",animationFrame:c=!1}=i,h=Hr(e),u=o||r?[...h?wi(h):[],...wi(t)]:[];u.forEach(z=>{o&&z.addEventListener("scroll",s,{passive:!0}),r&&z.addEventListener("resize",s)});const p=h&&l?gd(h,s):null;let f=-1,m=null;a&&(m=new ResizeObserver(z=>{let[_]=z;_&&_.target===h&&m&&(m.unobserve(t),cancelAnimationFrame(f),f=requestAnimationFrame(()=>{var S;(S=m)==null||S.observe(t)})),s()}),h&&!c&&m.observe(h),m.observe(t));let b,y=c?rs(e):null;c&&k();function k(){const z=rs(e);y&&(z.x!==y.x||z.y!==y.y||z.width!==y.width||z.height!==y.height)&&s(),y=z,b=requestAnimationFrame(k)}return s(),()=>{var z;u.forEach(_=>{o&&_.removeEventListener("scroll",s),r&&_.removeEventListener("resize",s)}),p==null||p(),(z=m)==null||z.disconnect(),m=null,c&&cancelAnimationFrame(b)}}const vd=Qc,yd=Jc,wd=Xc,Ga=td,xd=Gc,_d=(e,t,s)=>{const i=new Map,o={platform:so,...s},r={...o.platform,_c:i};return Yc(e,t,{...o,platform:r})};/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Ce={ATTRIBUTE:1,CHILD:2,PROPERTY:3,BOOLEAN_ATTRIBUTE:4,EVENT:5,ELEMENT:6},Ls=e=>(...t)=>({_$litDirective$:e,values:t});let Ti=class{constructor(t){}get _$AU(){return this._$AM._$AU}_$AT(t,s,i){this._$Ct=t,this._$AM=s,this._$Ci=i}_$AS(t,s){return this.update(t,s)}update(t,s){return this.render(...s)}};/**
 * @license
 * Copyright 2018 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const L=Ls(class extends Ti{constructor(e){var t;if(super(e),e.type!==Ce.ATTRIBUTE||e.name!=="class"||((t=e.strings)==null?void 0:t.length)>2)throw Error("`classMap()` can only be used in the `class` attribute and must be the only part in the attribute.")}render(e){return" "+Object.keys(e).filter(t=>e[t]).join(" ")+" "}update(e,[t]){var i,o;if(this.st===void 0){this.st=new Set,e.strings!==void 0&&(this.nt=new Set(e.strings.join(" ").split(/\s/).filter(r=>r!=="")));for(const r in t)t[r]&&!((i=this.nt)!=null&&i.has(r))&&this.st.add(r);return this.render(t)}const s=e.element.classList;for(const r of this.st)r in t||(s.remove(r),this.st.delete(r));for(const r in t){const a=!!t[r];a===this.st.has(r)||(o=this.nt)!=null&&o.has(r)||(a?(s.add(r),this.st.add(r)):(s.remove(r),this.st.delete(r)))}return Nt}});function kd(e){return $d(e)}function Qo(e){return e.assignedSlot?e.assignedSlot:e.parentNode instanceof ShadowRoot?e.parentNode.host:e.parentNode}function $d(e){for(let t=e;t;t=Qo(t))if(t instanceof Element&&getComputedStyle(t).display==="none")return null;for(let t=Qo(e);t;t=Qo(t)){if(!(t instanceof Element))continue;const s=getComputedStyle(t);if(s.display!=="contents"&&(s.position!=="static"||s.filter!=="none"||t.tagName==="BODY"))return t}return null}function Cd(e){return e!==null&&typeof e=="object"&&"getBoundingClientRect"in e&&("contextElement"in e?e instanceof Element:!0)}var X=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.active=!1,this.placement="top",this.strategy="absolute",this.distance=0,this.skidding=0,this.arrow=!1,this.arrowPlacement="anchor",this.arrowPadding=10,this.flip=!1,this.flipFallbackPlacements="",this.flipFallbackStrategy="best-fit",this.flipPadding=0,this.shift=!1,this.shiftPadding=0,this.autoSizePadding=0,this.hoverBridge=!1,this.updateHoverBridge=()=>{if(this.hoverBridge&&this.anchorEl){const e=this.anchorEl.getBoundingClientRect(),t=this.popup.getBoundingClientRect(),s=this.placement.includes("top")||this.placement.includes("bottom");let i=0,o=0,r=0,a=0,l=0,c=0,h=0,u=0;s?e.top<t.top?(i=e.left,o=e.bottom,r=e.right,a=e.bottom,l=t.left,c=t.top,h=t.right,u=t.top):(i=t.left,o=t.bottom,r=t.right,a=t.bottom,l=e.left,c=e.top,h=e.right,u=e.top):e.left<t.left?(i=e.right,o=e.top,r=t.left,a=t.top,l=e.right,c=e.bottom,h=t.left,u=t.bottom):(i=t.right,o=t.top,r=e.left,a=e.top,l=t.right,c=t.bottom,h=e.left,u=e.bottom),this.style.setProperty("--hover-bridge-top-left-x",`${i}px`),this.style.setProperty("--hover-bridge-top-left-y",`${o}px`),this.style.setProperty("--hover-bridge-top-right-x",`${r}px`),this.style.setProperty("--hover-bridge-top-right-y",`${a}px`),this.style.setProperty("--hover-bridge-bottom-left-x",`${l}px`),this.style.setProperty("--hover-bridge-bottom-left-y",`${c}px`),this.style.setProperty("--hover-bridge-bottom-right-x",`${h}px`),this.style.setProperty("--hover-bridge-bottom-right-y",`${u}px`)}}}async connectedCallback(){super.connectedCallback(),await this.updateComplete,this.start()}disconnectedCallback(){super.disconnectedCallback(),this.stop()}async updated(e){super.updated(e),e.has("active")&&(this.active?this.start():this.stop()),e.has("anchor")&&this.handleAnchorChange(),this.active&&(await this.updateComplete,this.reposition())}async handleAnchorChange(){if(await this.stop(),this.anchor&&typeof this.anchor=="string"){const e=this.getRootNode();this.anchorEl=e.getElementById(this.anchor)}else this.anchor instanceof Element||Cd(this.anchor)?this.anchorEl=this.anchor:this.anchorEl=this.querySelector('[slot="anchor"]');this.anchorEl instanceof HTMLSlotElement&&(this.anchorEl=this.anchorEl.assignedElements({flatten:!0})[0]),this.anchorEl&&this.active&&this.start()}start(){this.anchorEl&&(this.cleanup=bd(this.anchorEl,this.popup,()=>{this.reposition()}))}async stop(){return new Promise(e=>{this.cleanup?(this.cleanup(),this.cleanup=void 0,this.removeAttribute("data-current-placement"),this.style.removeProperty("--auto-size-available-width"),this.style.removeProperty("--auto-size-available-height"),requestAnimationFrame(()=>e())):e()})}reposition(){if(!this.active||!this.anchorEl)return;const e=[vd({mainAxis:this.distance,crossAxis:this.skidding})];this.sync?e.push(Ga({apply:({rects:s})=>{const i=this.sync==="width"||this.sync==="both",o=this.sync==="height"||this.sync==="both";this.popup.style.width=i?`${s.reference.width}px`:"",this.popup.style.height=o?`${s.reference.height}px`:""}})):(this.popup.style.width="",this.popup.style.height=""),this.flip&&e.push(wd({boundary:this.flipBoundary,fallbackPlacements:this.flipFallbackPlacements,fallbackStrategy:this.flipFallbackStrategy==="best-fit"?"bestFit":"initialPlacement",padding:this.flipPadding})),this.shift&&e.push(yd({boundary:this.shiftBoundary,padding:this.shiftPadding})),this.autoSize?e.push(Ga({boundary:this.autoSizeBoundary,padding:this.autoSizePadding,apply:({availableWidth:s,availableHeight:i})=>{this.autoSize==="vertical"||this.autoSize==="both"?this.style.setProperty("--auto-size-available-height",`${i}px`):this.style.removeProperty("--auto-size-available-height"),this.autoSize==="horizontal"||this.autoSize==="both"?this.style.setProperty("--auto-size-available-width",`${s}px`):this.style.removeProperty("--auto-size-available-width")}})):(this.style.removeProperty("--auto-size-available-width"),this.style.removeProperty("--auto-size-available-height")),this.arrow&&e.push(xd({element:this.arrowEl,padding:this.arrowPadding}));const t=this.strategy==="absolute"?s=>so.getOffsetParent(s,kd):so.getOffsetParent;_d(this.anchorEl,this.popup,{placement:this.placement,middleware:e,strategy:this.strategy,platform:Si(Ne({},so),{getOffsetParent:t})}).then(({x:s,y:i,middlewareData:o,placement:r})=>{const a=this.localize.dir()==="rtl",l={top:"bottom",right:"left",bottom:"top",left:"right"}[r.split("-")[0]];if(this.setAttribute("data-current-placement",r),Object.assign(this.popup.style,{left:`${s}px`,top:`${i}px`}),this.arrow){const c=o.arrow.x,h=o.arrow.y;let u="",p="",f="",m="";if(this.arrowPlacement==="start"){const b=typeof c=="number"?`calc(${this.arrowPadding}px - var(--arrow-padding-offset))`:"";u=typeof h=="number"?`calc(${this.arrowPadding}px - var(--arrow-padding-offset))`:"",p=a?b:"",m=a?"":b}else if(this.arrowPlacement==="end"){const b=typeof c=="number"?`calc(${this.arrowPadding}px - var(--arrow-padding-offset))`:"";p=a?"":b,m=a?b:"",f=typeof h=="number"?`calc(${this.arrowPadding}px - var(--arrow-padding-offset))`:""}else this.arrowPlacement==="center"?(m=typeof c=="number"?"calc(50% - var(--arrow-size-diagonal))":"",u=typeof h=="number"?"calc(50% - var(--arrow-size-diagonal))":""):(m=typeof c=="number"?`${c}px`:"",u=typeof h=="number"?`${h}px`:"");Object.assign(this.arrowEl.style,{top:u,right:p,bottom:f,left:m,[l]:"calc(var(--arrow-size-diagonal) * -1)"})}}),requestAnimationFrame(()=>this.updateHoverBridge()),this.emit("sl-reposition")}render(){return v`
      <slot name="anchor" @slotchange=${this.handleAnchorChange}></slot>

      <span
        part="hover-bridge"
        class=${L({"popup-hover-bridge":!0,"popup-hover-bridge--visible":this.hoverBridge&&this.active})}
      ></span>

      <div
        part="popup"
        class=${L({popup:!0,"popup--active":this.active,"popup--fixed":this.strategy==="fixed","popup--has-arrow":this.arrow})}
      >
        <slot></slot>
        ${this.arrow?v`<div part="arrow" class="popup__arrow" role="presentation"></div>`:""}
      </div>
    `}};X.styles=[M,Nc];n([T(".popup")],X.prototype,"popup",2);n([T(".popup__arrow")],X.prototype,"arrowEl",2);n([d()],X.prototype,"anchor",2);n([d({type:Boolean,reflect:!0})],X.prototype,"active",2);n([d({reflect:!0})],X.prototype,"placement",2);n([d({reflect:!0})],X.prototype,"strategy",2);n([d({type:Number})],X.prototype,"distance",2);n([d({type:Number})],X.prototype,"skidding",2);n([d({type:Boolean})],X.prototype,"arrow",2);n([d({attribute:"arrow-placement"})],X.prototype,"arrowPlacement",2);n([d({attribute:"arrow-padding",type:Number})],X.prototype,"arrowPadding",2);n([d({type:Boolean})],X.prototype,"flip",2);n([d({attribute:"flip-fallback-placements",converter:{fromAttribute:e=>e.split(" ").map(t=>t.trim()).filter(t=>t!==""),toAttribute:e=>e.join(" ")}})],X.prototype,"flipFallbackPlacements",2);n([d({attribute:"flip-fallback-strategy"})],X.prototype,"flipFallbackStrategy",2);n([d({type:Object})],X.prototype,"flipBoundary",2);n([d({attribute:"flip-padding",type:Number})],X.prototype,"flipPadding",2);n([d({type:Boolean})],X.prototype,"shift",2);n([d({type:Object})],X.prototype,"shiftBoundary",2);n([d({attribute:"shift-padding",type:Number})],X.prototype,"shiftPadding",2);n([d({attribute:"auto-size"})],X.prototype,"autoSize",2);n([d()],X.prototype,"sync",2);n([d({type:Object})],X.prototype,"autoSizeBoundary",2);n([d({attribute:"auto-size-padding",type:Number})],X.prototype,"autoSizePadding",2);n([d({attribute:"hover-bridge",type:Boolean})],X.prototype,"hoverBridge",2);var tl=new Map,Sd=new WeakMap;function Ad(e){return e??{keyframes:[],options:{duration:0}}}function Xa(e,t){return t.toLowerCase()==="rtl"?{keyframes:e.rtlKeyframes||e.keyframes,options:e.options}:e}function Z(e,t){tl.set(e,Ad(t))}function nt(e,t,s){const i=Sd.get(e);if(i!=null&&i[t])return Xa(i[t],s.dir);const o=tl.get(t);return o?Xa(o,s.dir):{keyframes:[],options:{duration:0}}}function Bt(e,t){return new Promise(s=>{function i(o){o.target===e&&(e.removeEventListener(t,i),s())}e.addEventListener(t,i)})}function ht(e,t,s){return new Promise(i=>{if((s==null?void 0:s.duration)===1/0)throw new Error("Promise-based animations must be finite.");const o=e.animate(t,Si(Ne({},s),{duration:mr()?0:s.duration}));o.addEventListener("cancel",i,{once:!0}),o.addEventListener("finish",i,{once:!0})})}function Za(e){return e=e.toString().toLowerCase(),e.indexOf("ms")>-1?parseFloat(e):e.indexOf("s")>-1?parseFloat(e)*1e3:parseFloat(e)}function mr(){return window.matchMedia("(prefers-reduced-motion: reduce)").matches}function bt(e){return Promise.all(e.getAnimations().map(t=>new Promise(s=>{t.cancel(),requestAnimationFrame(s)})))}function lo(e,t){return e.map(s=>Si(Ne({},s),{height:s.height==="auto"?`${t}px`:s.height}))}function C(e,t){const s=Ne({waitUntilFirstUpdate:!1},t);return(i,o)=>{const{update:r}=i,a=Array.isArray(e)?e:[e];i.update=function(l){a.forEach(c=>{const h=c;if(l.has(h)){const u=l.get(h),p=this[h];u!==p&&(!s.waitUntilFirstUpdate||this.hasUpdated)&&this[o](u,p)}}),r.call(this,l)}}}var kt=class extends P{constructor(){super(),this.localize=new K(this),this.content="",this.placement="top",this.disabled=!1,this.distance=8,this.open=!1,this.skidding=0,this.trigger="hover focus",this.hoist=!1,this.handleBlur=()=>{this.hasTrigger("focus")&&this.hide()},this.handleClick=()=>{this.hasTrigger("click")&&(this.open?this.hide():this.show())},this.handleFocus=()=>{this.hasTrigger("focus")&&this.show()},this.handleDocumentKeyDown=e=>{e.key==="Escape"&&(e.stopPropagation(),this.hide())},this.handleMouseOver=()=>{if(this.hasTrigger("hover")){const e=Za(getComputedStyle(this).getPropertyValue("--show-delay"));clearTimeout(this.hoverTimeout),this.hoverTimeout=window.setTimeout(()=>this.show(),e)}},this.handleMouseOut=()=>{if(this.hasTrigger("hover")){const e=Za(getComputedStyle(this).getPropertyValue("--hide-delay"));clearTimeout(this.hoverTimeout),this.hoverTimeout=window.setTimeout(()=>this.hide(),e)}},this.addEventListener("blur",this.handleBlur,!0),this.addEventListener("focus",this.handleFocus,!0),this.addEventListener("click",this.handleClick),this.addEventListener("mouseover",this.handleMouseOver),this.addEventListener("mouseout",this.handleMouseOut)}disconnectedCallback(){var e;super.disconnectedCallback(),(e=this.closeWatcher)==null||e.destroy(),document.removeEventListener("keydown",this.handleDocumentKeyDown)}firstUpdated(){this.body.hidden=!this.open,this.open&&(this.popup.active=!0,this.popup.reposition())}hasTrigger(e){return this.trigger.split(" ").includes(e)}async handleOpenChange(){var e,t;if(this.open){if(this.disabled)return;this.emit("sl-show"),"CloseWatcher"in window?((e=this.closeWatcher)==null||e.destroy(),this.closeWatcher=new CloseWatcher,this.closeWatcher.onclose=()=>{this.hide()}):document.addEventListener("keydown",this.handleDocumentKeyDown),await bt(this.body),this.body.hidden=!1,this.popup.active=!0;const{keyframes:s,options:i}=nt(this,"tooltip.show",{dir:this.localize.dir()});await ht(this.popup.popup,s,i),this.popup.reposition(),this.emit("sl-after-show")}else{this.emit("sl-hide"),(t=this.closeWatcher)==null||t.destroy(),document.removeEventListener("keydown",this.handleDocumentKeyDown),await bt(this.body);const{keyframes:s,options:i}=nt(this,"tooltip.hide",{dir:this.localize.dir()});await ht(this.popup.popup,s,i),this.popup.active=!1,this.body.hidden=!0,this.emit("sl-after-hide")}}async handleOptionsChange(){this.hasUpdated&&(await this.updateComplete,this.popup.reposition())}handleDisabledChange(){this.disabled&&this.open&&this.hide()}async show(){if(!this.open)return this.open=!0,Bt(this,"sl-after-show")}async hide(){if(this.open)return this.open=!1,Bt(this,"sl-after-hide")}render(){return v`
      <sl-popup
        part="base"
        exportparts="
          popup:base__popup,
          arrow:base__arrow
        "
        class=${L({tooltip:!0,"tooltip--open":this.open})}
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
    `}};kt.styles=[M,Mc];kt.dependencies={"sl-popup":X};n([T("slot:not([name])")],kt.prototype,"defaultSlot",2);n([T(".tooltip__body")],kt.prototype,"body",2);n([T("sl-popup")],kt.prototype,"popup",2);n([d()],kt.prototype,"content",2);n([d()],kt.prototype,"placement",2);n([d({type:Boolean,reflect:!0})],kt.prototype,"disabled",2);n([d({type:Number})],kt.prototype,"distance",2);n([d({type:Boolean,reflect:!0})],kt.prototype,"open",2);n([d({type:Number})],kt.prototype,"skidding",2);n([d()],kt.prototype,"trigger",2);n([d({type:Boolean})],kt.prototype,"hoist",2);n([C("open",{waitUntilFirstUpdate:!0})],kt.prototype,"handleOpenChange",1);n([C(["content","distance","hoist","placement","skidding"])],kt.prototype,"handleOptionsChange",1);n([C("disabled")],kt.prototype,"handleDisabledChange",1);Z("tooltip.show",{keyframes:[{opacity:0,scale:.8},{opacity:1,scale:1}],options:{duration:150,easing:"ease"}});Z("tooltip.hide",{keyframes:[{opacity:1,scale:1},{opacity:0,scale:.8}],options:{duration:150,easing:"ease"}});kt.define("sl-tooltip");var Td=A`
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
`,zd=A`
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
`,Ed=A`
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
`,cs=(e="value")=>(t,s)=>{const i=t.constructor,o=i.prototype.attributeChangedCallback;i.prototype.attributeChangedCallback=function(r,a,l){var c;const h=i.getPropertyOptions(e),u=typeof h.attribute=="string"?h.attribute:e;if(r===u){const p=h.converter||Ts,m=(typeof p=="function"?p:(c=p==null?void 0:p.fromAttribute)!=null?c:Ts.fromAttribute)(l,h.type);this[e]!==m&&(this[s]=m)}o.call(this,r,a,l)}},ds=A`
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
`,Ht=class{constructor(e,...t){this.slotNames=[],this.handleSlotChange=s=>{const i=s.target;(this.slotNames.includes("[default]")&&!i.name||i.name&&this.slotNames.includes(i.name))&&this.host.requestUpdate()},(this.host=e).addController(this),this.slotNames=t}hasDefaultSlot(){return[...this.host.childNodes].some(e=>{if(e.nodeType===e.TEXT_NODE&&e.textContent.trim()!=="")return!0;if(e.nodeType===e.ELEMENT_NODE){const t=e;if(t.tagName.toLowerCase()==="sl-visually-hidden")return!1;if(!t.hasAttribute("slot"))return!0}return!1})}hasNamedSlot(e){return this.host.querySelector(`:scope > [slot="${e}"]`)!==null}test(e){return e==="[default]"?this.hasDefaultSlot():this.hasNamedSlot(e)}hostConnected(){this.host.shadowRoot.addEventListener("slotchange",this.handleSlotChange)}hostDisconnected(){this.host.shadowRoot.removeEventListener("slotchange",this.handleSlotChange)}};function Od(e){if(!e)return"";const t=e.assignedNodes({flatten:!0});let s="";return[...t].forEach(i=>{i.nodeType===Node.TEXT_NODE&&(s+=i.textContent)}),s}var gr="";function Qa(e){gr=e}function Pd(e=""){if(!gr){const t=[...document.getElementsByTagName("script")],s=t.find(i=>i.hasAttribute("data-shoelace"));if(s)Qa(s.getAttribute("data-shoelace"));else{const i=t.find(r=>/shoelace(\.min)?\.js($|\?)/.test(r.src)||/shoelace-autoloader(\.min)?\.js($|\?)/.test(r.src));let o="";i&&(o=i.getAttribute("src")),Qa(o.split("/").slice(0,-1).join("/"))}}return gr.replace(/\/$/,"")+(e?`/${e.replace(/^\//,"")}`:"")}var Dd={name:"default",resolver:e=>Pd(`assets/icons/${e}.svg`)},Id=Dd,Ja={caret:`
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
  `},Rd={name:"system",resolver:e=>e in Ja?`data:image/svg+xml,${encodeURIComponent(Ja[e])}`:""},Ld=Rd,Md=[Id,Ld],br=[];function Nd(e){br.push(e)}function Fd(e){br=br.filter(t=>t!==e)}function tn(e){return Md.find(t=>t.name===e)}var Bd=A`
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
`;/**
 * @license
 * Copyright 2020 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Ud=e=>e===null||typeof e!="object"&&typeof e!="function",Hd=(e,t)=>(e==null?void 0:e._$litType$)!==void 0,el=e=>e.strings===void 0,Vd={},jd=(e,t=Vd)=>e._$AH=t;var Xs=Symbol(),qi=Symbol(),Jo,tr=new Map,it=class extends P{constructor(){super(...arguments),this.initialRender=!1,this.svg=null,this.label="",this.library="default"}async resolveIcon(e,t){var s;let i;if(t!=null&&t.spriteSheet)return this.svg=v`<svg part="svg">
        <use part="use" href="${e}"></use>
      </svg>`,this.svg;try{if(i=await fetch(e,{mode:"cors"}),!i.ok)return i.status===410?Xs:qi}catch{return qi}try{const o=document.createElement("div");o.innerHTML=await i.text();const r=o.firstElementChild;if(((s=r==null?void 0:r.tagName)==null?void 0:s.toLowerCase())!=="svg")return Xs;Jo||(Jo=new DOMParser);const l=Jo.parseFromString(r.outerHTML,"text/html").body.querySelector("svg");return l?(l.part.add("svg"),document.adoptNode(l)):Xs}catch{return Xs}}connectedCallback(){super.connectedCallback(),Nd(this)}firstUpdated(){this.initialRender=!0,this.setIcon()}disconnectedCallback(){super.disconnectedCallback(),Fd(this)}getIconSource(){const e=tn(this.library);return this.name&&e?{url:e.resolver(this.name),fromLibrary:!0}:{url:this.src,fromLibrary:!1}}handleLabelChange(){typeof this.label=="string"&&this.label.length>0?(this.setAttribute("role","img"),this.setAttribute("aria-label",this.label),this.removeAttribute("aria-hidden")):(this.removeAttribute("role"),this.removeAttribute("aria-label"),this.setAttribute("aria-hidden","true"))}async setIcon(){var e;const{url:t,fromLibrary:s}=this.getIconSource(),i=s?tn(this.library):void 0;if(!t){this.svg=null;return}let o=tr.get(t);if(o||(o=this.resolveIcon(t,i),tr.set(t,o)),!this.initialRender)return;const r=await o;if(r===qi&&tr.delete(t),t===this.getIconSource().url){if(Hd(r)){if(this.svg=r,i){await this.updateComplete;const a=this.shadowRoot.querySelector("[part='svg']");typeof i.mutator=="function"&&a&&i.mutator(a)}return}switch(r){case qi:case Xs:this.svg=null,this.emit("sl-error");break;default:this.svg=r.cloneNode(!0),(e=i==null?void 0:i.mutator)==null||e.call(i,this.svg),this.emit("sl-load")}}}render(){return this.svg}};it.styles=[M,Bd];n([E()],it.prototype,"svg",2);n([d({reflect:!0})],it.prototype,"name",2);n([d()],it.prototype,"src",2);n([d()],it.prototype,"label",2);n([d({reflect:!0})],it.prototype,"library",2);n([C("label")],it.prototype,"handleLabelChange",1);n([C(["name","src","library"])],it.prototype,"setIcon",1);/**
 * @license
 * Copyright 2018 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const O=e=>e??G;/**
 * @license
 * Copyright 2020 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const as=Ls(class extends Ti{constructor(e){if(super(e),e.type!==Ce.PROPERTY&&e.type!==Ce.ATTRIBUTE&&e.type!==Ce.BOOLEAN_ATTRIBUTE)throw Error("The `live` directive is not allowed on child or event bindings");if(!el(e))throw Error("`live` bindings can only contain a single expression")}render(e){return e}update(e,[t]){if(t===Nt||t===G)return t;const s=e.element,i=e.name;if(e.type===Ce.PROPERTY){if(t===s[i])return Nt}else if(e.type===Ce.BOOLEAN_ATTRIBUTE){if(!!t===s.hasAttribute(i))return Nt}else if(e.type===Ce.ATTRIBUTE&&s.getAttribute(i)===t+"")return Nt;return jd(e),t}});var yt=class extends P{constructor(){super(...arguments),this.formControlController=new Fe(this,{value:e=>e.checked?e.value||"on":void 0,defaultValue:e=>e.defaultChecked,setValue:(e,t)=>e.checked=t}),this.hasSlotController=new Ht(this,"help-text"),this.hasFocus=!1,this.title="",this.name="",this.size="medium",this.disabled=!1,this.checked=!1,this.indeterminate=!1,this.defaultChecked=!1,this.form="",this.required=!1,this.helpText=""}get validity(){return this.input.validity}get validationMessage(){return this.input.validationMessage}firstUpdated(){this.formControlController.updateValidity()}handleClick(){this.checked=!this.checked,this.indeterminate=!1,this.emit("sl-change")}handleBlur(){this.hasFocus=!1,this.emit("sl-blur")}handleInput(){this.emit("sl-input")}handleInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}handleFocus(){this.hasFocus=!0,this.emit("sl-focus")}handleDisabledChange(){this.formControlController.setValidity(this.disabled)}handleStateChange(){this.input.checked=this.checked,this.input.indeterminate=this.indeterminate,this.formControlController.updateValidity()}click(){this.input.click()}focus(e){this.input.focus(e)}blur(){this.input.blur()}checkValidity(){return this.input.checkValidity()}getForm(){return this.formControlController.getForm()}reportValidity(){return this.input.reportValidity()}setCustomValidity(e){this.input.setCustomValidity(e),this.formControlController.updateValidity()}render(){const e=this.hasSlotController.test("help-text"),t=this.helpText?!0:!!e;return v`
      <div
        class=${L({"form-control":!0,"form-control--small":this.size==="small","form-control--medium":this.size==="medium","form-control--large":this.size==="large","form-control--has-help-text":t})}
      >
        <label
          part="base"
          class=${L({checkbox:!0,"checkbox--checked":this.checked,"checkbox--disabled":this.disabled,"checkbox--focused":this.hasFocus,"checkbox--indeterminate":this.indeterminate,"checkbox--small":this.size==="small","checkbox--medium":this.size==="medium","checkbox--large":this.size==="large"})}
        >
          <input
            class="checkbox__input"
            type="checkbox"
            title=${this.title}
            name=${this.name}
            value=${O(this.value)}
            .indeterminate=${as(this.indeterminate)}
            .checked=${as(this.checked)}
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
            ${this.checked?v`
                  <sl-icon part="checked-icon" class="checkbox__checked-icon" library="system" name="check"></sl-icon>
                `:""}
            ${!this.checked&&this.indeterminate?v`
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
    `}};yt.styles=[M,ds,Ed];yt.dependencies={"sl-icon":it};n([T('input[type="checkbox"]')],yt.prototype,"input",2);n([E()],yt.prototype,"hasFocus",2);n([d()],yt.prototype,"title",2);n([d()],yt.prototype,"name",2);n([d()],yt.prototype,"value",2);n([d({reflect:!0})],yt.prototype,"size",2);n([d({type:Boolean,reflect:!0})],yt.prototype,"disabled",2);n([d({type:Boolean,reflect:!0})],yt.prototype,"checked",2);n([d({type:Boolean,reflect:!0})],yt.prototype,"indeterminate",2);n([cs("checked")],yt.prototype,"defaultChecked",2);n([d({reflect:!0})],yt.prototype,"form",2);n([d({type:Boolean,reflect:!0})],yt.prototype,"required",2);n([d({attribute:"help-text"})],yt.prototype,"helpText",2);n([C("disabled",{waitUntilFirstUpdate:!0})],yt.prototype,"handleDisabledChange",1);n([C(["checked","indeterminate"],{waitUntilFirstUpdate:!0})],yt.prototype,"handleStateChange",1);var Wd=A`
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
`,zi=class extends P{constructor(){super(...arguments),this.localize=new K(this)}render(){return v`
      <svg part="base" class="spinner" role="progressbar" aria-label=${this.localize.term("loading")}>
        <circle class="spinner__track"></circle>
        <circle class="spinner__indicator"></circle>
      </svg>
    `}};zi.styles=[M,Wd];/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function en(e,t,s){return e?t(e):s==null?void 0:s(e)}var ut=class vr extends P{constructor(){super(...arguments),this.localize=new K(this),this.indeterminate=!1,this.isLeaf=!1,this.loading=!1,this.selectable=!1,this.expanded=!1,this.selected=!1,this.disabled=!1,this.lazy=!1}static isTreeItem(t){return t instanceof Element&&t.getAttribute("role")==="treeitem"}connectedCallback(){super.connectedCallback(),this.setAttribute("role","treeitem"),this.setAttribute("tabindex","-1"),this.isNestedItem()&&(this.slot="children")}firstUpdated(){this.childrenContainer.hidden=!this.expanded,this.childrenContainer.style.height=this.expanded?"auto":"0",this.isLeaf=!this.lazy&&this.getChildrenItems().length===0,this.handleExpandedChange()}async animateCollapse(){this.emit("sl-collapse"),await bt(this.childrenContainer);const{keyframes:t,options:s}=nt(this,"tree-item.collapse",{dir:this.localize.dir()});await ht(this.childrenContainer,lo(t,this.childrenContainer.scrollHeight),s),this.childrenContainer.hidden=!0,this.emit("sl-after-collapse")}isNestedItem(){const t=this.parentElement;return!!t&&vr.isTreeItem(t)}handleChildrenSlotChange(){this.loading=!1,this.isLeaf=!this.lazy&&this.getChildrenItems().length===0}willUpdate(t){t.has("selected")&&!t.has("indeterminate")&&(this.indeterminate=!1)}async animateExpand(){this.emit("sl-expand"),await bt(this.childrenContainer),this.childrenContainer.hidden=!1;const{keyframes:t,options:s}=nt(this,"tree-item.expand",{dir:this.localize.dir()});await ht(this.childrenContainer,lo(t,this.childrenContainer.scrollHeight),s),this.childrenContainer.style.height="auto",this.emit("sl-after-expand")}handleLoadingChange(){this.setAttribute("aria-busy",this.loading?"true":"false"),this.loading||this.animateExpand()}handleDisabledChange(){this.setAttribute("aria-disabled",this.disabled?"true":"false")}handleSelectedChange(){this.setAttribute("aria-selected",this.selected?"true":"false")}handleExpandedChange(){this.isLeaf?this.removeAttribute("aria-expanded"):this.setAttribute("aria-expanded",this.expanded?"true":"false")}handleExpandAnimation(){this.expanded?this.lazy?(this.loading=!0,this.emit("sl-lazy-load")):this.animateExpand():this.animateCollapse()}handleLazyChange(){this.emit("sl-lazy-change")}getChildrenItems({includeDisabled:t=!0}={}){return this.childrenSlot?[...this.childrenSlot.assignedElements({flatten:!0})].filter(s=>vr.isTreeItem(s)&&(t||!s.disabled)):[]}render(){const t=this.localize.dir()==="rtl",s=!this.loading&&(!this.isLeaf||this.lazy);return v`
      <div
        part="base"
        class="${L({"tree-item":!0,"tree-item--expanded":this.expanded,"tree-item--selected":this.selected,"tree-item--disabled":this.disabled,"tree-item--leaf":this.isLeaf,"tree-item--has-expand-button":s,"tree-item--rtl":this.localize.dir()==="rtl"})}"
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
            class=${L({"tree-item__expand-button":!0,"tree-item__expand-button--visible":s})}
            aria-hidden="true"
          >
            ${en(this.loading,()=>v` <sl-spinner part="spinner" exportparts="base:spinner__base"></sl-spinner> `)}
            <slot class="tree-item__expand-icon-slot" name="expand-icon">
              <sl-icon library="system" name=${t?"chevron-left":"chevron-right"}></sl-icon>
            </slot>
            <slot class="tree-item__expand-icon-slot" name="collapse-icon">
              <sl-icon library="system" name=${t?"chevron-left":"chevron-right"}></sl-icon>
            </slot>
          </div>

          ${en(this.selectable,()=>v`
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
                ?checked="${as(this.selected)}"
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
    `}};ut.styles=[M,zd];ut.dependencies={"sl-checkbox":yt,"sl-icon":it,"sl-spinner":zi};n([E()],ut.prototype,"indeterminate",2);n([E()],ut.prototype,"isLeaf",2);n([E()],ut.prototype,"loading",2);n([E()],ut.prototype,"selectable",2);n([d({type:Boolean,reflect:!0})],ut.prototype,"expanded",2);n([d({type:Boolean,reflect:!0})],ut.prototype,"selected",2);n([d({type:Boolean,reflect:!0})],ut.prototype,"disabled",2);n([d({type:Boolean,reflect:!0})],ut.prototype,"lazy",2);n([T("slot:not([name])")],ut.prototype,"defaultSlot",2);n([T("slot[name=children]")],ut.prototype,"childrenSlot",2);n([T(".tree-item__item")],ut.prototype,"itemElement",2);n([T(".tree-item__children")],ut.prototype,"childrenContainer",2);n([T(".tree-item__expand-button slot")],ut.prototype,"expandButtonSlot",2);n([C("loading",{waitUntilFirstUpdate:!0})],ut.prototype,"handleLoadingChange",1);n([C("disabled")],ut.prototype,"handleDisabledChange",1);n([C("selected")],ut.prototype,"handleSelectedChange",1);n([C("expanded",{waitUntilFirstUpdate:!0})],ut.prototype,"handleExpandedChange",1);n([C("expanded",{waitUntilFirstUpdate:!0})],ut.prototype,"handleExpandAnimation",1);n([C("lazy",{waitUntilFirstUpdate:!0})],ut.prototype,"handleLazyChange",1);var li=ut;Z("tree-item.expand",{keyframes:[{height:"0",opacity:"0",overflow:"hidden"},{height:"auto",opacity:"1",overflow:"hidden"}],options:{duration:250,easing:"cubic-bezier(0.4, 0.0, 0.2, 1)"}});Z("tree-item.collapse",{keyframes:[{height:"auto",opacity:"1",overflow:"hidden"},{height:"0",opacity:"0",overflow:"hidden"}],options:{duration:200,easing:"cubic-bezier(0.4, 0.0, 0.2, 1)"}});function gt(e,t,s){const i=o=>Object.is(o,-0)?0:o;return e<t?i(t):e>s?i(s):i(e)}function sn(e,t=!1){function s(r){const a=r.getChildrenItems({includeDisabled:!1});if(a.length){const l=a.every(h=>h.selected),c=a.every(h=>!h.selected&&!h.indeterminate);r.selected=l,r.indeterminate=!l&&!c}}function i(r){const a=r.parentElement;li.isTreeItem(a)&&(s(a),i(a))}function o(r){for(const a of r.getChildrenItems())a.selected=t?r.selected||a.selected:!a.disabled&&r.selected,o(a);t&&s(r)}o(e),i(e)}var hs=class extends P{constructor(){super(),this.selection="single",this.clickTarget=null,this.localize=new K(this),this.initTreeItem=e=>{e.selectable=this.selection==="multiple",["expand","collapse"].filter(t=>!!this.querySelector(`[slot="${t}-icon"]`)).forEach(t=>{const s=e.querySelector(`[slot="${t}-icon"]`),i=this.getExpandButtonIcon(t);i&&(s===null?e.append(i):s.hasAttribute("data-default")&&s.replaceWith(i))})},this.handleTreeChanged=e=>{for(const t of e){const s=[...t.addedNodes].filter(li.isTreeItem),i=[...t.removedNodes].filter(li.isTreeItem);s.forEach(this.initTreeItem),this.lastFocusedItem&&i.includes(this.lastFocusedItem)&&(this.lastFocusedItem=null)}},this.handleFocusOut=e=>{const t=e.relatedTarget;(!t||!this.contains(t))&&(this.tabIndex=0)},this.handleFocusIn=e=>{const t=e.target;e.target===this&&this.focusItem(this.lastFocusedItem||this.getAllTreeItems()[0]),li.isTreeItem(t)&&!t.disabled&&(this.lastFocusedItem&&(this.lastFocusedItem.tabIndex=-1),this.lastFocusedItem=t,this.tabIndex=-1,t.tabIndex=0)},this.addEventListener("focusin",this.handleFocusIn),this.addEventListener("focusout",this.handleFocusOut),this.addEventListener("sl-lazy-change",this.handleSlotChange)}async connectedCallback(){super.connectedCallback(),this.setAttribute("role","tree"),this.setAttribute("tabindex","0"),await this.updateComplete,this.mutationObserver=new MutationObserver(this.handleTreeChanged),this.mutationObserver.observe(this,{childList:!0,subtree:!0})}disconnectedCallback(){var e;super.disconnectedCallback(),(e=this.mutationObserver)==null||e.disconnect()}getExpandButtonIcon(e){const s=(e==="expand"?this.expandedIconSlot:this.collapsedIconSlot).assignedElements({flatten:!0})[0];if(s){const i=s.cloneNode(!0);return[i,...i.querySelectorAll("[id]")].forEach(o=>o.removeAttribute("id")),i.setAttribute("data-default",""),i.slot=`${e}-icon`,i}return null}selectItem(e){const t=[...this.selectedItems];if(this.selection==="multiple")e.selected=!e.selected,e.lazy&&(e.expanded=!0),sn(e);else if(this.selection==="single"||e.isLeaf){const i=this.getAllTreeItems();for(const o of i)o.selected=o===e}else this.selection==="leaf"&&(e.expanded=!e.expanded);const s=this.selectedItems;(t.length!==s.length||s.some(i=>!t.includes(i)))&&Promise.all(s.map(i=>i.updateComplete)).then(()=>{this.emit("sl-selection-change",{detail:{selection:s}})})}getAllTreeItems(){return[...this.querySelectorAll("sl-tree-item")]}focusItem(e){e==null||e.focus()}handleKeyDown(e){if(!["ArrowDown","ArrowUp","ArrowRight","ArrowLeft","Home","End","Enter"," "].includes(e.key)||e.composedPath().some(o=>{var r;return["input","textarea"].includes((r=o==null?void 0:o.tagName)==null?void 0:r.toLowerCase())}))return;const t=this.getFocusableItems(),s=this.localize.dir()==="ltr",i=this.localize.dir()==="rtl";if(t.length>0){e.preventDefault();const o=t.findIndex(c=>c.matches(":focus")),r=t[o],a=c=>{const h=t[gt(c,0,t.length-1)];this.focusItem(h)},l=c=>{r.expanded=c};e.key==="ArrowDown"?a(o+1):e.key==="ArrowUp"?a(o-1):s&&e.key==="ArrowRight"||i&&e.key==="ArrowLeft"?!r||r.disabled||r.expanded||r.isLeaf&&!r.lazy?a(o+1):l(!0):s&&e.key==="ArrowLeft"||i&&e.key==="ArrowRight"?!r||r.disabled||r.isLeaf||!r.expanded?a(o-1):l(!1):e.key==="Home"?a(0):e.key==="End"?a(t.length-1):(e.key==="Enter"||e.key===" ")&&(r.disabled||this.selectItem(r))}}handleClick(e){const t=e.target,s=t.closest("sl-tree-item"),i=e.composedPath().some(o=>{var r;return(r=o==null?void 0:o.classList)==null?void 0:r.contains("tree-item__expand-button")});!s||s.disabled||t!==this.clickTarget||(i?s.expanded=!s.expanded:this.selectItem(s))}handleMouseDown(e){this.clickTarget=e.target}handleSlotChange(){this.getAllTreeItems().forEach(this.initTreeItem)}async handleSelectionChange(){const e=this.selection==="multiple",t=this.getAllTreeItems();this.setAttribute("aria-multiselectable",e?"true":"false");for(const s of t)s.selectable=e;e&&(await this.updateComplete,[...this.querySelectorAll(":scope > sl-tree-item")].forEach(s=>sn(s,!0)))}get selectedItems(){const e=this.getAllTreeItems(),t=s=>s.selected;return e.filter(t)}getFocusableItems(){const e=this.getAllTreeItems(),t=new Set;return e.filter(s=>{var i;if(s.disabled)return!1;const o=(i=s.parentElement)==null?void 0:i.closest("[role=treeitem]");return o&&(!o.expanded||o.loading||t.has(o))&&t.add(s),!t.has(s)})}render(){return v`
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
    `}};hs.styles=[M,Td];n([T("slot:not([name])")],hs.prototype,"defaultSlot",2);n([T("slot[name=expand-icon]")],hs.prototype,"expandedIconSlot",2);n([T("slot[name=collapse-icon]")],hs.prototype,"collapsedIconSlot",2);n([d()],hs.prototype,"selection",2);n([C("selection")],hs.prototype,"handleSelectionChange",1);hs.define("sl-tree");li.define("sl-tree-item");var qd=A`
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
`,Kd=A`
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
 */const sl=Symbol.for(""),Yd=e=>{if((e==null?void 0:e.r)===sl)return e==null?void 0:e._$litStatic$},co=(e,...t)=>({_$litStatic$:t.reduce((s,i,o)=>s+(r=>{if(r._$litStatic$!==void 0)return r._$litStatic$;throw Error(`Value passed to 'literal' function must be a 'literal' result: ${r}. Use 'unsafeStatic' to pass non-literal values, but
            take care to ensure page security.`)})(i)+e[o+1],e[0]),r:sl}),on=new Map,Gd=e=>(t,...s)=>{const i=s.length;let o,r;const a=[],l=[];let c,h=0,u=!1;for(;h<i;){for(c=t[h];h<i&&(r=s[h],(o=Yd(r))!==void 0);)c+=o+t[++h],u=!0;h!==i&&l.push(r),a.push(c),h++}if(h===i&&a.push(t[i]),u){const p=a.join("$$lit$$");(t=on.get(p))===void 0&&(a.raw=a,on.set(p,t=a)),s=l}return e(t,...s)},ci=Gd(v);var wt=class extends P{constructor(){super(...arguments),this.hasFocus=!1,this.label="",this.disabled=!1}handleBlur(){this.hasFocus=!1,this.emit("sl-blur")}handleFocus(){this.hasFocus=!0,this.emit("sl-focus")}handleClick(e){this.disabled&&(e.preventDefault(),e.stopPropagation())}click(){this.button.click()}focus(e){this.button.focus(e)}blur(){this.button.blur()}render(){const e=!!this.href,t=e?co`a`:co`button`;return ci`
      <${t}
        part="base"
        class=${L({"icon-button":!0,"icon-button--disabled":!e&&this.disabled,"icon-button--focused":this.hasFocus})}
        ?disabled=${O(e?void 0:this.disabled)}
        type=${O(e?void 0:"button")}
        href=${O(e?this.href:void 0)}
        target=${O(e?this.target:void 0)}
        download=${O(e?this.download:void 0)}
        rel=${O(e&&this.target?"noreferrer noopener":void 0)}
        role=${O(e?void 0:"button")}
        aria-disabled=${this.disabled?"true":"false"}
        aria-label="${this.label}"
        tabindex=${this.disabled?"-1":"0"}
        @blur=${this.handleBlur}
        @focus=${this.handleFocus}
        @click=${this.handleClick}
      >
        <sl-icon
          class="icon-button__icon"
          name=${O(this.name)}
          library=${O(this.library)}
          src=${O(this.src)}
          aria-hidden="true"
        ></sl-icon>
      </${t}>
    `}};wt.styles=[M,Kd];wt.dependencies={"sl-icon":it};n([T(".icon-button")],wt.prototype,"button",2);n([E()],wt.prototype,"hasFocus",2);n([d()],wt.prototype,"name",2);n([d()],wt.prototype,"library",2);n([d()],wt.prototype,"src",2);n([d()],wt.prototype,"href",2);n([d()],wt.prototype,"target",2);n([d()],wt.prototype,"download",2);n([d()],wt.prototype,"label",2);n([d({type:Boolean,reflect:!0})],wt.prototype,"disabled",2);var Ge=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.variant="neutral",this.size="medium",this.pill=!1,this.removable=!1}handleRemoveClick(){this.emit("sl-remove")}render(){return v`
      <span
        part="base"
        class=${L({tag:!0,"tag--primary":this.variant==="primary","tag--success":this.variant==="success","tag--neutral":this.variant==="neutral","tag--warning":this.variant==="warning","tag--danger":this.variant==="danger","tag--text":this.variant==="text","tag--small":this.size==="small","tag--medium":this.size==="medium","tag--large":this.size==="large","tag--pill":this.pill,"tag--removable":this.removable})}
      >
        <slot part="content" class="tag__content"></slot>

        ${this.removable?v`
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
    `}};Ge.styles=[M,qd];Ge.dependencies={"sl-icon-button":wt};n([d({reflect:!0})],Ge.prototype,"variant",2);n([d({reflect:!0})],Ge.prototype,"size",2);n([d({type:Boolean,reflect:!0})],Ge.prototype,"pill",2);n([d({type:Boolean})],Ge.prototype,"removable",2);Ge.define("sl-tag");var Xd=A`
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
`,Y=class extends P{constructor(){super(...arguments),this.formControlController=new Fe(this,{assumeInteractionOn:["sl-blur","sl-input"]}),this.hasSlotController=new Ht(this,"help-text","label"),this.hasFocus=!1,this.title="",this.name="",this.value="",this.size="medium",this.filled=!1,this.label="",this.helpText="",this.placeholder="",this.rows=4,this.resize="vertical",this.disabled=!1,this.readonly=!1,this.form="",this.required=!1,this.spellcheck=!0,this.defaultValue=""}get validity(){return this.input.validity}get validationMessage(){return this.input.validationMessage}connectedCallback(){super.connectedCallback(),this.resizeObserver=new ResizeObserver(()=>this.setTextareaHeight()),this.updateComplete.then(()=>{this.setTextareaHeight(),this.resizeObserver.observe(this.input)})}firstUpdated(){this.formControlController.updateValidity()}disconnectedCallback(){var e;super.disconnectedCallback(),this.input&&((e=this.resizeObserver)==null||e.unobserve(this.input))}handleBlur(){this.hasFocus=!1,this.emit("sl-blur")}handleChange(){this.value=this.input.value,this.setTextareaHeight(),this.emit("sl-change")}handleFocus(){this.hasFocus=!0,this.emit("sl-focus")}handleInput(){this.value=this.input.value,this.emit("sl-input")}handleInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}setTextareaHeight(){this.resize==="auto"?(this.sizeAdjuster.style.height=`${this.input.clientHeight}px`,this.input.style.height="auto",this.input.style.height=`${this.input.scrollHeight}px`):this.input.style.height=void 0}handleDisabledChange(){this.formControlController.setValidity(this.disabled)}handleRowsChange(){this.setTextareaHeight()}async handleValueChange(){await this.updateComplete,this.formControlController.updateValidity(),this.setTextareaHeight()}focus(e){this.input.focus(e)}blur(){this.input.blur()}select(){this.input.select()}scrollPosition(e){if(e){typeof e.top=="number"&&(this.input.scrollTop=e.top),typeof e.left=="number"&&(this.input.scrollLeft=e.left);return}return{top:this.input.scrollTop,left:this.input.scrollTop}}setSelectionRange(e,t,s="none"){this.input.setSelectionRange(e,t,s)}setRangeText(e,t,s,i="preserve"){const o=t??this.input.selectionStart,r=s??this.input.selectionEnd;this.input.setRangeText(e,o,r,i),this.value!==this.input.value&&(this.value=this.input.value,this.setTextareaHeight())}checkValidity(){return this.input.checkValidity()}getForm(){return this.formControlController.getForm()}reportValidity(){return this.input.reportValidity()}setCustomValidity(e){this.input.setCustomValidity(e),this.formControlController.updateValidity()}render(){const e=this.hasSlotController.test("label"),t=this.hasSlotController.test("help-text"),s=this.label?!0:!!e,i=this.helpText?!0:!!t;return v`
      <div
        part="form-control"
        class=${L({"form-control":!0,"form-control--small":this.size==="small","form-control--medium":this.size==="medium","form-control--large":this.size==="large","form-control--has-label":s,"form-control--has-help-text":i})}
      >
        <label
          part="form-control-label"
          class="form-control__label"
          for="input"
          aria-hidden=${s?"false":"true"}
        >
          <slot name="label">${this.label}</slot>
        </label>

        <div part="form-control-input" class="form-control-input">
          <div
            part="base"
            class=${L({textarea:!0,"textarea--small":this.size==="small","textarea--medium":this.size==="medium","textarea--large":this.size==="large","textarea--standard":!this.filled,"textarea--filled":this.filled,"textarea--disabled":this.disabled,"textarea--focused":this.hasFocus,"textarea--empty":!this.value,"textarea--resize-none":this.resize==="none","textarea--resize-vertical":this.resize==="vertical","textarea--resize-auto":this.resize==="auto"})}
          >
            <textarea
              part="textarea"
              id="input"
              class="textarea__control"
              title=${this.title}
              name=${O(this.name)}
              .value=${as(this.value)}
              ?disabled=${this.disabled}
              ?readonly=${this.readonly}
              ?required=${this.required}
              placeholder=${O(this.placeholder)}
              rows=${O(this.rows)}
              minlength=${O(this.minlength)}
              maxlength=${O(this.maxlength)}
              autocapitalize=${O(this.autocapitalize)}
              autocorrect=${O(this.autocorrect)}
              ?autofocus=${this.autofocus}
              spellcheck=${O(this.spellcheck)}
              enterkeyhint=${O(this.enterkeyhint)}
              inputmode=${O(this.inputmode)}
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
    `}};Y.styles=[M,ds,Xd];n([T(".textarea__control")],Y.prototype,"input",2);n([T(".textarea__size-adjuster")],Y.prototype,"sizeAdjuster",2);n([E()],Y.prototype,"hasFocus",2);n([d()],Y.prototype,"title",2);n([d()],Y.prototype,"name",2);n([d()],Y.prototype,"value",2);n([d({reflect:!0})],Y.prototype,"size",2);n([d({type:Boolean,reflect:!0})],Y.prototype,"filled",2);n([d()],Y.prototype,"label",2);n([d({attribute:"help-text"})],Y.prototype,"helpText",2);n([d()],Y.prototype,"placeholder",2);n([d({type:Number})],Y.prototype,"rows",2);n([d()],Y.prototype,"resize",2);n([d({type:Boolean,reflect:!0})],Y.prototype,"disabled",2);n([d({type:Boolean,reflect:!0})],Y.prototype,"readonly",2);n([d({reflect:!0})],Y.prototype,"form",2);n([d({type:Boolean,reflect:!0})],Y.prototype,"required",2);n([d({type:Number})],Y.prototype,"minlength",2);n([d({type:Number})],Y.prototype,"maxlength",2);n([d()],Y.prototype,"autocapitalize",2);n([d()],Y.prototype,"autocorrect",2);n([d()],Y.prototype,"autocomplete",2);n([d({type:Boolean})],Y.prototype,"autofocus",2);n([d()],Y.prototype,"enterkeyhint",2);n([d({type:Boolean,converter:{fromAttribute:e=>!(!e||e==="false"),toAttribute:e=>e?"true":"false"}})],Y.prototype,"spellcheck",2);n([d()],Y.prototype,"inputmode",2);n([cs()],Y.prototype,"defaultValue",2);n([C("disabled",{waitUntilFirstUpdate:!0})],Y.prototype,"handleDisabledChange",1);n([C("rows",{waitUntilFirstUpdate:!0})],Y.prototype,"handleRowsChange",1);n([C("value",{waitUntilFirstUpdate:!0})],Y.prototype,"handleValueChange",1);Y.define("sl-textarea");var Zd=A`
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
`,Qd=0,Ei=class extends P{constructor(){super(...arguments),this.attrId=++Qd,this.componentId=`sl-tab-panel-${this.attrId}`,this.name="",this.active=!1}connectedCallback(){super.connectedCallback(),this.id=this.id.length>0?this.id:this.componentId,this.setAttribute("role","tabpanel")}handleActiveChange(){this.setAttribute("aria-hidden",this.active?"false":"true")}render(){return v`
      <slot
        part="base"
        class=${L({"tab-panel":!0,"tab-panel--active":this.active})}
      ></slot>
    `}};Ei.styles=[M,Zd];n([d({reflect:!0})],Ei.prototype,"name",2);n([d({type:Boolean,reflect:!0})],Ei.prototype,"active",2);n([C("active")],Ei.prototype,"handleActiveChange",1);Ei.define("sl-tab-panel");var Jd=A`
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
`;function di(e,t){function s(o){const r=e.getBoundingClientRect(),a=e.ownerDocument.defaultView,l=r.left+a.scrollX,c=r.top+a.scrollY,h=o.pageX-l,u=o.pageY-c;t!=null&&t.onMove&&t.onMove(h,u)}function i(){document.removeEventListener("pointermove",s),document.removeEventListener("pointerup",i),t!=null&&t.onStop&&t.onStop()}document.addEventListener("pointermove",s,{passive:!0}),document.addEventListener("pointerup",i),(t==null?void 0:t.initialEvent)instanceof PointerEvent&&s(t.initialEvent)}var Qt=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.position=50,this.vertical=!1,this.disabled=!1,this.snapThreshold=12}connectedCallback(){super.connectedCallback(),this.resizeObserver=new ResizeObserver(e=>this.handleResize(e)),this.updateComplete.then(()=>this.resizeObserver.observe(this)),this.detectSize(),this.cachedPositionInPixels=this.percentageToPixels(this.position)}disconnectedCallback(){var e;super.disconnectedCallback(),(e=this.resizeObserver)==null||e.unobserve(this)}detectSize(){const{width:e,height:t}=this.getBoundingClientRect();this.size=this.vertical?t:e}percentageToPixels(e){return this.size*(e/100)}pixelsToPercentage(e){return e/this.size*100}handleDrag(e){const t=this.localize.dir()==="rtl";this.disabled||(e.cancelable&&e.preventDefault(),di(this,{onMove:(s,i)=>{let o=this.vertical?i:s;this.primary==="end"&&(o=this.size-o),this.snap&&this.snap.split(" ").forEach(a=>{let l;a.endsWith("%")?l=this.size*(parseFloat(a)/100):l=parseFloat(a),t&&!this.vertical&&(l=this.size-l),o>=l-this.snapThreshold&&o<=l+this.snapThreshold&&(o=l)}),this.position=gt(this.pixelsToPercentage(o),0,100)},initialEvent:e}))}handleKeyDown(e){if(!this.disabled&&["ArrowLeft","ArrowRight","ArrowUp","ArrowDown","Home","End"].includes(e.key)){let t=this.position;const s=(e.shiftKey?10:1)*(this.primary==="end"?-1:1);e.preventDefault(),(e.key==="ArrowLeft"&&!this.vertical||e.key==="ArrowUp"&&this.vertical)&&(t-=s),(e.key==="ArrowRight"&&!this.vertical||e.key==="ArrowDown"&&this.vertical)&&(t+=s),e.key==="Home"&&(t=this.primary==="end"?100:0),e.key==="End"&&(t=this.primary==="end"?0:100),this.position=gt(t,0,100)}}handleResize(e){const{width:t,height:s}=e[0].contentRect;this.size=this.vertical?s:t,(isNaN(this.cachedPositionInPixels)||this.position===1/0)&&(this.cachedPositionInPixels=Number(this.getAttribute("position-in-pixels")),this.positionInPixels=Number(this.getAttribute("position-in-pixels")),this.position=this.pixelsToPercentage(this.positionInPixels)),this.primary&&(this.position=this.pixelsToPercentage(this.cachedPositionInPixels))}handlePositionChange(){this.cachedPositionInPixels=this.percentageToPixels(this.position),this.positionInPixels=this.percentageToPixels(this.position),this.emit("sl-reposition")}handlePositionInPixelsChange(){this.position=this.pixelsToPercentage(this.positionInPixels)}handleVerticalChange(){this.detectSize()}render(){const e=this.vertical?"gridTemplateRows":"gridTemplateColumns",t=this.vertical?"gridTemplateColumns":"gridTemplateRows",s=this.localize.dir()==="rtl",i=`
      clamp(
        0%,
        clamp(
          var(--min),
          ${this.position}% - var(--divider-width) / 2,
          var(--max)
        ),
        calc(100% - var(--divider-width))
      )
    `,o="auto";return this.primary==="end"?s&&!this.vertical?this.style[e]=`${i} var(--divider-width) ${o}`:this.style[e]=`${o} var(--divider-width) ${i}`:s&&!this.vertical?this.style[e]=`${o} var(--divider-width) ${i}`:this.style[e]=`${i} var(--divider-width) ${o}`,this.style[t]="",v`
      <slot name="start" part="panel start" class="start"></slot>

      <div
        part="divider"
        class="divider"
        tabindex=${O(this.disabled?void 0:"0")}
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
    `}};Qt.styles=[M,Jd];n([T(".divider")],Qt.prototype,"divider",2);n([d({type:Number,reflect:!0})],Qt.prototype,"position",2);n([d({attribute:"position-in-pixels",type:Number})],Qt.prototype,"positionInPixels",2);n([d({type:Boolean,reflect:!0})],Qt.prototype,"vertical",2);n([d({type:Boolean,reflect:!0})],Qt.prototype,"disabled",2);n([d()],Qt.prototype,"primary",2);n([d()],Qt.prototype,"snap",2);n([d({type:Number,attribute:"snap-threshold"})],Qt.prototype,"snapThreshold",2);n([C("position")],Qt.prototype,"handlePositionChange",1);n([C("positionInPixels")],Qt.prototype,"handlePositionInPixelsChange",1);n([C("vertical")],Qt.prototype,"handleVerticalChange",1);Qt.define("sl-split-panel");var th=A`
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
`,eh=A`
  :host {
    display: contents;
  }
`,Oi=class extends P{constructor(){super(...arguments),this.observedElements=[],this.disabled=!1}connectedCallback(){super.connectedCallback(),this.resizeObserver=new ResizeObserver(e=>{this.emit("sl-resize",{detail:{entries:e}})}),this.disabled||this.startObserver()}disconnectedCallback(){super.disconnectedCallback(),this.stopObserver()}handleSlotChange(){this.disabled||this.startObserver()}startObserver(){const e=this.shadowRoot.querySelector("slot");if(e!==null){const t=e.assignedElements({flatten:!0});this.observedElements.forEach(s=>this.resizeObserver.unobserve(s)),this.observedElements=[],t.forEach(s=>{this.resizeObserver.observe(s),this.observedElements.push(s)})}}stopObserver(){this.resizeObserver.disconnect()}handleDisabledChange(){this.disabled?this.stopObserver():this.startObserver()}render(){return v` <slot @slotchange=${this.handleSlotChange}></slot> `}};Oi.styles=[M,eh];n([d({type:Boolean,reflect:!0})],Oi.prototype,"disabled",2);n([C("disabled",{waitUntilFirstUpdate:!0})],Oi.prototype,"handleDisabledChange",1);function sh(e,t){return{top:Math.round(e.getBoundingClientRect().top-t.getBoundingClientRect().top),left:Math.round(e.getBoundingClientRect().left-t.getBoundingClientRect().left)}}var yr=new Set;function ih(){const e=document.documentElement.clientWidth;return Math.abs(window.innerWidth-e)}function oh(){const e=Number(getComputedStyle(document.body).paddingRight.replace(/px/,""));return isNaN(e)||!e?0:e}function hi(e){if(yr.add(e),!document.documentElement.classList.contains("sl-scroll-lock")){const t=ih()+oh();let s=getComputedStyle(document.documentElement).scrollbarGutter;(!s||s==="auto")&&(s="stable"),t<2&&(s=""),document.documentElement.style.setProperty("--sl-scroll-lock-gutter",s),document.documentElement.classList.add("sl-scroll-lock"),document.documentElement.style.setProperty("--sl-scroll-lock-size",`${t}px`)}}function ui(e){yr.delete(e),yr.size===0&&(document.documentElement.classList.remove("sl-scroll-lock"),document.documentElement.style.removeProperty("--sl-scroll-lock-size"))}function wr(e,t,s="vertical",i="smooth"){const o=sh(e,t),r=o.top+t.scrollTop,a=o.left+t.scrollLeft,l=t.scrollLeft,c=t.scrollLeft+t.offsetWidth,h=t.scrollTop,u=t.scrollTop+t.offsetHeight;(s==="horizontal"||s==="both")&&(a<l?t.scrollTo({left:a,behavior:i}):a+e.clientWidth>c&&t.scrollTo({left:a-t.offsetWidth+e.clientWidth,behavior:i})),(s==="vertical"||s==="both")&&(r<h?t.scrollTo({top:r,behavior:i}):r+e.clientHeight>u&&t.scrollTo({top:r-t.offsetHeight+e.clientHeight,behavior:i}))}var zt=class extends P{constructor(){super(...arguments),this.tabs=[],this.focusableTabs=[],this.panels=[],this.localize=new K(this),this.hasScrollControls=!1,this.shouldHideScrollStartButton=!1,this.shouldHideScrollEndButton=!1,this.placement="top",this.activation="auto",this.noScrollControls=!1,this.fixedScrollControls=!1,this.scrollOffset=1}connectedCallback(){const e=Promise.all([customElements.whenDefined("sl-tab"),customElements.whenDefined("sl-tab-panel")]);super.connectedCallback(),this.resizeObserver=new ResizeObserver(()=>{this.repositionIndicator(),this.updateScrollControls()}),this.mutationObserver=new MutationObserver(t=>{t.some(s=>!["aria-labelledby","aria-controls"].includes(s.attributeName))&&setTimeout(()=>this.setAriaLabels()),t.some(s=>s.attributeName==="disabled")&&this.syncTabsAndPanels()}),this.updateComplete.then(()=>{this.syncTabsAndPanels(),this.mutationObserver.observe(this,{attributes:!0,childList:!0,subtree:!0}),this.resizeObserver.observe(this.nav),e.then(()=>{new IntersectionObserver((s,i)=>{var o;s[0].intersectionRatio>0&&(this.setAriaLabels(),this.setActiveTab((o=this.getActiveTab())!=null?o:this.tabs[0],{emitEvents:!1}),i.unobserve(s[0].target))}).observe(this.tabGroup)})})}disconnectedCallback(){var e,t;super.disconnectedCallback(),(e=this.mutationObserver)==null||e.disconnect(),this.nav&&((t=this.resizeObserver)==null||t.unobserve(this.nav))}getAllTabs(){return this.shadowRoot.querySelector('slot[name="nav"]').assignedElements()}getAllPanels(){return[...this.body.assignedElements()].filter(e=>e.tagName.toLowerCase()==="sl-tab-panel")}getActiveTab(){return this.tabs.find(e=>e.active)}handleClick(e){const s=e.target.closest("sl-tab");(s==null?void 0:s.closest("sl-tab-group"))===this&&s!==null&&this.setActiveTab(s,{scrollBehavior:"smooth"})}handleKeyDown(e){const s=e.target.closest("sl-tab");if((s==null?void 0:s.closest("sl-tab-group"))===this&&(["Enter"," "].includes(e.key)&&s!==null&&(this.setActiveTab(s,{scrollBehavior:"smooth"}),e.preventDefault()),["ArrowLeft","ArrowRight","ArrowUp","ArrowDown","Home","End"].includes(e.key))){const o=this.tabs.find(l=>l.matches(":focus")),r=this.localize.dir()==="rtl";let a=null;if((o==null?void 0:o.tagName.toLowerCase())==="sl-tab"){if(e.key==="Home")a=this.focusableTabs[0];else if(e.key==="End")a=this.focusableTabs[this.focusableTabs.length-1];else if(["top","bottom"].includes(this.placement)&&e.key===(r?"ArrowRight":"ArrowLeft")||["start","end"].includes(this.placement)&&e.key==="ArrowUp"){const l=this.tabs.findIndex(c=>c===o);a=this.findNextFocusableTab(l,"backward")}else if(["top","bottom"].includes(this.placement)&&e.key===(r?"ArrowLeft":"ArrowRight")||["start","end"].includes(this.placement)&&e.key==="ArrowDown"){const l=this.tabs.findIndex(c=>c===o);a=this.findNextFocusableTab(l,"forward")}if(!a)return;a.tabIndex=0,a.focus({preventScroll:!0}),this.activation==="auto"?this.setActiveTab(a,{scrollBehavior:"smooth"}):this.tabs.forEach(l=>{l.tabIndex=l===a?0:-1}),["top","bottom"].includes(this.placement)&&wr(a,this.nav,"horizontal"),e.preventDefault()}}}handleScrollToStart(){this.nav.scroll({left:this.localize.dir()==="rtl"?this.nav.scrollLeft+this.nav.clientWidth:this.nav.scrollLeft-this.nav.clientWidth,behavior:"smooth"})}handleScrollToEnd(){this.nav.scroll({left:this.localize.dir()==="rtl"?this.nav.scrollLeft-this.nav.clientWidth:this.nav.scrollLeft+this.nav.clientWidth,behavior:"smooth"})}setActiveTab(e,t){if(t=Ne({emitEvents:!0,scrollBehavior:"auto"},t),e!==this.activeTab&&!e.disabled){const s=this.activeTab;this.activeTab=e,this.tabs.forEach(i=>{i.active=i===this.activeTab,i.tabIndex=i===this.activeTab?0:-1}),this.panels.forEach(i=>{var o;return i.active=i.name===((o=this.activeTab)==null?void 0:o.panel)}),this.syncIndicator(),["top","bottom"].includes(this.placement)&&wr(this.activeTab,this.nav,"horizontal",t.scrollBehavior),t.emitEvents&&(s&&this.emit("sl-tab-hide",{detail:{name:s.panel}}),this.emit("sl-tab-show",{detail:{name:this.activeTab.panel}}))}}setAriaLabels(){this.tabs.forEach(e=>{const t=this.panels.find(s=>s.name===e.panel);t&&(e.setAttribute("aria-controls",t.getAttribute("id")),t.setAttribute("aria-labelledby",e.getAttribute("id")))})}repositionIndicator(){const e=this.getActiveTab();if(!e)return;const t=e.clientWidth,s=e.clientHeight,i=this.localize.dir()==="rtl",o=this.getAllTabs(),a=o.slice(0,o.indexOf(e)).reduce((l,c)=>({left:l.left+c.clientWidth,top:l.top+c.clientHeight}),{left:0,top:0});switch(this.placement){case"top":case"bottom":this.indicator.style.width=`${t}px`,this.indicator.style.height="auto",this.indicator.style.translate=i?`${-1*a.left}px`:`${a.left}px`;break;case"start":case"end":this.indicator.style.width="auto",this.indicator.style.height=`${s}px`,this.indicator.style.translate=`0 ${a.top}px`;break}}syncTabsAndPanels(){this.tabs=this.getAllTabs(),this.focusableTabs=this.tabs.filter(e=>!e.disabled),this.panels=this.getAllPanels(),this.syncIndicator(),this.updateComplete.then(()=>this.updateScrollControls())}findNextFocusableTab(e,t){let s=null;const i=t==="forward"?1:-1;let o=e+i;for(;e<this.tabs.length;){if(s=this.tabs[o]||null,s===null){t==="forward"?s=this.focusableTabs[0]:s=this.focusableTabs[this.focusableTabs.length-1];break}if(!s.disabled)break;o+=i}return s}updateScrollButtons(){this.hasScrollControls&&!this.fixedScrollControls&&(this.shouldHideScrollStartButton=this.scrollFromStart()<=this.scrollOffset,this.shouldHideScrollEndButton=this.isScrolledToEnd())}isScrolledToEnd(){return this.scrollFromStart()+this.nav.clientWidth>=this.nav.scrollWidth-this.scrollOffset}scrollFromStart(){return this.localize.dir()==="rtl"?-this.nav.scrollLeft:this.nav.scrollLeft}updateScrollControls(){this.noScrollControls?this.hasScrollControls=!1:this.hasScrollControls=["top","bottom"].includes(this.placement)&&this.nav.scrollWidth>this.nav.clientWidth+1,this.updateScrollButtons()}syncIndicator(){this.getActiveTab()?(this.indicator.style.display="block",this.repositionIndicator()):this.indicator.style.display="none"}show(e){const t=this.tabs.find(s=>s.panel===e);t&&this.setActiveTab(t,{scrollBehavior:"smooth"})}render(){const e=this.localize.dir()==="rtl";return v`
      <div
        part="base"
        class=${L({"tab-group":!0,"tab-group--top":this.placement==="top","tab-group--bottom":this.placement==="bottom","tab-group--start":this.placement==="start","tab-group--end":this.placement==="end","tab-group--rtl":this.localize.dir()==="rtl","tab-group--has-scroll-controls":this.hasScrollControls})}
        @click=${this.handleClick}
        @keydown=${this.handleKeyDown}
      >
        <div class="tab-group__nav-container" part="nav">
          ${this.hasScrollControls?v`
                <sl-icon-button
                  part="scroll-button scroll-button--start"
                  exportparts="base:scroll-button__base"
                  class=${L({"tab-group__scroll-button":!0,"tab-group__scroll-button--start":!0,"tab-group__scroll-button--start--hidden":this.shouldHideScrollStartButton})}
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

          ${this.hasScrollControls?v`
                <sl-icon-button
                  part="scroll-button scroll-button--end"
                  exportparts="base:scroll-button__base"
                  class=${L({"tab-group__scroll-button":!0,"tab-group__scroll-button--end":!0,"tab-group__scroll-button--end--hidden":this.shouldHideScrollEndButton})}
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
    `}};zt.styles=[M,th];zt.dependencies={"sl-icon-button":wt,"sl-resize-observer":Oi};n([T(".tab-group")],zt.prototype,"tabGroup",2);n([T(".tab-group__body")],zt.prototype,"body",2);n([T(".tab-group__nav")],zt.prototype,"nav",2);n([T(".tab-group__indicator")],zt.prototype,"indicator",2);n([E()],zt.prototype,"hasScrollControls",2);n([E()],zt.prototype,"shouldHideScrollStartButton",2);n([E()],zt.prototype,"shouldHideScrollEndButton",2);n([d()],zt.prototype,"placement",2);n([d()],zt.prototype,"activation",2);n([d({attribute:"no-scroll-controls",type:Boolean})],zt.prototype,"noScrollControls",2);n([d({attribute:"fixed-scroll-controls",type:Boolean})],zt.prototype,"fixedScrollControls",2);n([Ci({passive:!0})],zt.prototype,"updateScrollButtons",1);n([C("noScrollControls",{waitUntilFirstUpdate:!0})],zt.prototype,"updateScrollControls",1);n([C("placement",{waitUntilFirstUpdate:!0})],zt.prototype,"syncIndicator",1);zt.define("sl-tab-group");zi.define("sl-spinner");var rh=A`
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

  :host(:focus-visible):not([disabled]) {
    color: var(--sl-color-primary-600);
  }

  :host(:focus-visible) {
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
`,ah=0,fe=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.attrId=++ah,this.componentId=`sl-tab-${this.attrId}`,this.panel="",this.active=!1,this.closable=!1,this.disabled=!1,this.tabIndex=0}connectedCallback(){super.connectedCallback(),this.setAttribute("role","tab")}handleCloseClick(e){e.stopPropagation(),this.emit("sl-close")}handleActiveChange(){this.setAttribute("aria-selected",this.active?"true":"false")}handleDisabledChange(){this.setAttribute("aria-disabled",this.disabled?"true":"false"),this.disabled&&!this.active?this.tabIndex=-1:this.tabIndex=0}render(){return this.id=this.id.length>0?this.id:this.componentId,v`
      <div
        part="base"
        class=${L({tab:!0,"tab--active":this.active,"tab--closable":this.closable,"tab--disabled":this.disabled})}
      >
        <slot></slot>
        ${this.closable?v`
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
    `}};fe.styles=[M,rh];fe.dependencies={"sl-icon-button":wt};n([T(".tab")],fe.prototype,"tab",2);n([d({reflect:!0})],fe.prototype,"panel",2);n([d({type:Boolean,reflect:!0})],fe.prototype,"active",2);n([d({type:Boolean,reflect:!0})],fe.prototype,"closable",2);n([d({type:Boolean,reflect:!0})],fe.prototype,"disabled",2);n([d({type:Number,reflect:!0})],fe.prototype,"tabIndex",2);n([C("active")],fe.prototype,"handleActiveChange",1);n([C("disabled")],fe.prototype,"handleDisabledChange",1);fe.define("sl-tab");var nh=A`
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
`,It=class extends P{constructor(){super(...arguments),this.formControlController=new Fe(this,{value:e=>e.checked?e.value||"on":void 0,defaultValue:e=>e.defaultChecked,setValue:(e,t)=>e.checked=t}),this.hasSlotController=new Ht(this,"help-text"),this.hasFocus=!1,this.title="",this.name="",this.size="medium",this.disabled=!1,this.checked=!1,this.defaultChecked=!1,this.form="",this.required=!1,this.helpText=""}get validity(){return this.input.validity}get validationMessage(){return this.input.validationMessage}firstUpdated(){this.formControlController.updateValidity()}handleBlur(){this.hasFocus=!1,this.emit("sl-blur")}handleInput(){this.emit("sl-input")}handleInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}handleClick(){this.checked=!this.checked,this.emit("sl-change")}handleFocus(){this.hasFocus=!0,this.emit("sl-focus")}handleKeyDown(e){e.key==="ArrowLeft"&&(e.preventDefault(),this.checked=!1,this.emit("sl-change"),this.emit("sl-input")),e.key==="ArrowRight"&&(e.preventDefault(),this.checked=!0,this.emit("sl-change"),this.emit("sl-input"))}handleCheckedChange(){this.input.checked=this.checked,this.formControlController.updateValidity()}handleDisabledChange(){this.formControlController.setValidity(!0)}click(){this.input.click()}focus(e){this.input.focus(e)}blur(){this.input.blur()}checkValidity(){return this.input.checkValidity()}getForm(){return this.formControlController.getForm()}reportValidity(){return this.input.reportValidity()}setCustomValidity(e){this.input.setCustomValidity(e),this.formControlController.updateValidity()}render(){const e=this.hasSlotController.test("help-text"),t=this.helpText?!0:!!e;return v`
      <div
        class=${L({"form-control":!0,"form-control--small":this.size==="small","form-control--medium":this.size==="medium","form-control--large":this.size==="large","form-control--has-help-text":t})}
      >
        <label
          part="base"
          class=${L({switch:!0,"switch--checked":this.checked,"switch--disabled":this.disabled,"switch--focused":this.hasFocus,"switch--small":this.size==="small","switch--medium":this.size==="medium","switch--large":this.size==="large"})}
        >
          <input
            class="switch__input"
            type="checkbox"
            title=${this.title}
            name=${this.name}
            value=${O(this.value)}
            .checked=${as(this.checked)}
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
    `}};It.styles=[M,ds,nh];n([T('input[type="checkbox"]')],It.prototype,"input",2);n([E()],It.prototype,"hasFocus",2);n([d()],It.prototype,"title",2);n([d()],It.prototype,"name",2);n([d()],It.prototype,"value",2);n([d({reflect:!0})],It.prototype,"size",2);n([d({type:Boolean,reflect:!0})],It.prototype,"disabled",2);n([d({type:Boolean,reflect:!0})],It.prototype,"checked",2);n([cs("checked")],It.prototype,"defaultChecked",2);n([d({reflect:!0})],It.prototype,"form",2);n([d({type:Boolean,reflect:!0})],It.prototype,"required",2);n([d({attribute:"help-text"})],It.prototype,"helpText",2);n([C("checked",{waitUntilFirstUpdate:!0})],It.prototype,"handleCheckedChange",1);n([C("disabled",{waitUntilFirstUpdate:!0})],It.prototype,"handleDisabledChange",1);It.define("sl-switch");Oi.define("sl-resize-observer");var lh=A`
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

  .select--small.select--multiple .select__prefix::slotted(*) {
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

  .select--medium.select--multiple .select__prefix::slotted(*) {
    margin-inline-start: var(--sl-input-spacing-medium);
  }

  .select--medium.select--multiple .select__combobox {
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

  .select--large.select--multiple .select__prefix::slotted(*) {
    margin-inline-start: var(--sl-input-spacing-large);
  }

  .select--large.select--multiple .select__combobox {
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
 */let xr=class extends Ti{constructor(t){if(super(t),this.it=G,t.type!==Ce.CHILD)throw Error(this.constructor.directiveName+"() can only be used in child bindings")}render(t){if(t===G||t==null)return this._t=void 0,this.it=t;if(t===Nt)return t;if(typeof t!="string")throw Error(this.constructor.directiveName+"() called with a non-string value");if(t===this.it)return this._t;this.it=t;const s=[t];return s.raw=s,this._t={_$litType$:this.constructor.resultType,strings:s,values:[]}}};xr.directiveName="unsafeHTML",xr.resultType=1;const pi=Ls(xr);var j=class extends P{constructor(){super(...arguments),this.formControlController=new Fe(this,{assumeInteractionOn:["sl-blur","sl-input"]}),this.hasSlotController=new Ht(this,"help-text","label"),this.localize=new K(this),this.typeToSelectString="",this.hasFocus=!1,this.displayLabel="",this.selectedOptions=[],this.valueHasChanged=!1,this.name="",this.value="",this.defaultValue="",this.size="medium",this.placeholder="",this.multiple=!1,this.maxOptionsVisible=3,this.disabled=!1,this.clearable=!1,this.open=!1,this.hoist=!1,this.filled=!1,this.pill=!1,this.label="",this.placement="bottom",this.helpText="",this.form="",this.required=!1,this.getTag=e=>v`
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
    `,this.handleDocumentFocusIn=e=>{const t=e.composedPath();this&&!t.includes(this)&&this.hide()},this.handleDocumentKeyDown=e=>{const t=e.target,s=t.closest(".select__clear")!==null,i=t.closest("sl-icon-button")!==null;if(!(s||i)){if(e.key==="Escape"&&this.open&&!this.closeWatcher&&(e.preventDefault(),e.stopPropagation(),this.hide(),this.displayInput.focus({preventScroll:!0})),e.key==="Enter"||e.key===" "&&this.typeToSelectString===""){if(e.preventDefault(),e.stopImmediatePropagation(),!this.open){this.show();return}this.currentOption&&!this.currentOption.disabled&&(this.valueHasChanged=!0,this.multiple?this.toggleOptionSelection(this.currentOption):this.setSelectedOptions(this.currentOption),this.updateComplete.then(()=>{this.emit("sl-input"),this.emit("sl-change")}),this.multiple||(this.hide(),this.displayInput.focus({preventScroll:!0})));return}if(["ArrowUp","ArrowDown","Home","End"].includes(e.key)){const o=this.getAllOptions(),r=o.indexOf(this.currentOption);let a=Math.max(0,r);if(e.preventDefault(),!this.open&&(this.show(),this.currentOption))return;e.key==="ArrowDown"?(a=r+1,a>o.length-1&&(a=0)):e.key==="ArrowUp"?(a=r-1,a<0&&(a=o.length-1)):e.key==="Home"?a=0:e.key==="End"&&(a=o.length-1),this.setCurrentOption(o[a])}if(e.key&&e.key.length===1||e.key==="Backspace"){const o=this.getAllOptions();if(e.metaKey||e.ctrlKey||e.altKey)return;if(!this.open){if(e.key==="Backspace")return;this.show()}e.stopPropagation(),e.preventDefault(),clearTimeout(this.typeToSelectTimeout),this.typeToSelectTimeout=window.setTimeout(()=>this.typeToSelectString="",1e3),e.key==="Backspace"?this.typeToSelectString=this.typeToSelectString.slice(0,-1):this.typeToSelectString+=e.key.toLowerCase();for(const r of o)if(r.getTextLabel().toLowerCase().startsWith(this.typeToSelectString)){this.setCurrentOption(r);break}}}},this.handleDocumentMouseDown=e=>{const t=e.composedPath();this&&!t.includes(this)&&this.hide()}}get validity(){return this.valueInput.validity}get validationMessage(){return this.valueInput.validationMessage}connectedCallback(){super.connectedCallback(),setTimeout(()=>{this.handleDefaultSlotChange()}),this.open=!1}addOpenListeners(){var e;document.addEventListener("focusin",this.handleDocumentFocusIn),document.addEventListener("keydown",this.handleDocumentKeyDown),document.addEventListener("mousedown",this.handleDocumentMouseDown),this.getRootNode()!==document&&this.getRootNode().addEventListener("focusin",this.handleDocumentFocusIn),"CloseWatcher"in window&&((e=this.closeWatcher)==null||e.destroy(),this.closeWatcher=new CloseWatcher,this.closeWatcher.onclose=()=>{this.open&&(this.hide(),this.displayInput.focus({preventScroll:!0}))})}removeOpenListeners(){var e;document.removeEventListener("focusin",this.handleDocumentFocusIn),document.removeEventListener("keydown",this.handleDocumentKeyDown),document.removeEventListener("mousedown",this.handleDocumentMouseDown),this.getRootNode()!==document&&this.getRootNode().removeEventListener("focusin",this.handleDocumentFocusIn),(e=this.closeWatcher)==null||e.destroy()}handleFocus(){this.hasFocus=!0,this.displayInput.setSelectionRange(0,0),this.emit("sl-focus")}handleBlur(){this.hasFocus=!1,this.emit("sl-blur")}handleLabelClick(){this.displayInput.focus()}handleComboboxMouseDown(e){const s=e.composedPath().some(i=>i instanceof Element&&i.tagName.toLowerCase()==="sl-icon-button");this.disabled||s||(e.preventDefault(),this.displayInput.focus({preventScroll:!0}),this.open=!this.open)}handleComboboxKeyDown(e){e.key!=="Tab"&&(e.stopPropagation(),this.handleDocumentKeyDown(e))}handleClearClick(e){e.stopPropagation(),this.value!==""&&(this.setSelectedOptions([]),this.displayInput.focus({preventScroll:!0}),this.updateComplete.then(()=>{this.emit("sl-clear"),this.emit("sl-input"),this.emit("sl-change")}))}handleClearMouseDown(e){e.stopPropagation(),e.preventDefault()}handleOptionClick(e){const s=e.target.closest("sl-option"),i=this.value;s&&!s.disabled&&(this.valueHasChanged=!0,this.multiple?this.toggleOptionSelection(s):this.setSelectedOptions(s),this.updateComplete.then(()=>this.displayInput.focus({preventScroll:!0})),this.value!==i&&this.updateComplete.then(()=>{this.emit("sl-input"),this.emit("sl-change")}),this.multiple||(this.hide(),this.displayInput.focus({preventScroll:!0})))}handleDefaultSlotChange(){customElements.get("wa-option")||customElements.whenDefined("wa-option").then(()=>this.handleDefaultSlotChange());const e=this.getAllOptions(),t=this.valueHasChanged?this.value:this.defaultValue,s=Array.isArray(t)?t:[t],i=[];e.forEach(o=>i.push(o.value)),this.setSelectedOptions(e.filter(o=>s.includes(o.value)))}handleTagRemove(e,t){e.stopPropagation(),this.disabled||(this.toggleOptionSelection(t,!1),this.updateComplete.then(()=>{this.emit("sl-input"),this.emit("sl-change")}))}getAllOptions(){return[...this.querySelectorAll("sl-option")]}getFirstOption(){return this.querySelector("sl-option")}setCurrentOption(e){this.getAllOptions().forEach(s=>{s.current=!1,s.tabIndex=-1}),e&&(this.currentOption=e,e.current=!0,e.tabIndex=0,e.focus())}setSelectedOptions(e){const t=this.getAllOptions(),s=Array.isArray(e)?e:[e];t.forEach(i=>i.selected=!1),s.length&&s.forEach(i=>i.selected=!0),this.selectionChanged()}toggleOptionSelection(e,t){t===!0||t===!1?e.selected=t:e.selected=!e.selected,this.selectionChanged()}selectionChanged(){var e,t,s;const i=this.getAllOptions();if(this.selectedOptions=i.filter(o=>o.selected),this.multiple)this.value=this.selectedOptions.map(o=>o.value),this.placeholder&&this.value.length===0?this.displayLabel="":this.displayLabel=this.localize.term("numOptionsSelected",this.selectedOptions.length);else{const o=this.selectedOptions[0];this.value=(e=o==null?void 0:o.value)!=null?e:"",this.displayLabel=(s=(t=o==null?void 0:o.getTextLabel)==null?void 0:t.call(o))!=null?s:""}this.updateComplete.then(()=>{this.formControlController.updateValidity()})}get tags(){return this.selectedOptions.map((e,t)=>{if(t<this.maxOptionsVisible||this.maxOptionsVisible<=0){const s=this.getTag(e,t);return v`<div @sl-remove=${i=>this.handleTagRemove(i,e)}>
          ${typeof s=="string"?pi(s):s}
        </div>`}else if(t===this.maxOptionsVisible)return v`<sl-tag size=${this.size}>+${this.selectedOptions.length-t}</sl-tag>`;return v``})}handleInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}handleDisabledChange(){this.disabled&&(this.open=!1,this.handleOpenChange())}handleValueChange(){const e=this.getAllOptions(),t=Array.isArray(this.value)?this.value:[this.value];this.setSelectedOptions(e.filter(s=>t.includes(s.value)))}async handleOpenChange(){if(this.open&&!this.disabled){this.setCurrentOption(this.selectedOptions[0]||this.getFirstOption()),this.emit("sl-show"),this.addOpenListeners(),await bt(this),this.listbox.hidden=!1,this.popup.active=!0,requestAnimationFrame(()=>{this.setCurrentOption(this.currentOption)});const{keyframes:e,options:t}=nt(this,"select.show",{dir:this.localize.dir()});await ht(this.popup.popup,e,t),this.currentOption&&wr(this.currentOption,this.listbox,"vertical","auto"),this.emit("sl-after-show")}else{this.emit("sl-hide"),this.removeOpenListeners(),await bt(this);const{keyframes:e,options:t}=nt(this,"select.hide",{dir:this.localize.dir()});await ht(this.popup.popup,e,t),this.listbox.hidden=!0,this.popup.active=!1,this.emit("sl-after-hide")}}async show(){if(this.open||this.disabled){this.open=!1;return}return this.open=!0,Bt(this,"sl-after-show")}async hide(){if(!this.open||this.disabled){this.open=!1;return}return this.open=!1,Bt(this,"sl-after-hide")}checkValidity(){return this.valueInput.checkValidity()}getForm(){return this.formControlController.getForm()}reportValidity(){return this.valueInput.reportValidity()}setCustomValidity(e){this.valueInput.setCustomValidity(e),this.formControlController.updateValidity()}focus(e){this.displayInput.focus(e)}blur(){this.displayInput.blur()}render(){const e=this.hasSlotController.test("label"),t=this.hasSlotController.test("help-text"),s=this.label?!0:!!e,i=this.helpText?!0:!!t,o=this.clearable&&!this.disabled&&this.value.length>0,r=this.placeholder&&this.value&&this.value.length<=0;return v`
      <div
        part="form-control"
        class=${L({"form-control":!0,"form-control--small":this.size==="small","form-control--medium":this.size==="medium","form-control--large":this.size==="large","form-control--has-label":s,"form-control--has-help-text":i})}
      >
        <label
          id="label"
          part="form-control-label"
          class="form-control__label"
          aria-hidden=${s?"false":"true"}
          @click=${this.handleLabelClick}
        >
          <slot name="label">${this.label}</slot>
        </label>

        <div part="form-control-input" class="form-control-input">
          <sl-popup
            class=${L({select:!0,"select--standard":!0,"select--filled":this.filled,"select--pill":this.pill,"select--open":this.open,"select--disabled":this.disabled,"select--multiple":this.multiple,"select--focused":this.hasFocus,"select--placeholder-visible":r,"select--top":this.placement==="top","select--bottom":this.placement==="bottom","select--small":this.size==="small","select--medium":this.size==="medium","select--large":this.size==="large"})}
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

              ${this.multiple?v`<div part="tags" class="select__tags">${this.tags}</div>`:""}

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

              ${o?v`
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
    `}};j.styles=[M,ds,lh];j.dependencies={"sl-icon":it,"sl-popup":X,"sl-tag":Ge};n([T(".select")],j.prototype,"popup",2);n([T(".select__combobox")],j.prototype,"combobox",2);n([T(".select__display-input")],j.prototype,"displayInput",2);n([T(".select__value-input")],j.prototype,"valueInput",2);n([T(".select__listbox")],j.prototype,"listbox",2);n([E()],j.prototype,"hasFocus",2);n([E()],j.prototype,"displayLabel",2);n([E()],j.prototype,"currentOption",2);n([E()],j.prototype,"selectedOptions",2);n([E()],j.prototype,"valueHasChanged",2);n([d()],j.prototype,"name",2);n([d({converter:{fromAttribute:e=>e.split(" "),toAttribute:e=>e.join(" ")}})],j.prototype,"value",2);n([cs()],j.prototype,"defaultValue",2);n([d({reflect:!0})],j.prototype,"size",2);n([d()],j.prototype,"placeholder",2);n([d({type:Boolean,reflect:!0})],j.prototype,"multiple",2);n([d({attribute:"max-options-visible",type:Number})],j.prototype,"maxOptionsVisible",2);n([d({type:Boolean,reflect:!0})],j.prototype,"disabled",2);n([d({type:Boolean})],j.prototype,"clearable",2);n([d({type:Boolean,reflect:!0})],j.prototype,"open",2);n([d({type:Boolean})],j.prototype,"hoist",2);n([d({type:Boolean,reflect:!0})],j.prototype,"filled",2);n([d({type:Boolean,reflect:!0})],j.prototype,"pill",2);n([d()],j.prototype,"label",2);n([d({reflect:!0})],j.prototype,"placement",2);n([d({attribute:"help-text"})],j.prototype,"helpText",2);n([d({reflect:!0})],j.prototype,"form",2);n([d({type:Boolean,reflect:!0})],j.prototype,"required",2);n([d()],j.prototype,"getTag",2);n([C("disabled",{waitUntilFirstUpdate:!0})],j.prototype,"handleDisabledChange",1);n([C("value",{waitUntilFirstUpdate:!0})],j.prototype,"handleValueChange",1);n([C("open",{waitUntilFirstUpdate:!0})],j.prototype,"handleOpenChange",1);Z("select.show",{keyframes:[{opacity:0,scale:.9},{opacity:1,scale:1}],options:{duration:100,easing:"ease"}});Z("select.hide",{keyframes:[{opacity:1,scale:1},{opacity:0,scale:.9}],options:{duration:100,easing:"ease"}});j.define("sl-select");var ch=A`
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
`,jr=class extends P{constructor(){super(...arguments),this.effect="none"}render(){return v`
      <div
        part="base"
        class=${L({skeleton:!0,"skeleton--pulse":this.effect==="pulse","skeleton--sheen":this.effect==="sheen"})}
      >
        <div part="indicator" class="skeleton__indicator"></div>
      </div>
    `}};jr.styles=[M,ch];n([d()],jr.prototype,"effect",2);jr.define("sl-skeleton");var dh=A`
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
 */const il="important",hh=" !"+il,jt=Ls(class extends Ti{constructor(e){var t;if(super(e),e.type!==Ce.ATTRIBUTE||e.name!=="style"||((t=e.strings)==null?void 0:t.length)>2)throw Error("The `styleMap` directive must be used in the `style` attribute and must be the only part in the attribute.")}render(e){return Object.keys(e).reduce((t,s)=>{const i=e[s];return i==null?t:t+`${s=s.includes("-")?s:s.replace(/(?:^(webkit|moz|ms|o)|)(?=[A-Z])/g,"-$&").toLowerCase()}:${i};`},"")}update(e,[t]){const{style:s}=e.element;if(this.ft===void 0)return this.ft=new Set(Object.keys(t)),this.render(t);for(const i of this.ft)t[i]==null&&(this.ft.delete(i),i.includes("-")?s.removeProperty(i):s[i]=null);for(const i in t){const o=t[i];if(o!=null){this.ft.add(i);const r=typeof o=="string"&&o.endsWith(hh);i.includes("-")||r?s.setProperty(i,r?o.slice(0,-11):o,r?il:""):s[i]=o}}return Nt}});var Rt=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.hoverValue=0,this.isHovering=!1,this.label="",this.value=0,this.max=5,this.precision=1,this.readonly=!1,this.disabled=!1,this.getSymbol=()=>'<sl-icon name="star-fill" library="system"></sl-icon>'}getValueFromMousePosition(e){return this.getValueFromXCoordinate(e.clientX)}getValueFromTouchPosition(e){return this.getValueFromXCoordinate(e.touches[0].clientX)}getValueFromXCoordinate(e){const t=this.localize.dir()==="rtl",{left:s,right:i,width:o}=this.rating.getBoundingClientRect(),r=t?this.roundToPrecision((i-e)/o*this.max,this.precision):this.roundToPrecision((e-s)/o*this.max,this.precision);return gt(r,0,this.max)}handleClick(e){this.disabled||(this.setValue(this.getValueFromMousePosition(e)),this.emit("sl-change"))}setValue(e){this.disabled||this.readonly||(this.value=e===this.value?0:e,this.isHovering=!1)}handleKeyDown(e){const t=this.localize.dir()==="ltr",s=this.localize.dir()==="rtl",i=this.value;if(!(this.disabled||this.readonly)){if(e.key==="ArrowDown"||t&&e.key==="ArrowLeft"||s&&e.key==="ArrowRight"){const o=e.shiftKey?1:this.precision;this.value=Math.max(0,this.value-o),e.preventDefault()}if(e.key==="ArrowUp"||t&&e.key==="ArrowRight"||s&&e.key==="ArrowLeft"){const o=e.shiftKey?1:this.precision;this.value=Math.min(this.max,this.value+o),e.preventDefault()}e.key==="Home"&&(this.value=0,e.preventDefault()),e.key==="End"&&(this.value=this.max,e.preventDefault()),this.value!==i&&this.emit("sl-change")}}handleMouseEnter(e){this.isHovering=!0,this.hoverValue=this.getValueFromMousePosition(e)}handleMouseMove(e){this.hoverValue=this.getValueFromMousePosition(e)}handleMouseLeave(){this.isHovering=!1}handleTouchStart(e){this.isHovering=!0,this.hoverValue=this.getValueFromTouchPosition(e),e.preventDefault()}handleTouchMove(e){this.hoverValue=this.getValueFromTouchPosition(e)}handleTouchEnd(e){this.isHovering=!1,this.setValue(this.hoverValue),this.emit("sl-change"),e.preventDefault()}roundToPrecision(e,t=.5){const s=1/t;return Math.ceil(e*s)/s}handleHoverValueChange(){this.emit("sl-hover",{detail:{phase:"move",value:this.hoverValue}})}handleIsHoveringChange(){this.emit("sl-hover",{detail:{phase:this.isHovering?"start":"end",value:this.hoverValue}})}focus(e){this.rating.focus(e)}blur(){this.rating.blur()}render(){const e=this.localize.dir()==="rtl",t=Array.from(Array(this.max).keys());let s=0;return this.disabled||this.readonly?s=this.value:s=this.isHovering?this.hoverValue:this.value,v`
      <div
        part="base"
        class=${L({rating:!0,"rating--readonly":this.readonly,"rating--disabled":this.disabled,"rating--rtl":e})}
        role="slider"
        aria-label=${this.label}
        aria-disabled=${this.disabled?"true":"false"}
        aria-readonly=${this.readonly?"true":"false"}
        aria-valuenow=${this.value}
        aria-valuemin=${0}
        aria-valuemax=${this.max}
        tabindex=${this.disabled?"-1":"0"}
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
          ${t.map(i=>s>i&&s<i+1?v`
                <span
                  class=${L({rating__symbol:!0,"rating__partial-symbol-container":!0,"rating__symbol--hover":this.isHovering&&Math.ceil(s)===i+1})}
                  role="presentation"
                >
                  <div
                    style=${jt({clipPath:e?`inset(0 ${(s-i)*100}% 0 0)`:`inset(0 0 0 ${(s-i)*100}%)`})}
                  >
                    ${pi(this.getSymbol(i+1))}
                  </div>
                  <div
                    class="rating__partial--filled"
                    style=${jt({clipPath:e?`inset(0 0 0 ${100-(s-i)*100}%)`:`inset(0 ${100-(s-i)*100}% 0 0)`})}
                  >
                    ${pi(this.getSymbol(i+1))}
                  </div>
                </span>
              `:v`
              <span
                class=${L({rating__symbol:!0,"rating__symbol--hover":this.isHovering&&Math.ceil(s)===i+1,"rating__symbol--active":s>=i+1})}
                role="presentation"
              >
                ${pi(this.getSymbol(i+1))}
              </span>
            `)}
        </span>
      </div>
    `}};Rt.styles=[M,dh];Rt.dependencies={"sl-icon":it};n([T(".rating")],Rt.prototype,"rating",2);n([E()],Rt.prototype,"hoverValue",2);n([E()],Rt.prototype,"isHovering",2);n([d()],Rt.prototype,"label",2);n([d({type:Number})],Rt.prototype,"value",2);n([d({type:Number})],Rt.prototype,"max",2);n([d({type:Number})],Rt.prototype,"precision",2);n([d({type:Boolean,reflect:!0})],Rt.prototype,"readonly",2);n([d({type:Boolean,reflect:!0})],Rt.prototype,"disabled",2);n([d()],Rt.prototype,"getSymbol",2);n([Ci({passive:!0})],Rt.prototype,"handleTouchMove",1);n([C("hoverValue")],Rt.prototype,"handleHoverValueChange",1);n([C("isHovering")],Rt.prototype,"handleIsHoveringChange",1);Rt.define("sl-rating");var uh=[{max:276e4,value:6e4,unit:"minute"},{max:72e6,value:36e5,unit:"hour"},{max:5184e5,value:864e5,unit:"day"},{max:24192e5,value:6048e5,unit:"week"},{max:28512e6,value:2592e6,unit:"month"},{max:1/0,value:31536e6,unit:"year"}],us=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.isoTime="",this.relativeTime="",this.date=new Date,this.format="long",this.numeric="auto",this.sync=!1}disconnectedCallback(){super.disconnectedCallback(),clearTimeout(this.updateTimeout)}render(){const e=new Date,t=new Date(this.date);if(isNaN(t.getMilliseconds()))return this.relativeTime="",this.isoTime="","";const s=t.getTime()-e.getTime(),{unit:i,value:o}=uh.find(r=>Math.abs(s)<r.max);if(this.isoTime=t.toISOString(),this.relativeTime=this.localize.relativeTime(Math.round(s/o),i,{numeric:this.numeric,style:this.format}),clearTimeout(this.updateTimeout),this.sync){let r;i==="minute"?r=Ki("second"):i==="hour"?r=Ki("minute"):i==="day"?r=Ki("hour"):r=Ki("day"),this.updateTimeout=window.setTimeout(()=>this.requestUpdate(),r)}return v` <time datetime=${this.isoTime}>${this.relativeTime}</time> `}};n([E()],us.prototype,"isoTime",2);n([E()],us.prototype,"relativeTime",2);n([d()],us.prototype,"date",2);n([d()],us.prototype,"format",2);n([d()],us.prototype,"numeric",2);n([d({type:Boolean})],us.prototype,"sync",2);function Ki(e){const s={second:1e3,minute:6e4,hour:36e5,day:864e5}[e];return s-Date.now()%s}us.define("sl-relative-time");var ph=A`
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
`,ct=class extends P{constructor(){super(...arguments),this.formControlController=new Fe(this),this.hasSlotController=new Ht(this,"help-text","label"),this.localize=new K(this),this.hasFocus=!1,this.hasTooltip=!1,this.title="",this.name="",this.value=0,this.label="",this.helpText="",this.disabled=!1,this.min=0,this.max=100,this.step=1,this.tooltip="top",this.tooltipFormatter=e=>e.toString(),this.form="",this.defaultValue=0}get validity(){return this.input.validity}get validationMessage(){return this.input.validationMessage}connectedCallback(){super.connectedCallback(),this.resizeObserver=new ResizeObserver(()=>this.syncRange()),this.value<this.min&&(this.value=this.min),this.value>this.max&&(this.value=this.max),this.updateComplete.then(()=>{this.syncRange(),this.resizeObserver.observe(this.input)})}disconnectedCallback(){var e;super.disconnectedCallback(),(e=this.resizeObserver)==null||e.unobserve(this.input)}handleChange(){this.emit("sl-change")}handleInput(){this.value=parseFloat(this.input.value),this.emit("sl-input"),this.syncRange()}handleBlur(){this.hasFocus=!1,this.hasTooltip=!1,this.emit("sl-blur")}handleFocus(){this.hasFocus=!0,this.hasTooltip=!0,this.emit("sl-focus")}handleThumbDragStart(){this.hasTooltip=!0}handleThumbDragEnd(){this.hasTooltip=!1}syncProgress(e){this.input.style.setProperty("--percent",`${e*100}%`)}syncTooltip(e){if(this.output!==null){const t=this.input.offsetWidth,s=this.output.offsetWidth,i=getComputedStyle(this.input).getPropertyValue("--thumb-size"),o=this.localize.dir()==="rtl",r=t*e;if(o){const a=`${t-r}px + ${e} * ${i}`;this.output.style.translate=`calc((${a} - ${s/2}px - ${i} / 2))`}else{const a=`${r}px - ${e} * ${i}`;this.output.style.translate=`calc(${a} - ${s/2}px + ${i} / 2)`}}}handleValueChange(){this.formControlController.updateValidity(),this.input.value=this.value.toString(),this.value=parseFloat(this.input.value),this.syncRange()}handleDisabledChange(){this.formControlController.setValidity(this.disabled)}syncRange(){const e=Math.max(0,(this.value-this.min)/(this.max-this.min));this.syncProgress(e),this.tooltip!=="none"&&this.updateComplete.then(()=>this.syncTooltip(e))}handleInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}focus(e){this.input.focus(e)}blur(){this.input.blur()}stepUp(){this.input.stepUp(),this.value!==Number(this.input.value)&&(this.value=Number(this.input.value))}stepDown(){this.input.stepDown(),this.value!==Number(this.input.value)&&(this.value=Number(this.input.value))}checkValidity(){return this.input.checkValidity()}getForm(){return this.formControlController.getForm()}reportValidity(){return this.input.reportValidity()}setCustomValidity(e){this.input.setCustomValidity(e),this.formControlController.updateValidity()}render(){const e=this.hasSlotController.test("label"),t=this.hasSlotController.test("help-text"),s=this.label?!0:!!e,i=this.helpText?!0:!!t;return v`
      <div
        part="form-control"
        class=${L({"form-control":!0,"form-control--medium":!0,"form-control--has-label":s,"form-control--has-help-text":i})}
      >
        <label
          part="form-control-label"
          class="form-control__label"
          for="input"
          aria-hidden=${s?"false":"true"}
        >
          <slot name="label">${this.label}</slot>
        </label>

        <div part="form-control-input" class="form-control-input">
          <div
            part="base"
            class=${L({range:!0,"range--disabled":this.disabled,"range--focused":this.hasFocus,"range--rtl":this.localize.dir()==="rtl","range--tooltip-visible":this.hasTooltip,"range--tooltip-top":this.tooltip==="top","range--tooltip-bottom":this.tooltip==="bottom"})}
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
              name=${O(this.name)}
              ?disabled=${this.disabled}
              min=${O(this.min)}
              max=${O(this.max)}
              step=${O(this.step)}
              .value=${as(this.value.toString())}
              aria-describedby="help-text"
              @change=${this.handleChange}
              @focus=${this.handleFocus}
              @input=${this.handleInput}
              @invalid=${this.handleInvalid}
              @blur=${this.handleBlur}
            />
            ${this.tooltip!=="none"&&!this.disabled?v`
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
    `}};ct.styles=[M,ds,ph];n([T(".range__control")],ct.prototype,"input",2);n([T(".range__tooltip")],ct.prototype,"output",2);n([E()],ct.prototype,"hasFocus",2);n([E()],ct.prototype,"hasTooltip",2);n([d()],ct.prototype,"title",2);n([d()],ct.prototype,"name",2);n([d({type:Number})],ct.prototype,"value",2);n([d()],ct.prototype,"label",2);n([d({attribute:"help-text"})],ct.prototype,"helpText",2);n([d({type:Boolean,reflect:!0})],ct.prototype,"disabled",2);n([d({type:Number})],ct.prototype,"min",2);n([d({type:Number})],ct.prototype,"max",2);n([d({type:Number})],ct.prototype,"step",2);n([d()],ct.prototype,"tooltip",2);n([d({attribute:!1})],ct.prototype,"tooltipFormatter",2);n([d({reflect:!0})],ct.prototype,"form",2);n([cs()],ct.prototype,"defaultValue",2);n([Ci({passive:!0})],ct.prototype,"handleThumbDragStart",1);n([C("value",{waitUntilFirstUpdate:!0})],ct.prototype,"handleValueChange",1);n([C("disabled",{waitUntilFirstUpdate:!0})],ct.prototype,"handleDisabledChange",1);n([C("hasTooltip",{waitUntilFirstUpdate:!0})],ct.prototype,"syncRange",1);ct.define("sl-range");var ol=A`
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
`,fh=A`
  ${ol}

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
`,me=class extends P{constructor(){super(...arguments),this.hasSlotController=new Ht(this,"[default]","prefix","suffix"),this.hasFocus=!1,this.checked=!1,this.disabled=!1,this.size="medium",this.pill=!1}connectedCallback(){super.connectedCallback(),this.setAttribute("role","presentation")}handleBlur(){this.hasFocus=!1,this.emit("sl-blur")}handleClick(e){if(this.disabled){e.preventDefault(),e.stopPropagation();return}this.checked=!0}handleFocus(){this.hasFocus=!0,this.emit("sl-focus")}handleDisabledChange(){this.setAttribute("aria-disabled",this.disabled?"true":"false")}focus(e){this.input.focus(e)}blur(){this.input.blur()}render(){return ci`
      <div part="base" role="presentation">
        <button
          part="${`button${this.checked?" button--checked":""}`}"
          role="radio"
          aria-checked="${this.checked}"
          class=${L({button:!0,"button--default":!0,"button--small":this.size==="small","button--medium":this.size==="medium","button--large":this.size==="large","button--checked":this.checked,"button--disabled":this.disabled,"button--focused":this.hasFocus,"button--outline":!0,"button--pill":this.pill,"button--has-label":this.hasSlotController.test("[default]"),"button--has-prefix":this.hasSlotController.test("prefix"),"button--has-suffix":this.hasSlotController.test("suffix")})}
          aria-disabled=${this.disabled}
          type="button"
          value=${O(this.value)}
          @blur=${this.handleBlur}
          @focus=${this.handleFocus}
          @click=${this.handleClick}
        >
          <slot name="prefix" part="prefix" class="button__prefix"></slot>
          <slot part="label" class="button__label"></slot>
          <slot name="suffix" part="suffix" class="button__suffix"></slot>
        </button>
      </div>
    `}};me.styles=[M,fh];n([T(".button")],me.prototype,"input",2);n([T(".hidden-input")],me.prototype,"hiddenInput",2);n([E()],me.prototype,"hasFocus",2);n([d({type:Boolean,reflect:!0})],me.prototype,"checked",2);n([d()],me.prototype,"value",2);n([d({type:Boolean,reflect:!0})],me.prototype,"disabled",2);n([d({reflect:!0})],me.prototype,"size",2);n([d({type:Boolean,reflect:!0})],me.prototype,"pill",2);n([C("disabled",{waitUntilFirstUpdate:!0})],me.prototype,"handleDisabledChange",1);me.define("sl-radio-button");var mh=A`
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
`,gh=A`
  :host {
    display: inline-block;
  }

  .button-group {
    display: flex;
    flex-wrap: nowrap;
  }
`,ps=class extends P{constructor(){super(...arguments),this.disableRole=!1,this.label=""}handleFocus(e){const t=Zs(e.target);t==null||t.toggleAttribute("data-sl-button-group__button--focus",!0)}handleBlur(e){const t=Zs(e.target);t==null||t.toggleAttribute("data-sl-button-group__button--focus",!1)}handleMouseOver(e){const t=Zs(e.target);t==null||t.toggleAttribute("data-sl-button-group__button--hover",!0)}handleMouseOut(e){const t=Zs(e.target);t==null||t.toggleAttribute("data-sl-button-group__button--hover",!1)}handleSlotChange(){const e=[...this.defaultSlot.assignedElements({flatten:!0})];e.forEach(t=>{const s=e.indexOf(t),i=Zs(t);i&&(i.toggleAttribute("data-sl-button-group__button",!0),i.toggleAttribute("data-sl-button-group__button--first",s===0),i.toggleAttribute("data-sl-button-group__button--inner",s>0&&s<e.length-1),i.toggleAttribute("data-sl-button-group__button--last",s===e.length-1),i.toggleAttribute("data-sl-button-group__button--radio",i.tagName.toLowerCase()==="sl-radio-button"))})}render(){return v`
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
    `}};ps.styles=[M,gh];n([T("slot")],ps.prototype,"defaultSlot",2);n([E()],ps.prototype,"disableRole",2);n([d()],ps.prototype,"label",2);function Zs(e){var t;const s="sl-button, sl-radio-button";return(t=e.closest(s))!=null?t:e.querySelector(s)}var Et=class extends P{constructor(){super(...arguments),this.formControlController=new Fe(this),this.hasSlotController=new Ht(this,"help-text","label"),this.customValidityMessage="",this.hasButtonGroup=!1,this.errorMessage="",this.defaultValue="",this.label="",this.helpText="",this.name="option",this.value="",this.size="medium",this.form="",this.required=!1}get validity(){const e=this.required&&!this.value;return this.customValidityMessage!==""?Rc:e?Ic:So}get validationMessage(){const e=this.required&&!this.value;return this.customValidityMessage!==""?this.customValidityMessage:e?this.validationInput.validationMessage:""}connectedCallback(){super.connectedCallback(),this.defaultValue=this.value}firstUpdated(){this.formControlController.updateValidity()}getAllRadios(){return[...this.querySelectorAll("sl-radio, sl-radio-button")]}handleRadioClick(e){const t=e.target.closest("sl-radio, sl-radio-button"),s=this.getAllRadios(),i=this.value;!t||t.disabled||(this.value=t.value,s.forEach(o=>o.checked=o===t),this.value!==i&&(this.emit("sl-change"),this.emit("sl-input")))}handleKeyDown(e){var t;if(!["ArrowUp","ArrowDown","ArrowLeft","ArrowRight"," "].includes(e.key))return;const s=this.getAllRadios().filter(l=>!l.disabled),i=(t=s.find(l=>l.checked))!=null?t:s[0],o=e.key===" "?0:["ArrowUp","ArrowLeft"].includes(e.key)?-1:1,r=this.value;let a=s.indexOf(i)+o;a<0&&(a=s.length-1),a>s.length-1&&(a=0),this.getAllRadios().forEach(l=>{l.checked=!1,this.hasButtonGroup||l.setAttribute("tabindex","-1")}),this.value=s[a].value,s[a].checked=!0,this.hasButtonGroup?s[a].shadowRoot.querySelector("button").focus():(s[a].setAttribute("tabindex","0"),s[a].focus()),this.value!==r&&(this.emit("sl-change"),this.emit("sl-input")),e.preventDefault()}handleLabelClick(){this.focus()}handleInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}async syncRadioElements(){var e,t;const s=this.getAllRadios();if(await Promise.all(s.map(async i=>{await i.updateComplete,i.checked=i.value===this.value,i.size=this.size})),this.hasButtonGroup=s.some(i=>i.tagName.toLowerCase()==="sl-radio-button"),s.length>0&&!s.some(i=>i.checked))if(this.hasButtonGroup){const i=(e=s[0].shadowRoot)==null?void 0:e.querySelector("button");i&&i.setAttribute("tabindex","0")}else s[0].setAttribute("tabindex","0");if(this.hasButtonGroup){const i=(t=this.shadowRoot)==null?void 0:t.querySelector("sl-button-group");i&&(i.disableRole=!0)}}syncRadios(){if(customElements.get("sl-radio")&&customElements.get("sl-radio-button")){this.syncRadioElements();return}customElements.get("sl-radio")?this.syncRadioElements():customElements.whenDefined("sl-radio").then(()=>this.syncRadios()),customElements.get("sl-radio-button")?this.syncRadioElements():customElements.whenDefined("sl-radio-button").then(()=>this.syncRadios())}updateCheckedRadio(){this.getAllRadios().forEach(t=>t.checked=t.value===this.value),this.formControlController.setValidity(this.validity.valid)}handleSizeChange(){this.syncRadios()}handleValueChange(){this.hasUpdated&&this.updateCheckedRadio()}checkValidity(){const e=this.required&&!this.value,t=this.customValidityMessage!=="";return e||t?(this.formControlController.emitInvalidEvent(),!1):!0}getForm(){return this.formControlController.getForm()}reportValidity(){const e=this.validity.valid;return this.errorMessage=this.customValidityMessage||e?"":this.validationInput.validationMessage,this.formControlController.setValidity(e),this.validationInput.hidden=!0,clearTimeout(this.validationTimeout),e||(this.validationInput.hidden=!1,this.validationInput.reportValidity(),this.validationTimeout=setTimeout(()=>this.validationInput.hidden=!0,1e4)),e}setCustomValidity(e=""){this.customValidityMessage=e,this.errorMessage=e,this.validationInput.setCustomValidity(e),this.formControlController.updateValidity()}focus(e){const t=this.getAllRadios(),s=t.find(r=>r.checked),i=t.find(r=>!r.disabled),o=s||i;o&&o.focus(e)}render(){const e=this.hasSlotController.test("label"),t=this.hasSlotController.test("help-text"),s=this.label?!0:!!e,i=this.helpText?!0:!!t,o=v`
      <slot @slotchange=${this.syncRadios} @click=${this.handleRadioClick} @keydown=${this.handleKeyDown}></slot>
    `;return v`
      <fieldset
        part="form-control"
        class=${L({"form-control":!0,"form-control--small":this.size==="small","form-control--medium":this.size==="medium","form-control--large":this.size==="large","form-control--radio-group":!0,"form-control--has-label":s,"form-control--has-help-text":i})}
        role="radiogroup"
        aria-labelledby="label"
        aria-describedby="help-text"
        aria-errormessage="error-message"
      >
        <label
          part="form-control-label"
          id="label"
          class="form-control__label"
          aria-hidden=${s?"false":"true"}
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

          ${this.hasButtonGroup?v`
                <sl-button-group part="button-group" exportparts="base:button-group__base" role="presentation">
                  ${o}
                </sl-button-group>
              `:o}
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
    `}};Et.styles=[M,ds,mh];Et.dependencies={"sl-button-group":ps};n([T("slot:not([name])")],Et.prototype,"defaultSlot",2);n([T(".radio-group__validation-input")],Et.prototype,"validationInput",2);n([E()],Et.prototype,"hasButtonGroup",2);n([E()],Et.prototype,"errorMessage",2);n([E()],Et.prototype,"defaultValue",2);n([d()],Et.prototype,"label",2);n([d({attribute:"help-text"})],Et.prototype,"helpText",2);n([d()],Et.prototype,"name",2);n([d({reflect:!0})],Et.prototype,"value",2);n([d({reflect:!0})],Et.prototype,"size",2);n([d({reflect:!0})],Et.prototype,"form",2);n([d({type:Boolean,reflect:!0})],Et.prototype,"required",2);n([C("size",{waitUntilFirstUpdate:!0})],Et.prototype,"handleSizeChange",1);n([C("value")],Et.prototype,"handleValueChange",1);Et.define("sl-radio-group");var bh=A`
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
`,Ms=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.value=0,this.label=""}updated(e){if(super.updated(e),e.has("value")){const t=parseFloat(getComputedStyle(this.indicator).getPropertyValue("r")),s=2*Math.PI*t,i=s-this.value/100*s;this.indicatorOffset=`${i}px`}}render(){return v`
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
    `}};Ms.styles=[M,bh];n([T(".progress-ring__indicator")],Ms.prototype,"indicator",2);n([E()],Ms.prototype,"indicatorOffset",2);n([d({type:Number,reflect:!0})],Ms.prototype,"value",2);n([d()],Ms.prototype,"label",2);Ms.define("sl-progress-ring");var vh=A`
  :host {
    display: inline-block;
  }
`;let rl=null;class al{}al.render=function(e,t){rl(e,t)};self.QrCreator=al;(function(e){function t(l,c,h,u){var p={},f=e(h,c);f.u(l),f.J(),u=u||0;var m=f.h(),b=f.h()+2*u;return p.text=l,p.level=c,p.version=h,p.O=b,p.a=function(y,k){return y-=u,k-=u,0>y||y>=m||0>k||k>=m?!1:f.a(y,k)},p}function s(l,c,h,u,p,f,m,b,y,k){function z(_,S,w,x,D,B,U){_?(l.lineTo(S+B,w+U),l.arcTo(S,w,x,D,f)):l.lineTo(S,w)}m?l.moveTo(c+f,h):l.moveTo(c,h),z(b,u,h,u,p,-f,0),z(y,u,p,c,p,0,-f),z(k,c,p,c,h,f,0),z(m,c,h,u,h,0,f)}function i(l,c,h,u,p,f,m,b,y,k){function z(_,S,w,x){l.moveTo(_+w,S),l.lineTo(_,S),l.lineTo(_,S+x),l.arcTo(_,S,_+w,S,f)}m&&z(c,h,f,f),b&&z(u,h,-f,f),y&&z(u,p,-f,-f),k&&z(c,p,f,-f)}function o(l,c){var h=c.fill;if(typeof h=="string")l.fillStyle=h;else{var u=h.type,p=h.colorStops;if(h=h.position.map(m=>Math.round(m*c.size)),u==="linear-gradient")var f=l.createLinearGradient.apply(l,h);else if(u==="radial-gradient")f=l.createRadialGradient.apply(l,h);else throw Error("Unsupported fill");p.forEach(([m,b])=>{f.addColorStop(m,b)}),l.fillStyle=f}}function r(l,c){t:{var h=c.text,u=c.v,p=c.N,f=c.K,m=c.P;for(p=Math.max(1,p||1),f=Math.min(40,f||40);p<=f;p+=1)try{var b=t(h,u,p,m);break t}catch{}b=void 0}if(!b)return null;for(h=l.getContext("2d"),c.background&&(h.fillStyle=c.background,h.fillRect(c.left,c.top,c.size,c.size)),u=b.O,f=c.size/u,h.beginPath(),m=0;m<u;m+=1)for(p=0;p<u;p+=1){var y=h,k=c.left+p*f,z=c.top+m*f,_=m,S=p,w=b.a,x=k+f,D=z+f,B=_-1,U=_+1,F=S-1,R=S+1,q=Math.floor(Math.min(.5,Math.max(0,c.R))*f),ot=w(_,S),mt=w(B,F),rt=w(B,S);B=w(B,R);var qt=w(_,R);R=w(U,R),S=w(U,S),U=w(U,F),_=w(_,F),k=Math.round(k),z=Math.round(z),x=Math.round(x),D=Math.round(D),ot?s(y,k,z,x,D,q,!rt&&!_,!rt&&!qt,!S&&!qt,!S&&!_):i(y,k,z,x,D,q,rt&&_&&mt,rt&&qt&&B,S&&qt&&R,S&&_&&U)}return o(h,c),h.fill(),l}var a={minVersion:1,maxVersion:40,ecLevel:"L",left:0,top:0,size:200,fill:"#000",background:null,text:"no text",radius:.5,quiet:0};rl=function(l,c){var h={};Object.assign(h,a,l),h.N=h.minVersion,h.K=h.maxVersion,h.v=h.ecLevel,h.left=h.left,h.top=h.top,h.size=h.size,h.fill=h.fill,h.background=h.background,h.text=h.text,h.R=h.radius,h.P=h.quiet,c instanceof HTMLCanvasElement?((c.width!==h.size||c.height!==h.size)&&(c.width=h.size,c.height=h.size),c.getContext("2d").clearRect(0,0,c.width,c.height),r(c,h)):(l=document.createElement("canvas"),l.width=h.size,l.height=h.size,h=r(l,h),c.appendChild(h))}})(function(){function e(c){var h=s.s(c);return{S:function(){return 4},b:function(){return h.length},write:function(u){for(var p=0;p<h.length;p+=1)u.put(h[p],8)}}}function t(){var c=[],h=0,u={B:function(){return c},c:function(p){return(c[Math.floor(p/8)]>>>7-p%8&1)==1},put:function(p,f){for(var m=0;m<f;m+=1)u.m((p>>>f-m-1&1)==1)},f:function(){return h},m:function(p){var f=Math.floor(h/8);c.length<=f&&c.push(0),p&&(c[f]|=128>>>h%8),h+=1}};return u}function s(c,h){function u(_,S){for(var w=-1;7>=w;w+=1)if(!(-1>=_+w||b<=_+w))for(var x=-1;7>=x;x+=1)-1>=S+x||b<=S+x||(m[_+w][S+x]=0<=w&&6>=w&&(x==0||x==6)||0<=x&&6>=x&&(w==0||w==6)||2<=w&&4>=w&&2<=x&&4>=x)}function p(_,S){for(var w=b=4*c+17,x=Array(w),D=0;D<w;D+=1){x[D]=Array(w);for(var B=0;B<w;B+=1)x[D][B]=null}for(m=x,u(0,0),u(b-7,0),u(0,b-7),w=r.G(c),x=0;x<w.length;x+=1)for(D=0;D<w.length;D+=1){B=w[x];var U=w[D];if(m[B][U]==null)for(var F=-2;2>=F;F+=1)for(var R=-2;2>=R;R+=1)m[B+F][U+R]=F==-2||F==2||R==-2||R==2||F==0&&R==0}for(w=8;w<b-8;w+=1)m[w][6]==null&&(m[w][6]=w%2==0);for(w=8;w<b-8;w+=1)m[6][w]==null&&(m[6][w]=w%2==0);for(w=r.w(f<<3|S),x=0;15>x;x+=1)D=!_&&(w>>x&1)==1,m[6>x?x:8>x?x+1:b-15+x][8]=D,m[8][8>x?b-x-1:9>x?15-x:14-x]=D;if(m[b-8][8]=!_,7<=c){for(w=r.A(c),x=0;18>x;x+=1)D=!_&&(w>>x&1)==1,m[Math.floor(x/3)][x%3+b-8-3]=D;for(x=0;18>x;x+=1)D=!_&&(w>>x&1)==1,m[x%3+b-8-3][Math.floor(x/3)]=D}if(y==null){for(_=l.I(c,f),w=t(),x=0;x<k.length;x+=1)D=k[x],w.put(4,4),w.put(D.b(),r.f(4,c)),D.write(w);for(x=D=0;x<_.length;x+=1)D+=_[x].j;if(w.f()>8*D)throw Error("code length overflow. ("+w.f()+">"+8*D+")");for(w.f()+4<=8*D&&w.put(0,4);w.f()%8!=0;)w.m(!1);for(;!(w.f()>=8*D)&&(w.put(236,8),!(w.f()>=8*D));)w.put(17,8);var q=0;for(D=x=0,B=Array(_.length),U=Array(_.length),F=0;F<_.length;F+=1){var ot=_[F].j,mt=_[F].o-ot;for(x=Math.max(x,ot),D=Math.max(D,mt),B[F]=Array(ot),R=0;R<B[F].length;R+=1)B[F][R]=255&w.B()[R+q];for(q+=ot,R=r.C(mt),ot=i(B[F],R.b()-1).l(R),U[F]=Array(R.b()-1),R=0;R<U[F].length;R+=1)mt=R+ot.b()-U[F].length,U[F][R]=0<=mt?ot.c(mt):0}for(R=w=0;R<_.length;R+=1)w+=_[R].o;for(w=Array(w),R=q=0;R<x;R+=1)for(F=0;F<_.length;F+=1)R<B[F].length&&(w[q]=B[F][R],q+=1);for(R=0;R<D;R+=1)for(F=0;F<_.length;F+=1)R<U[F].length&&(w[q]=U[F][R],q+=1);y=w}for(_=y,w=-1,x=b-1,D=7,B=0,S=r.F(S),U=b-1;0<U;U-=2)for(U==6&&--U;;){for(F=0;2>F;F+=1)m[x][U-F]==null&&(R=!1,B<_.length&&(R=(_[B]>>>D&1)==1),S(x,U-F)&&(R=!R),m[x][U-F]=R,--D,D==-1&&(B+=1,D=7));if(x+=w,0>x||b<=x){x-=w,w=-w;break}}}var f=o[h],m=null,b=0,y=null,k=[],z={u:function(_){_=e(_),k.push(_),y=null},a:function(_,S){if(0>_||b<=_||0>S||b<=S)throw Error(_+","+S);return m[_][S]},h:function(){return b},J:function(){for(var _=0,S=0,w=0;8>w;w+=1){p(!0,w);var x=r.D(z);(w==0||_>x)&&(_=x,S=w)}p(!1,S)}};return z}function i(c,h){if(typeof c.length>"u")throw Error(c.length+"/"+h);var u=function(){for(var f=0;f<c.length&&c[f]==0;)f+=1;for(var m=Array(c.length-f+h),b=0;b<c.length-f;b+=1)m[b]=c[b+f];return m}(),p={c:function(f){return u[f]},b:function(){return u.length},multiply:function(f){for(var m=Array(p.b()+f.b()-1),b=0;b<p.b();b+=1)for(var y=0;y<f.b();y+=1)m[b+y]^=a.i(a.g(p.c(b))+a.g(f.c(y)));return i(m,0)},l:function(f){if(0>p.b()-f.b())return p;for(var m=a.g(p.c(0))-a.g(f.c(0)),b=Array(p.b()),y=0;y<p.b();y+=1)b[y]=p.c(y);for(y=0;y<f.b();y+=1)b[y]^=a.i(a.g(f.c(y))+m);return i(b,0).l(f)}};return p}s.s=function(c){for(var h=[],u=0;u<c.length;u++){var p=c.charCodeAt(u);128>p?h.push(p):2048>p?h.push(192|p>>6,128|p&63):55296>p||57344<=p?h.push(224|p>>12,128|p>>6&63,128|p&63):(u++,p=65536+((p&1023)<<10|c.charCodeAt(u)&1023),h.push(240|p>>18,128|p>>12&63,128|p>>6&63,128|p&63))}return h};var o={L:1,M:0,Q:3,H:2},r=function(){function c(p){for(var f=0;p!=0;)f+=1,p>>>=1;return f}var h=[[],[6,18],[6,22],[6,26],[6,30],[6,34],[6,22,38],[6,24,42],[6,26,46],[6,28,50],[6,30,54],[6,32,58],[6,34,62],[6,26,46,66],[6,26,48,70],[6,26,50,74],[6,30,54,78],[6,30,56,82],[6,30,58,86],[6,34,62,90],[6,28,50,72,94],[6,26,50,74,98],[6,30,54,78,102],[6,28,54,80,106],[6,32,58,84,110],[6,30,58,86,114],[6,34,62,90,118],[6,26,50,74,98,122],[6,30,54,78,102,126],[6,26,52,78,104,130],[6,30,56,82,108,134],[6,34,60,86,112,138],[6,30,58,86,114,142],[6,34,62,90,118,146],[6,30,54,78,102,126,150],[6,24,50,76,102,128,154],[6,28,54,80,106,132,158],[6,32,58,84,110,136,162],[6,26,54,82,110,138,166],[6,30,58,86,114,142,170]],u={w:function(p){for(var f=p<<10;0<=c(f)-c(1335);)f^=1335<<c(f)-c(1335);return(p<<10|f)^21522},A:function(p){for(var f=p<<12;0<=c(f)-c(7973);)f^=7973<<c(f)-c(7973);return p<<12|f},G:function(p){return h[p-1]},F:function(p){switch(p){case 0:return function(f,m){return(f+m)%2==0};case 1:return function(f){return f%2==0};case 2:return function(f,m){return m%3==0};case 3:return function(f,m){return(f+m)%3==0};case 4:return function(f,m){return(Math.floor(f/2)+Math.floor(m/3))%2==0};case 5:return function(f,m){return f*m%2+f*m%3==0};case 6:return function(f,m){return(f*m%2+f*m%3)%2==0};case 7:return function(f,m){return(f*m%3+(f+m)%2)%2==0};default:throw Error("bad maskPattern:"+p)}},C:function(p){for(var f=i([1],0),m=0;m<p;m+=1)f=f.multiply(i([1,a.i(m)],0));return f},f:function(p,f){if(p!=4||1>f||40<f)throw Error("mode: "+p+"; type: "+f);return 10>f?8:16},D:function(p){for(var f=p.h(),m=0,b=0;b<f;b+=1)for(var y=0;y<f;y+=1){for(var k=0,z=p.a(b,y),_=-1;1>=_;_+=1)if(!(0>b+_||f<=b+_))for(var S=-1;1>=S;S+=1)0>y+S||f<=y+S||(_!=0||S!=0)&&z==p.a(b+_,y+S)&&(k+=1);5<k&&(m+=3+k-5)}for(b=0;b<f-1;b+=1)for(y=0;y<f-1;y+=1)k=0,p.a(b,y)&&(k+=1),p.a(b+1,y)&&(k+=1),p.a(b,y+1)&&(k+=1),p.a(b+1,y+1)&&(k+=1),(k==0||k==4)&&(m+=3);for(b=0;b<f;b+=1)for(y=0;y<f-6;y+=1)p.a(b,y)&&!p.a(b,y+1)&&p.a(b,y+2)&&p.a(b,y+3)&&p.a(b,y+4)&&!p.a(b,y+5)&&p.a(b,y+6)&&(m+=40);for(y=0;y<f;y+=1)for(b=0;b<f-6;b+=1)p.a(b,y)&&!p.a(b+1,y)&&p.a(b+2,y)&&p.a(b+3,y)&&p.a(b+4,y)&&!p.a(b+5,y)&&p.a(b+6,y)&&(m+=40);for(y=k=0;y<f;y+=1)for(b=0;b<f;b+=1)p.a(b,y)&&(k+=1);return m+=Math.abs(100*k/f/f-50)/5*10}};return u}(),a=function(){for(var c=Array(256),h=Array(256),u=0;8>u;u+=1)c[u]=1<<u;for(u=8;256>u;u+=1)c[u]=c[u-4]^c[u-5]^c[u-6]^c[u-8];for(u=0;255>u;u+=1)h[c[u]]=u;return{g:function(p){if(1>p)throw Error("glog("+p+")");return h[p]},i:function(p){for(;0>p;)p+=255;for(;256<=p;)p-=255;return c[p]}}}(),l=function(){function c(p,f){switch(f){case o.L:return h[4*(p-1)];case o.M:return h[4*(p-1)+1];case o.Q:return h[4*(p-1)+2];case o.H:return h[4*(p-1)+3]}}var h=[[1,26,19],[1,26,16],[1,26,13],[1,26,9],[1,44,34],[1,44,28],[1,44,22],[1,44,16],[1,70,55],[1,70,44],[2,35,17],[2,35,13],[1,100,80],[2,50,32],[2,50,24],[4,25,9],[1,134,108],[2,67,43],[2,33,15,2,34,16],[2,33,11,2,34,12],[2,86,68],[4,43,27],[4,43,19],[4,43,15],[2,98,78],[4,49,31],[2,32,14,4,33,15],[4,39,13,1,40,14],[2,121,97],[2,60,38,2,61,39],[4,40,18,2,41,19],[4,40,14,2,41,15],[2,146,116],[3,58,36,2,59,37],[4,36,16,4,37,17],[4,36,12,4,37,13],[2,86,68,2,87,69],[4,69,43,1,70,44],[6,43,19,2,44,20],[6,43,15,2,44,16],[4,101,81],[1,80,50,4,81,51],[4,50,22,4,51,23],[3,36,12,8,37,13],[2,116,92,2,117,93],[6,58,36,2,59,37],[4,46,20,6,47,21],[7,42,14,4,43,15],[4,133,107],[8,59,37,1,60,38],[8,44,20,4,45,21],[12,33,11,4,34,12],[3,145,115,1,146,116],[4,64,40,5,65,41],[11,36,16,5,37,17],[11,36,12,5,37,13],[5,109,87,1,110,88],[5,65,41,5,66,42],[5,54,24,7,55,25],[11,36,12,7,37,13],[5,122,98,1,123,99],[7,73,45,3,74,46],[15,43,19,2,44,20],[3,45,15,13,46,16],[1,135,107,5,136,108],[10,74,46,1,75,47],[1,50,22,15,51,23],[2,42,14,17,43,15],[5,150,120,1,151,121],[9,69,43,4,70,44],[17,50,22,1,51,23],[2,42,14,19,43,15],[3,141,113,4,142,114],[3,70,44,11,71,45],[17,47,21,4,48,22],[9,39,13,16,40,14],[3,135,107,5,136,108],[3,67,41,13,68,42],[15,54,24,5,55,25],[15,43,15,10,44,16],[4,144,116,4,145,117],[17,68,42],[17,50,22,6,51,23],[19,46,16,6,47,17],[2,139,111,7,140,112],[17,74,46],[7,54,24,16,55,25],[34,37,13],[4,151,121,5,152,122],[4,75,47,14,76,48],[11,54,24,14,55,25],[16,45,15,14,46,16],[6,147,117,4,148,118],[6,73,45,14,74,46],[11,54,24,16,55,25],[30,46,16,2,47,17],[8,132,106,4,133,107],[8,75,47,13,76,48],[7,54,24,22,55,25],[22,45,15,13,46,16],[10,142,114,2,143,115],[19,74,46,4,75,47],[28,50,22,6,51,23],[33,46,16,4,47,17],[8,152,122,4,153,123],[22,73,45,3,74,46],[8,53,23,26,54,24],[12,45,15,28,46,16],[3,147,117,10,148,118],[3,73,45,23,74,46],[4,54,24,31,55,25],[11,45,15,31,46,16],[7,146,116,7,147,117],[21,73,45,7,74,46],[1,53,23,37,54,24],[19,45,15,26,46,16],[5,145,115,10,146,116],[19,75,47,10,76,48],[15,54,24,25,55,25],[23,45,15,25,46,16],[13,145,115,3,146,116],[2,74,46,29,75,47],[42,54,24,1,55,25],[23,45,15,28,46,16],[17,145,115],[10,74,46,23,75,47],[10,54,24,35,55,25],[19,45,15,35,46,16],[17,145,115,1,146,116],[14,74,46,21,75,47],[29,54,24,19,55,25],[11,45,15,46,46,16],[13,145,115,6,146,116],[14,74,46,23,75,47],[44,54,24,7,55,25],[59,46,16,1,47,17],[12,151,121,7,152,122],[12,75,47,26,76,48],[39,54,24,14,55,25],[22,45,15,41,46,16],[6,151,121,14,152,122],[6,75,47,34,76,48],[46,54,24,10,55,25],[2,45,15,64,46,16],[17,152,122,4,153,123],[29,74,46,14,75,47],[49,54,24,10,55,25],[24,45,15,46,46,16],[4,152,122,18,153,123],[13,74,46,32,75,47],[48,54,24,14,55,25],[42,45,15,32,46,16],[20,147,117,4,148,118],[40,75,47,7,76,48],[43,54,24,22,55,25],[10,45,15,67,46,16],[19,148,118,6,149,119],[18,75,47,31,76,48],[34,54,24,34,55,25],[20,45,15,61,46,16]],u={I:function(p,f){var m=c(p,f);if(typeof m>"u")throw Error("bad rs block @ typeNumber:"+p+"/errorCorrectLevel:"+f);p=m.length/3,f=[];for(var b=0;b<p;b+=1)for(var y=m[3*b],k=m[3*b+1],z=m[3*b+2],_=0;_<y;_+=1){var S=z,w={};w.o=k,w.j=S,f.push(w)}return f}};return u}();return s}());const yh=QrCreator;var ge=class extends P{constructor(){super(...arguments),this.value="",this.label="",this.size=128,this.fill="black",this.background="white",this.radius=0,this.errorCorrection="H"}firstUpdated(){this.generate()}generate(){this.hasUpdated&&yh.render({text:this.value,radius:this.radius,ecLevel:this.errorCorrection,fill:this.fill,background:this.background,size:this.size*2},this.canvas)}render(){var e;return v`
      <canvas
        part="base"
        class="qr-code"
        role="img"
        aria-label=${((e=this.label)==null?void 0:e.length)>0?this.label:this.value}
        style=${jt({width:`${this.size}px`,height:`${this.size}px`})}
      ></canvas>
    `}};ge.styles=[M,vh];n([T("canvas")],ge.prototype,"canvas",2);n([d()],ge.prototype,"value",2);n([d()],ge.prototype,"label",2);n([d({type:Number})],ge.prototype,"size",2);n([d()],ge.prototype,"fill",2);n([d()],ge.prototype,"background",2);n([d({type:Number})],ge.prototype,"radius",2);n([d({attribute:"error-correction"})],ge.prototype,"errorCorrection",2);n([C(["background","errorCorrection","fill","radius","size","value"])],ge.prototype,"generate",1);ge.define("sl-qr-code");var wh=A`
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
`,Ee=class extends P{constructor(){super(),this.checked=!1,this.hasFocus=!1,this.size="medium",this.disabled=!1,this.handleBlur=()=>{this.hasFocus=!1,this.emit("sl-blur")},this.handleClick=()=>{this.disabled||(this.checked=!0)},this.handleFocus=()=>{this.hasFocus=!0,this.emit("sl-focus")},this.addEventListener("blur",this.handleBlur),this.addEventListener("click",this.handleClick),this.addEventListener("focus",this.handleFocus)}connectedCallback(){super.connectedCallback(),this.setInitialAttributes()}setInitialAttributes(){this.setAttribute("role","radio"),this.setAttribute("tabindex","-1"),this.setAttribute("aria-disabled",this.disabled?"true":"false")}handleCheckedChange(){this.setAttribute("aria-checked",this.checked?"true":"false"),this.setAttribute("tabindex",this.checked?"0":"-1")}handleDisabledChange(){this.setAttribute("aria-disabled",this.disabled?"true":"false")}render(){return v`
      <span
        part="base"
        class=${L({radio:!0,"radio--checked":this.checked,"radio--disabled":this.disabled,"radio--focused":this.hasFocus,"radio--small":this.size==="small","radio--medium":this.size==="medium","radio--large":this.size==="large"})}
      >
        <span part="${`control${this.checked?" control--checked":""}`}" class="radio__control">
          ${this.checked?v` <sl-icon part="checked-icon" class="radio__checked-icon" library="system" name="radio"></sl-icon> `:""}
        </span>

        <slot part="label" class="radio__label"></slot>
      </span>
    `}};Ee.styles=[M,wh];Ee.dependencies={"sl-icon":it};n([E()],Ee.prototype,"checked",2);n([E()],Ee.prototype,"hasFocus",2);n([d()],Ee.prototype,"value",2);n([d({reflect:!0})],Ee.prototype,"size",2);n([d({type:Boolean,reflect:!0})],Ee.prototype,"disabled",2);n([C("checked")],Ee.prototype,"handleCheckedChange",1);n([C("disabled",{waitUntilFirstUpdate:!0})],Ee.prototype,"handleDisabledChange",1);Ee.define("sl-radio");var xh=A`
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
`,ce=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.current=!1,this.selected=!1,this.hasHover=!1,this.value="",this.disabled=!1}connectedCallback(){super.connectedCallback(),this.setAttribute("role","option"),this.setAttribute("aria-selected","false")}handleDefaultSlotChange(){const e=this.getTextLabel();if(typeof this.cachedTextLabel>"u"){this.cachedTextLabel=e;return}e!==this.cachedTextLabel&&(this.cachedTextLabel=e,this.emit("slotchange",{bubbles:!0,composed:!1,cancelable:!1}))}handleMouseEnter(){this.hasHover=!0}handleMouseLeave(){this.hasHover=!1}handleDisabledChange(){this.setAttribute("aria-disabled",this.disabled?"true":"false")}handleSelectedChange(){this.setAttribute("aria-selected",this.selected?"true":"false")}handleValueChange(){typeof this.value!="string"&&(this.value=String(this.value)),this.value.includes(" ")&&(console.error("Option values cannot include a space. All spaces have been replaced with underscores.",this),this.value=this.value.replace(/ /g,"_"))}getTextLabel(){const e=this.childNodes;let t="";return[...e].forEach(s=>{s.nodeType===Node.ELEMENT_NODE&&(s.hasAttribute("slot")||(t+=s.textContent)),s.nodeType===Node.TEXT_NODE&&(t+=s.textContent)}),t.trim()}render(){return v`
      <div
        part="base"
        class=${L({option:!0,"option--current":this.current,"option--disabled":this.disabled,"option--selected":this.selected,"option--hover":this.hasHover})}
        @mouseenter=${this.handleMouseEnter}
        @mouseleave=${this.handleMouseLeave}
      >
        <sl-icon part="checked-icon" class="option__check" name="check" library="system" aria-hidden="true"></sl-icon>
        <slot part="prefix" name="prefix" class="option__prefix"></slot>
        <slot part="label" class="option__label" @slotchange=${this.handleDefaultSlotChange}></slot>
        <slot part="suffix" name="suffix" class="option__suffix"></slot>
      </div>
    `}};ce.styles=[M,xh];ce.dependencies={"sl-icon":it};n([T(".option__label")],ce.prototype,"defaultSlot",2);n([E()],ce.prototype,"current",2);n([E()],ce.prototype,"selected",2);n([E()],ce.prototype,"hasHover",2);n([d({reflect:!0})],ce.prototype,"value",2);n([d({type:Boolean,reflect:!0})],ce.prototype,"disabled",2);n([C("disabled")],ce.prototype,"handleDisabledChange",1);n([C("selected")],ce.prototype,"handleSelectedChange",1);n([C("value")],ce.prototype,"handleValueChange",1);ce.define("sl-option");X.define("sl-popup");var _h=A`
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
`,Pi=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.value=0,this.indeterminate=!1,this.label=""}render(){return v`
      <div
        part="base"
        class=${L({"progress-bar":!0,"progress-bar--indeterminate":this.indeterminate,"progress-bar--rtl":this.localize.dir()==="rtl"})}
        role="progressbar"
        title=${O(this.title)}
        aria-label=${this.label.length>0?this.label:this.localize.term("progress")}
        aria-valuemin="0"
        aria-valuemax="100"
        aria-valuenow=${this.indeterminate?0:this.value}
      >
        <div part="indicator" class="progress-bar__indicator" style=${jt({width:`${this.value}%`})}>
          ${this.indeterminate?"":v` <slot part="label" class="progress-bar__label"></slot> `}
        </div>
      </div>
    `}};Pi.styles=[M,_h];n([d({type:Number,reflect:!0})],Pi.prototype,"value",2);n([d({type:Boolean,reflect:!0})],Pi.prototype,"indeterminate",2);n([d()],Pi.prototype,"label",2);Pi.define("sl-progress-bar");var kh=A`
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
`,nl=class extends P{render(){return v` <slot part="base" class="menu-label"></slot> `}};nl.styles=[M,kh];nl.define("sl-menu-label");var $h=A`
  :host {
    display: contents;
  }
`,Oe=class extends P{constructor(){super(...arguments),this.attrOldValue=!1,this.charData=!1,this.charDataOldValue=!1,this.childList=!1,this.disabled=!1,this.handleMutation=e=>{this.emit("sl-mutation",{detail:{mutationList:e}})}}connectedCallback(){super.connectedCallback(),this.mutationObserver=new MutationObserver(this.handleMutation),this.disabled||this.startObserver()}disconnectedCallback(){super.disconnectedCallback(),this.stopObserver()}startObserver(){const e=typeof this.attr=="string"&&this.attr.length>0,t=e&&this.attr!=="*"?this.attr.split(" "):void 0;try{this.mutationObserver.observe(this,{subtree:!0,childList:this.childList,attributes:e,attributeFilter:t,attributeOldValue:this.attrOldValue,characterData:this.charData,characterDataOldValue:this.charDataOldValue})}catch{}}stopObserver(){this.mutationObserver.disconnect()}handleDisabledChange(){this.disabled?this.stopObserver():this.startObserver()}handleChange(){this.stopObserver(),this.startObserver()}render(){return v` <slot></slot> `}};Oe.styles=[M,$h];n([d({reflect:!0})],Oe.prototype,"attr",2);n([d({attribute:"attr-old-value",type:Boolean,reflect:!0})],Oe.prototype,"attrOldValue",2);n([d({attribute:"char-data",type:Boolean,reflect:!0})],Oe.prototype,"charData",2);n([d({attribute:"char-data-old-value",type:Boolean,reflect:!0})],Oe.prototype,"charDataOldValue",2);n([d({attribute:"child-list",type:Boolean,reflect:!0})],Oe.prototype,"childList",2);n([d({type:Boolean,reflect:!0})],Oe.prototype,"disabled",2);n([C("disabled")],Oe.prototype,"handleDisabledChange",1);n([C("attr",{waitUntilFirstUpdate:!0}),C("attr-old-value",{waitUntilFirstUpdate:!0}),C("char-data",{waitUntilFirstUpdate:!0}),C("char-data-old-value",{waitUntilFirstUpdate:!0}),C("childList",{waitUntilFirstUpdate:!0})],Oe.prototype,"handleChange",1);Oe.define("sl-mutation-observer");var Ch=A`
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
`,H=class extends P{constructor(){super(...arguments),this.formControlController=new Fe(this,{assumeInteractionOn:["sl-blur","sl-input"]}),this.hasSlotController=new Ht(this,"help-text","label"),this.localize=new K(this),this.hasFocus=!1,this.title="",this.__numberInput=Object.assign(document.createElement("input"),{type:"number"}),this.__dateInput=Object.assign(document.createElement("input"),{type:"date"}),this.type="text",this.name="",this.value="",this.defaultValue="",this.size="medium",this.filled=!1,this.pill=!1,this.label="",this.helpText="",this.clearable=!1,this.disabled=!1,this.placeholder="",this.readonly=!1,this.passwordToggle=!1,this.passwordVisible=!1,this.noSpinButtons=!1,this.form="",this.required=!1,this.spellcheck=!0}get valueAsDate(){var e;return this.__dateInput.type=this.type,this.__dateInput.value=this.value,((e=this.input)==null?void 0:e.valueAsDate)||this.__dateInput.valueAsDate}set valueAsDate(e){this.__dateInput.type=this.type,this.__dateInput.valueAsDate=e,this.value=this.__dateInput.value}get valueAsNumber(){var e;return this.__numberInput.value=this.value,((e=this.input)==null?void 0:e.valueAsNumber)||this.__numberInput.valueAsNumber}set valueAsNumber(e){this.__numberInput.valueAsNumber=e,this.value=this.__numberInput.value}get validity(){return this.input.validity}get validationMessage(){return this.input.validationMessage}firstUpdated(){this.formControlController.updateValidity()}handleBlur(){this.hasFocus=!1,this.emit("sl-blur")}handleChange(){this.value=this.input.value,this.emit("sl-change")}handleClearClick(e){e.preventDefault(),this.value!==""&&(this.value="",this.emit("sl-clear"),this.emit("sl-input"),this.emit("sl-change")),this.input.focus()}handleFocus(){this.hasFocus=!0,this.emit("sl-focus")}handleInput(){this.value=this.input.value,this.formControlController.updateValidity(),this.emit("sl-input")}handleInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}handleKeyDown(e){const t=e.metaKey||e.ctrlKey||e.shiftKey||e.altKey;e.key==="Enter"&&!t&&setTimeout(()=>{!e.defaultPrevented&&!e.isComposing&&this.formControlController.submit()})}handlePasswordToggle(){this.passwordVisible=!this.passwordVisible}handleDisabledChange(){this.formControlController.setValidity(this.disabled)}handleStepChange(){this.input.step=String(this.step),this.formControlController.updateValidity()}async handleValueChange(){await this.updateComplete,this.formControlController.updateValidity()}focus(e){this.input.focus(e)}blur(){this.input.blur()}select(){this.input.select()}setSelectionRange(e,t,s="none"){this.input.setSelectionRange(e,t,s)}setRangeText(e,t,s,i="preserve"){const o=t??this.input.selectionStart,r=s??this.input.selectionEnd;this.input.setRangeText(e,o,r,i),this.value!==this.input.value&&(this.value=this.input.value)}showPicker(){"showPicker"in HTMLInputElement.prototype&&this.input.showPicker()}stepUp(){this.input.stepUp(),this.value!==this.input.value&&(this.value=this.input.value)}stepDown(){this.input.stepDown(),this.value!==this.input.value&&(this.value=this.input.value)}checkValidity(){return this.input.checkValidity()}getForm(){return this.formControlController.getForm()}reportValidity(){return this.input.reportValidity()}setCustomValidity(e){this.input.setCustomValidity(e),this.formControlController.updateValidity()}render(){const e=this.hasSlotController.test("label"),t=this.hasSlotController.test("help-text"),s=this.label?!0:!!e,i=this.helpText?!0:!!t,r=this.clearable&&!this.disabled&&!this.readonly&&(typeof this.value=="number"||this.value.length>0);return v`
      <div
        part="form-control"
        class=${L({"form-control":!0,"form-control--small":this.size==="small","form-control--medium":this.size==="medium","form-control--large":this.size==="large","form-control--has-label":s,"form-control--has-help-text":i})}
      >
        <label
          part="form-control-label"
          class="form-control__label"
          for="input"
          aria-hidden=${s?"false":"true"}
        >
          <slot name="label">${this.label}</slot>
        </label>

        <div part="form-control-input" class="form-control-input">
          <div
            part="base"
            class=${L({input:!0,"input--small":this.size==="small","input--medium":this.size==="medium","input--large":this.size==="large","input--pill":this.pill,"input--standard":!this.filled,"input--filled":this.filled,"input--disabled":this.disabled,"input--focused":this.hasFocus,"input--empty":!this.value,"input--no-spin-buttons":this.noSpinButtons})}
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
              name=${O(this.name)}
              ?disabled=${this.disabled}
              ?readonly=${this.readonly}
              ?required=${this.required}
              placeholder=${O(this.placeholder)}
              minlength=${O(this.minlength)}
              maxlength=${O(this.maxlength)}
              min=${O(this.min)}
              max=${O(this.max)}
              step=${O(this.step)}
              .value=${as(this.value)}
              autocapitalize=${O(this.autocapitalize)}
              autocomplete=${O(this.autocomplete)}
              autocorrect=${O(this.autocorrect)}
              ?autofocus=${this.autofocus}
              spellcheck=${this.spellcheck}
              pattern=${O(this.pattern)}
              enterkeyhint=${O(this.enterkeyhint)}
              inputmode=${O(this.inputmode)}
              aria-describedby="help-text"
              @change=${this.handleChange}
              @input=${this.handleInput}
              @invalid=${this.handleInvalid}
              @keydown=${this.handleKeyDown}
              @focus=${this.handleFocus}
              @blur=${this.handleBlur}
            />

            ${r?v`
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
            ${this.passwordToggle&&!this.disabled?v`
                  <button
                    part="password-toggle-button"
                    class="input__password-toggle"
                    type="button"
                    aria-label=${this.localize.term(this.passwordVisible?"hidePassword":"showPassword")}
                    @click=${this.handlePasswordToggle}
                    tabindex="-1"
                  >
                    ${this.passwordVisible?v`
                          <slot name="show-password-icon">
                            <sl-icon name="eye-slash" library="system"></sl-icon>
                          </slot>
                        `:v`
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
    `}};H.styles=[M,ds,Ch];H.dependencies={"sl-icon":it};n([T(".input__control")],H.prototype,"input",2);n([E()],H.prototype,"hasFocus",2);n([d()],H.prototype,"title",2);n([d({reflect:!0})],H.prototype,"type",2);n([d()],H.prototype,"name",2);n([d()],H.prototype,"value",2);n([cs()],H.prototype,"defaultValue",2);n([d({reflect:!0})],H.prototype,"size",2);n([d({type:Boolean,reflect:!0})],H.prototype,"filled",2);n([d({type:Boolean,reflect:!0})],H.prototype,"pill",2);n([d()],H.prototype,"label",2);n([d({attribute:"help-text"})],H.prototype,"helpText",2);n([d({type:Boolean})],H.prototype,"clearable",2);n([d({type:Boolean,reflect:!0})],H.prototype,"disabled",2);n([d()],H.prototype,"placeholder",2);n([d({type:Boolean,reflect:!0})],H.prototype,"readonly",2);n([d({attribute:"password-toggle",type:Boolean})],H.prototype,"passwordToggle",2);n([d({attribute:"password-visible",type:Boolean})],H.prototype,"passwordVisible",2);n([d({attribute:"no-spin-buttons",type:Boolean})],H.prototype,"noSpinButtons",2);n([d({reflect:!0})],H.prototype,"form",2);n([d({type:Boolean,reflect:!0})],H.prototype,"required",2);n([d()],H.prototype,"pattern",2);n([d({type:Number})],H.prototype,"minlength",2);n([d({type:Number})],H.prototype,"maxlength",2);n([d()],H.prototype,"min",2);n([d()],H.prototype,"max",2);n([d()],H.prototype,"step",2);n([d()],H.prototype,"autocapitalize",2);n([d()],H.prototype,"autocorrect",2);n([d()],H.prototype,"autocomplete",2);n([d({type:Boolean})],H.prototype,"autofocus",2);n([d()],H.prototype,"enterkeyhint",2);n([d({type:Boolean,converter:{fromAttribute:e=>!(!e||e==="false"),toAttribute:e=>e?"true":"false"}})],H.prototype,"spellcheck",2);n([d()],H.prototype,"inputmode",2);n([C("disabled",{waitUntilFirstUpdate:!0})],H.prototype,"handleDisabledChange",1);n([C("step",{waitUntilFirstUpdate:!0})],H.prototype,"handleStepChange",1);n([C("value",{waitUntilFirstUpdate:!0})],H.prototype,"handleValueChange",1);H.define("sl-input");var Sh=A`
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
`,Wr=class extends P{connectedCallback(){super.connectedCallback(),this.setAttribute("role","menu")}handleClick(e){const t=["menuitem","menuitemcheckbox"],s=e.composedPath(),i=s.find(l=>{var c;return t.includes(((c=l==null?void 0:l.getAttribute)==null?void 0:c.call(l,"role"))||"")});if(!i||s.find(l=>{var c;return((c=l==null?void 0:l.getAttribute)==null?void 0:c.call(l,"role"))==="menu"})!==this)return;const a=i;a.type==="checkbox"&&(a.checked=!a.checked),this.emit("sl-select",{detail:{item:a}})}handleKeyDown(e){if(e.key==="Enter"||e.key===" "){const t=this.getCurrentItem();e.preventDefault(),e.stopPropagation(),t==null||t.click()}else if(["ArrowDown","ArrowUp","Home","End"].includes(e.key)){const t=this.getAllItems(),s=this.getCurrentItem();let i=s?t.indexOf(s):0;t.length>0&&(e.preventDefault(),e.stopPropagation(),e.key==="ArrowDown"?i++:e.key==="ArrowUp"?i--:e.key==="Home"?i=0:e.key==="End"&&(i=t.length-1),i<0&&(i=t.length-1),i>t.length-1&&(i=0),this.setCurrentItem(t[i]),t[i].focus())}}handleMouseDown(e){const t=e.target;this.isMenuItem(t)&&this.setCurrentItem(t)}handleSlotChange(){const e=this.getAllItems();e.length>0&&this.setCurrentItem(e[0])}isMenuItem(e){var t;return e.tagName.toLowerCase()==="sl-menu-item"||["menuitem","menuitemcheckbox","menuitemradio"].includes((t=e.getAttribute("role"))!=null?t:"")}getAllItems(){return[...this.defaultSlot.assignedElements({flatten:!0})].filter(e=>!(e.inert||!this.isMenuItem(e)))}getCurrentItem(){return this.getAllItems().find(e=>e.getAttribute("tabindex")==="0")}setCurrentItem(e){this.getAllItems().forEach(s=>{s.setAttribute("tabindex",s===e?"0":"-1")})}render(){return v`
      <slot
        @slotchange=${this.handleSlotChange}
        @click=${this.handleClick}
        @keydown=${this.handleKeyDown}
        @mousedown=${this.handleMouseDown}
      ></slot>
    `}};Wr.styles=[M,Sh];n([T("slot")],Wr.prototype,"defaultSlot",2);Wr.define("sl-menu");var Ah=A`
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
 */const fi=(e,t)=>{var i;const s=e._$AN;if(s===void 0)return!1;for(const o of s)(i=o._$AO)==null||i.call(o,t,!1),fi(o,t);return!0},ho=e=>{let t,s;do{if((t=e._$AM)===void 0)break;s=t._$AN,s.delete(e),e=t}while((s==null?void 0:s.size)===0)},ll=e=>{for(let t;t=e._$AM;e=t){let s=t._$AN;if(s===void 0)t._$AN=s=new Set;else if(s.has(e))break;s.add(e),Eh(t)}};function Th(e){this._$AN!==void 0?(ho(this),this._$AM=e,ll(this)):this._$AM=e}function zh(e,t=!1,s=0){const i=this._$AH,o=this._$AN;if(o!==void 0&&o.size!==0)if(t)if(Array.isArray(i))for(let r=s;r<i.length;r++)fi(i[r],!1),ho(i[r]);else i!=null&&(fi(i,!1),ho(i));else fi(this,e)}const Eh=e=>{e.type==Ce.CHILD&&(e._$AP??(e._$AP=zh),e._$AQ??(e._$AQ=Th))};class cl extends Ti{constructor(){super(...arguments),this._$AN=void 0}_$AT(t,s,i){super._$AT(t,s,i),ll(this),this.isConnected=t._$AU}_$AO(t,s=!0){var i,o;t!==this.isConnected&&(this.isConnected=t,t?(i=this.reconnected)==null||i.call(this):(o=this.disconnected)==null||o.call(this)),s&&(fi(this,t),ho(this))}setValue(t){if(el(this._$Ct))this._$Ct._$AI(t,this);else{const s=[...this._$Ct._$AH];s[this._$Ci]=t,this._$Ct._$AI(s,this,0)}}disconnected(){}reconnected(){}}/**
 * @license
 * Copyright 2020 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const Oh=()=>new Ph;let Ph=class{};const er=new WeakMap,Dh=Ls(class extends cl{render(e){return G}update(e,[t]){var i;const s=t!==this.Y;return s&&this.Y!==void 0&&this.rt(void 0),(s||this.lt!==this.ct)&&(this.Y=t,this.ht=(i=e.options)==null?void 0:i.host,this.rt(this.ct=e.element)),G}rt(e){if(this.isConnected||(e=void 0),typeof this.Y=="function"){const t=this.ht??globalThis;let s=er.get(t);s===void 0&&(s=new WeakMap,er.set(t,s)),s.get(this.Y)!==void 0&&this.Y.call(this.ht,void 0),s.set(this.Y,e),e!==void 0&&this.Y.call(this.ht,e)}else this.Y.value=e}get lt(){var e,t;return typeof this.Y=="function"?(e=er.get(this.ht??globalThis))==null?void 0:e.get(this.Y):(t=this.Y)==null?void 0:t.value}disconnected(){this.lt===this.ct&&this.rt(void 0)}reconnected(){this.rt(this.ct)}});var Ih=class{constructor(e,t){this.popupRef=Oh(),this.enableSubmenuTimer=-1,this.isConnected=!1,this.isPopupConnected=!1,this.skidding=0,this.submenuOpenDelay=100,this.handleMouseMove=s=>{this.host.style.setProperty("--safe-triangle-cursor-x",`${s.clientX}px`),this.host.style.setProperty("--safe-triangle-cursor-y",`${s.clientY}px`)},this.handleMouseOver=()=>{this.hasSlotController.test("submenu")&&this.enableSubmenu()},this.handleKeyDown=s=>{switch(s.key){case"Escape":case"Tab":this.disableSubmenu();break;case"ArrowLeft":s.target!==this.host&&(s.preventDefault(),s.stopPropagation(),this.host.focus(),this.disableSubmenu());break;case"ArrowRight":case"Enter":case" ":this.handleSubmenuEntry(s);break}},this.handleClick=s=>{var i;s.target===this.host?(s.preventDefault(),s.stopPropagation()):s.target instanceof Element&&(s.target.tagName==="sl-menu-item"||(i=s.target.role)!=null&&i.startsWith("menuitem"))&&this.disableSubmenu()},this.handleFocusOut=s=>{s.relatedTarget&&s.relatedTarget instanceof Element&&this.host.contains(s.relatedTarget)||this.disableSubmenu()},this.handlePopupMouseover=s=>{s.stopPropagation()},this.handlePopupReposition=()=>{const s=this.host.renderRoot.querySelector("slot[name='submenu']"),i=s==null?void 0:s.assignedElements({flatten:!0}).filter(h=>h.localName==="sl-menu")[0],o=getComputedStyle(this.host).direction==="rtl";if(!i)return;const{left:r,top:a,width:l,height:c}=i.getBoundingClientRect();this.host.style.setProperty("--safe-triangle-submenu-start-x",`${o?r+l:r}px`),this.host.style.setProperty("--safe-triangle-submenu-start-y",`${a}px`),this.host.style.setProperty("--safe-triangle-submenu-end-x",`${o?r+l:r}px`),this.host.style.setProperty("--safe-triangle-submenu-end-y",`${a+c}px`)},(this.host=e).addController(this),this.hasSlotController=t}hostConnected(){this.hasSlotController.test("submenu")&&!this.host.disabled&&this.addListeners()}hostDisconnected(){this.removeListeners()}hostUpdated(){this.hasSlotController.test("submenu")&&!this.host.disabled?(this.addListeners(),this.updateSkidding()):this.removeListeners()}addListeners(){this.isConnected||(this.host.addEventListener("mousemove",this.handleMouseMove),this.host.addEventListener("mouseover",this.handleMouseOver),this.host.addEventListener("keydown",this.handleKeyDown),this.host.addEventListener("click",this.handleClick),this.host.addEventListener("focusout",this.handleFocusOut),this.isConnected=!0),this.isPopupConnected||this.popupRef.value&&(this.popupRef.value.addEventListener("mouseover",this.handlePopupMouseover),this.popupRef.value.addEventListener("sl-reposition",this.handlePopupReposition),this.isPopupConnected=!0)}removeListeners(){this.isConnected&&(this.host.removeEventListener("mousemove",this.handleMouseMove),this.host.removeEventListener("mouseover",this.handleMouseOver),this.host.removeEventListener("keydown",this.handleKeyDown),this.host.removeEventListener("click",this.handleClick),this.host.removeEventListener("focusout",this.handleFocusOut),this.isConnected=!1),this.isPopupConnected&&this.popupRef.value&&(this.popupRef.value.removeEventListener("mouseover",this.handlePopupMouseover),this.popupRef.value.removeEventListener("sl-reposition",this.handlePopupReposition),this.isPopupConnected=!1)}handleSubmenuEntry(e){const t=this.host.renderRoot.querySelector("slot[name='submenu']");if(!t){console.error("Cannot activate a submenu if no corresponding menuitem can be found.",this);return}let s=null;for(const i of t.assignedElements())if(s=i.querySelectorAll("sl-menu-item, [role^='menuitem']"),s.length!==0)break;if(!(!s||s.length===0)){s[0].setAttribute("tabindex","0");for(let i=1;i!==s.length;++i)s[i].setAttribute("tabindex","-1");this.popupRef.value&&(e.preventDefault(),e.stopPropagation(),this.popupRef.value.active?s[0]instanceof HTMLElement&&s[0].focus():(this.enableSubmenu(!1),this.host.updateComplete.then(()=>{s[0]instanceof HTMLElement&&s[0].focus()}),this.host.requestUpdate()))}}setSubmenuState(e){this.popupRef.value&&this.popupRef.value.active!==e&&(this.popupRef.value.active=e,this.host.requestUpdate())}enableSubmenu(e=!0){e?(window.clearTimeout(this.enableSubmenuTimer),this.enableSubmenuTimer=window.setTimeout(()=>{this.setSubmenuState(!0)},this.submenuOpenDelay)):this.setSubmenuState(!0)}disableSubmenu(){window.clearTimeout(this.enableSubmenuTimer),this.setSubmenuState(!1)}updateSkidding(){var e;if(!((e=this.host.parentElement)!=null&&e.computedStyleMap))return;const t=this.host.parentElement.computedStyleMap(),i=["padding-top","border-top-width","margin-top"].reduce((o,r)=>{var a;const l=(a=t.get(r))!=null?a:new CSSUnitValue(0,"px"),h=(l instanceof CSSUnitValue?l:new CSSUnitValue(0,"px")).to("px");return o-h.value},0);this.skidding=i}isExpanded(){return this.popupRef.value?this.popupRef.value.active:!1}renderSubmenu(){const e=getComputedStyle(this.host).direction==="rtl";return this.isConnected?v`
      <sl-popup
        ${Dh(this.popupRef)}
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
    `:v` <slot name="submenu" hidden></slot> `}},Jt=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.type="normal",this.checked=!1,this.value="",this.loading=!1,this.disabled=!1,this.hasSlotController=new Ht(this,"submenu"),this.submenuController=new Ih(this,this.hasSlotController),this.handleHostClick=e=>{this.disabled&&(e.preventDefault(),e.stopImmediatePropagation())},this.handleMouseOver=e=>{this.focus(),e.stopPropagation()}}connectedCallback(){super.connectedCallback(),this.addEventListener("click",this.handleHostClick),this.addEventListener("mouseover",this.handleMouseOver)}disconnectedCallback(){super.disconnectedCallback(),this.removeEventListener("click",this.handleHostClick),this.removeEventListener("mouseover",this.handleMouseOver)}handleDefaultSlotChange(){const e=this.getTextLabel();if(typeof this.cachedTextLabel>"u"){this.cachedTextLabel=e;return}e!==this.cachedTextLabel&&(this.cachedTextLabel=e,this.emit("slotchange",{bubbles:!0,composed:!1,cancelable:!1}))}handleCheckedChange(){if(this.checked&&this.type!=="checkbox"){this.checked=!1,console.error('The checked attribute can only be used on menu items with type="checkbox"',this);return}this.type==="checkbox"?this.setAttribute("aria-checked",this.checked?"true":"false"):this.removeAttribute("aria-checked")}handleDisabledChange(){this.setAttribute("aria-disabled",this.disabled?"true":"false")}handleTypeChange(){this.type==="checkbox"?(this.setAttribute("role","menuitemcheckbox"),this.setAttribute("aria-checked",this.checked?"true":"false")):(this.setAttribute("role","menuitem"),this.removeAttribute("aria-checked"))}getTextLabel(){return Od(this.defaultSlot)}isSubmenu(){return this.hasSlotController.test("submenu")}render(){const e=this.localize.dir()==="rtl",t=this.submenuController.isExpanded();return v`
      <div
        id="anchor"
        part="base"
        class=${L({"menu-item":!0,"menu-item--rtl":e,"menu-item--checked":this.checked,"menu-item--disabled":this.disabled,"menu-item--loading":this.loading,"menu-item--has-submenu":this.isSubmenu(),"menu-item--submenu-expanded":t})}
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
        ${this.loading?v` <sl-spinner part="spinner" exportparts="base:spinner__base"></sl-spinner> `:""}
      </div>
    `}};Jt.styles=[M,Ah];Jt.dependencies={"sl-icon":it,"sl-popup":X,"sl-spinner":zi};n([T("slot:not([name])")],Jt.prototype,"defaultSlot",2);n([T(".menu-item")],Jt.prototype,"menuItem",2);n([d()],Jt.prototype,"type",2);n([d({type:Boolean,reflect:!0})],Jt.prototype,"checked",2);n([d()],Jt.prototype,"value",2);n([d({type:Boolean,reflect:!0})],Jt.prototype,"loading",2);n([d({type:Boolean,reflect:!0})],Jt.prototype,"disabled",2);n([C("checked")],Jt.prototype,"handleCheckedChange",1);n([C("disabled")],Jt.prototype,"handleDisabledChange",1);n([C("type")],Jt.prototype,"handleTypeChange",1);Jt.define("sl-menu-item");var Rh=A`
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
`,fs=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.position=50}handleDrag(e){const{width:t}=this.base.getBoundingClientRect(),s=this.localize.dir()==="rtl";e.preventDefault(),di(this.base,{onMove:i=>{this.position=parseFloat(gt(i/t*100,0,100).toFixed(2)),s&&(this.position=100-this.position)},initialEvent:e})}handleKeyDown(e){const t=this.localize.dir()==="ltr",s=this.localize.dir()==="rtl";if(["ArrowLeft","ArrowRight","Home","End"].includes(e.key)){const i=e.shiftKey?10:1;let o=this.position;e.preventDefault(),(t&&e.key==="ArrowLeft"||s&&e.key==="ArrowRight")&&(o-=i),(t&&e.key==="ArrowRight"||s&&e.key==="ArrowLeft")&&(o+=i),e.key==="Home"&&(o=0),e.key==="End"&&(o=100),o=gt(o,0,100),this.position=o}}handlePositionChange(){this.emit("sl-change")}render(){const e=this.localize.dir()==="rtl";return v`
      <div
        part="base"
        id="image-comparer"
        class=${L({"image-comparer":!0,"image-comparer--rtl":e})}
        @keydown=${this.handleKeyDown}
      >
        <div class="image-comparer__image">
          <div part="before" class="image-comparer__before">
            <slot name="before"></slot>
          </div>

          <div
            part="after"
            class="image-comparer__after"
            style=${jt({clipPath:e?`inset(0 0 0 ${100-this.position}%)`:`inset(0 ${100-this.position}% 0 0)`})}
          >
            <slot name="after"></slot>
          </div>
        </div>

        <div
          part="divider"
          class="image-comparer__divider"
          style=${jt({left:e?`${100-this.position}%`:`${this.position}%`})}
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
    `}};fs.styles=[M,Rh];fs.scopedElement={"sl-icon":it};n([T(".image-comparer")],fs.prototype,"base",2);n([T(".image-comparer__handle")],fs.prototype,"handle",2);n([d({type:Number,reflect:!0})],fs.prototype,"position",2);n([C("position",{waitUntilFirstUpdate:!0})],fs.prototype,"handlePositionChange",1);fs.define("sl-image-comparer");var Lh=A`
  :host {
    display: block;
  }
`,sr=new Map;function Mh(e,t="cors"){const s=sr.get(e);if(s!==void 0)return Promise.resolve(s);const i=fetch(e,{mode:t}).then(async o=>{const r={ok:o.ok,status:o.status,html:await o.text()};return sr.set(e,r),r});return sr.set(e,i),i}var Ns=class extends P{constructor(){super(...arguments),this.mode="cors",this.allowScripts=!1}executeScript(e){const t=document.createElement("script");[...e.attributes].forEach(s=>t.setAttribute(s.name,s.value)),t.textContent=e.textContent,e.parentNode.replaceChild(t,e)}async handleSrcChange(){try{const e=this.src,t=await Mh(e,this.mode);if(e!==this.src)return;if(!t.ok){this.emit("sl-error",{detail:{status:t.status}});return}this.innerHTML=t.html,this.allowScripts&&[...this.querySelectorAll("script")].forEach(s=>this.executeScript(s)),this.emit("sl-load")}catch{this.emit("sl-error",{detail:{status:-1}})}}render(){return v`<slot></slot>`}};Ns.styles=[M,Lh];n([d()],Ns.prototype,"src",2);n([d()],Ns.prototype,"mode",2);n([d({attribute:"allow-scripts",type:Boolean})],Ns.prototype,"allowScripts",2);n([C("src")],Ns.prototype,"handleSrcChange",1);Ns.define("sl-include");it.define("sl-icon");wt.define("sl-icon-button");var Eo=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.value=0,this.unit="byte",this.display="short"}render(){if(isNaN(this.value))return"";const e=["","kilo","mega","giga","tera"],t=["","kilo","mega","giga","tera","peta"],s=this.unit==="bit"?e:t,i=Math.max(0,Math.min(Math.floor(Math.log10(this.value)/3),s.length-1)),o=s[i]+this.unit,r=parseFloat((this.value/Math.pow(1e3,i)).toPrecision(3));return this.localize.number(r,{style:"unit",unit:o,unitDisplay:this.display})}};n([d({type:Number})],Eo.prototype,"value",2);n([d()],Eo.prototype,"unit",2);n([d()],Eo.prototype,"display",2);Eo.define("sl-format-bytes");var te=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.date=new Date,this.hourFormat="auto"}render(){const e=new Date(this.date),t=this.hourFormat==="auto"?void 0:this.hourFormat==="12";if(!isNaN(e.getMilliseconds()))return v`
      <time datetime=${e.toISOString()}>
        ${this.localize.date(e,{weekday:this.weekday,era:this.era,year:this.year,month:this.month,day:this.day,hour:this.hour,minute:this.minute,second:this.second,timeZoneName:this.timeZoneName,timeZone:this.timeZone,hour12:t})}
      </time>
    `}};n([d()],te.prototype,"date",2);n([d()],te.prototype,"weekday",2);n([d()],te.prototype,"era",2);n([d()],te.prototype,"year",2);n([d()],te.prototype,"month",2);n([d()],te.prototype,"day",2);n([d()],te.prototype,"hour",2);n([d()],te.prototype,"minute",2);n([d()],te.prototype,"second",2);n([d({attribute:"time-zone-name"})],te.prototype,"timeZoneName",2);n([d({attribute:"time-zone"})],te.prototype,"timeZone",2);n([d({attribute:"hour-format"})],te.prototype,"hourFormat",2);te.define("sl-format-date");var be=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.value=0,this.type="decimal",this.noGrouping=!1,this.currency="USD",this.currencyDisplay="symbol"}render(){return isNaN(this.value)?"":this.localize.number(this.value,{style:this.type,currency:this.currency,currencyDisplay:this.currencyDisplay,useGrouping:!this.noGrouping,minimumIntegerDigits:this.minimumIntegerDigits,minimumFractionDigits:this.minimumFractionDigits,maximumFractionDigits:this.maximumFractionDigits,minimumSignificantDigits:this.minimumSignificantDigits,maximumSignificantDigits:this.maximumSignificantDigits})}};n([d({type:Number})],be.prototype,"value",2);n([d()],be.prototype,"type",2);n([d({attribute:"no-grouping",type:Boolean})],be.prototype,"noGrouping",2);n([d()],be.prototype,"currency",2);n([d({attribute:"currency-display"})],be.prototype,"currencyDisplay",2);n([d({attribute:"minimum-integer-digits",type:Number})],be.prototype,"minimumIntegerDigits",2);n([d({attribute:"minimum-fraction-digits",type:Number})],be.prototype,"minimumFractionDigits",2);n([d({attribute:"maximum-fraction-digits",type:Number})],be.prototype,"maximumFractionDigits",2);n([d({attribute:"minimum-significant-digits",type:Number})],be.prototype,"minimumSignificantDigits",2);n([d({attribute:"maximum-significant-digits",type:Number})],be.prototype,"maximumSignificantDigits",2);be.define("sl-format-number");var Nh=A`
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
`,Oo=class extends P{constructor(){super(...arguments),this.vertical=!1}connectedCallback(){super.connectedCallback(),this.setAttribute("role","separator")}handleVerticalChange(){this.setAttribute("aria-orientation",this.vertical?"vertical":"horizontal")}};Oo.styles=[M,Nh];n([d({type:Boolean,reflect:!0})],Oo.prototype,"vertical",2);n([C("vertical")],Oo.prototype,"handleVerticalChange",1);Oo.define("sl-divider");var Fh=A`
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
`,rn=new WeakMap;function dl(e){let t=rn.get(e);return t||(t=window.getComputedStyle(e,null),rn.set(e,t)),t}function Bh(e){if(typeof e.checkVisibility=="function")return e.checkVisibility({checkOpacity:!1,checkVisibilityCSS:!0});const t=dl(e);return t.visibility!=="hidden"&&t.display!=="none"}function Uh(e){const t=dl(e),{overflowY:s,overflowX:i}=t;return s==="scroll"||i==="scroll"?!0:s!=="auto"||i!=="auto"?!1:e.scrollHeight>e.clientHeight&&s==="auto"||e.scrollWidth>e.clientWidth&&i==="auto"}function Hh(e){const t=e.tagName.toLowerCase(),s=Number(e.getAttribute("tabindex"));return e.hasAttribute("tabindex")&&(isNaN(s)||s<=-1)||e.hasAttribute("disabled")||e.closest("[inert]")||t==="input"&&e.getAttribute("type")==="radio"&&!e.hasAttribute("checked")||!Bh(e)?!1:(t==="audio"||t==="video")&&e.hasAttribute("controls")||e.hasAttribute("tabindex")||e.hasAttribute("contenteditable")&&e.getAttribute("contenteditable")!=="false"||["button","input","select","textarea","a","audio","video","summary","iframe"].includes(t)?!0:Uh(e)}function Vh(e){var t,s;const i=_r(e),o=(t=i[0])!=null?t:null,r=(s=i[i.length-1])!=null?s:null;return{start:o,end:r}}function jh(e,t){var s;return((s=e.getRootNode({composed:!0}))==null?void 0:s.host)!==t}function _r(e){const t=new WeakMap,s=[];function i(o){if(o instanceof Element){if(o.hasAttribute("inert")||o.closest("[inert]")||t.has(o))return;t.set(o,!0),!s.includes(o)&&Hh(o)&&s.push(o),o instanceof HTMLSlotElement&&jh(o,e)&&o.assignedElements({flatten:!0}).forEach(r=>{i(r)}),o.shadowRoot!==null&&o.shadowRoot.mode==="open"&&i(o.shadowRoot)}for(const r of o.children)i(r)}return i(e),s.sort((o,r)=>{const a=Number(o.getAttribute("tabindex"))||0;return(Number(r.getAttribute("tabindex"))||0)-a})}function*qr(e=document.activeElement){e!=null&&(yield e,"shadowRoot"in e&&e.shadowRoot&&e.shadowRoot.mode!=="closed"&&(yield*Dc(qr(e.shadowRoot.activeElement))))}function Wh(){return[...qr()].pop()}var Qs=[],hl=class{constructor(e){this.tabDirection="forward",this.handleFocusIn=()=>{this.isActive()&&this.checkFocus()},this.handleKeyDown=t=>{var s;if(t.key!=="Tab"||this.isExternalActivated||!this.isActive())return;const i=Wh();if(this.previousFocus=i,this.previousFocus&&this.possiblyHasTabbableChildren(this.previousFocus))return;t.shiftKey?this.tabDirection="backward":this.tabDirection="forward";const o=_r(this.element);let r=o.findIndex(l=>l===i);this.previousFocus=this.currentFocus;const a=this.tabDirection==="forward"?1:-1;for(;;){r+a>=o.length?r=0:r+a<0?r=o.length-1:r+=a,this.previousFocus=this.currentFocus;const l=o[r];if(this.tabDirection==="backward"&&this.previousFocus&&this.possiblyHasTabbableChildren(this.previousFocus)||l&&this.possiblyHasTabbableChildren(l))return;t.preventDefault(),this.currentFocus=l,(s=this.currentFocus)==null||s.focus({preventScroll:!1});const c=[...qr()];if(c.includes(this.currentFocus)||!c.includes(this.previousFocus))break}setTimeout(()=>this.checkFocus())},this.handleKeyUp=()=>{this.tabDirection="forward"},this.element=e,this.elementsWithTabbableControls=["iframe"]}activate(){Qs.push(this.element),document.addEventListener("focusin",this.handleFocusIn),document.addEventListener("keydown",this.handleKeyDown),document.addEventListener("keyup",this.handleKeyUp)}deactivate(){Qs=Qs.filter(e=>e!==this.element),this.currentFocus=null,document.removeEventListener("focusin",this.handleFocusIn),document.removeEventListener("keydown",this.handleKeyDown),document.removeEventListener("keyup",this.handleKeyUp)}isActive(){return Qs[Qs.length-1]===this.element}activateExternal(){this.isExternalActivated=!0}deactivateExternal(){this.isExternalActivated=!1}checkFocus(){if(this.isActive()&&!this.isExternalActivated){const e=_r(this.element);if(!this.element.matches(":focus-within")){const t=e[0],s=e[e.length-1],i=this.tabDirection==="forward"?t:s;typeof(i==null?void 0:i.focus)=="function"&&(this.currentFocus=i,i.focus({preventScroll:!1}))}}}possiblyHasTabbableChildren(e){return this.elementsWithTabbableControls.includes(e.tagName.toLowerCase())||e.hasAttribute("controls")}};function an(e){return e.charAt(0).toUpperCase()+e.slice(1)}var ee=class extends P{constructor(){super(...arguments),this.hasSlotController=new Ht(this,"footer"),this.localize=new K(this),this.modal=new hl(this),this.open=!1,this.label="",this.placement="end",this.contained=!1,this.noHeader=!1,this.handleDocumentKeyDown=e=>{this.contained||e.key==="Escape"&&this.modal.isActive()&&this.open&&(e.stopImmediatePropagation(),this.requestClose("keyboard"))}}firstUpdated(){this.drawer.hidden=!this.open,this.open&&(this.addOpenListeners(),this.contained||(this.modal.activate(),hi(this)))}disconnectedCallback(){var e;super.disconnectedCallback(),ui(this),(e=this.closeWatcher)==null||e.destroy()}requestClose(e){if(this.emit("sl-request-close",{cancelable:!0,detail:{source:e}}).defaultPrevented){const s=nt(this,"drawer.denyClose",{dir:this.localize.dir()});ht(this.panel,s.keyframes,s.options);return}this.hide()}addOpenListeners(){var e;"CloseWatcher"in window?((e=this.closeWatcher)==null||e.destroy(),this.contained||(this.closeWatcher=new CloseWatcher,this.closeWatcher.onclose=()=>this.requestClose("keyboard"))):document.addEventListener("keydown",this.handleDocumentKeyDown)}removeOpenListeners(){var e;document.removeEventListener("keydown",this.handleDocumentKeyDown),(e=this.closeWatcher)==null||e.destroy()}async handleOpenChange(){if(this.open){this.emit("sl-show"),this.addOpenListeners(),this.originalTrigger=document.activeElement,this.contained||(this.modal.activate(),hi(this));const e=this.querySelector("[autofocus]");e&&e.removeAttribute("autofocus"),await Promise.all([bt(this.drawer),bt(this.overlay)]),this.drawer.hidden=!1,requestAnimationFrame(()=>{this.emit("sl-initial-focus",{cancelable:!0}).defaultPrevented||(e?e.focus({preventScroll:!0}):this.panel.focus({preventScroll:!0})),e&&e.setAttribute("autofocus","")});const t=nt(this,`drawer.show${an(this.placement)}`,{dir:this.localize.dir()}),s=nt(this,"drawer.overlay.show",{dir:this.localize.dir()});await Promise.all([ht(this.panel,t.keyframes,t.options),ht(this.overlay,s.keyframes,s.options)]),this.emit("sl-after-show")}else{this.emit("sl-hide"),this.removeOpenListeners(),this.contained||(this.modal.deactivate(),ui(this)),await Promise.all([bt(this.drawer),bt(this.overlay)]);const e=nt(this,`drawer.hide${an(this.placement)}`,{dir:this.localize.dir()}),t=nt(this,"drawer.overlay.hide",{dir:this.localize.dir()});await Promise.all([ht(this.overlay,t.keyframes,t.options).then(()=>{this.overlay.hidden=!0}),ht(this.panel,e.keyframes,e.options).then(()=>{this.panel.hidden=!0})]),this.drawer.hidden=!0,this.overlay.hidden=!1,this.panel.hidden=!1;const s=this.originalTrigger;typeof(s==null?void 0:s.focus)=="function"&&setTimeout(()=>s.focus()),this.emit("sl-after-hide")}}handleNoModalChange(){this.open&&!this.contained&&(this.modal.activate(),hi(this)),this.open&&this.contained&&(this.modal.deactivate(),ui(this))}async show(){if(!this.open)return this.open=!0,Bt(this,"sl-after-show")}async hide(){if(this.open)return this.open=!1,Bt(this,"sl-after-hide")}render(){return v`
      <div
        part="base"
        class=${L({drawer:!0,"drawer--open":this.open,"drawer--top":this.placement==="top","drawer--end":this.placement==="end","drawer--bottom":this.placement==="bottom","drawer--start":this.placement==="start","drawer--contained":this.contained,"drawer--fixed":!this.contained,"drawer--rtl":this.localize.dir()==="rtl","drawer--has-footer":this.hasSlotController.test("footer")})}
      >
        <div part="overlay" class="drawer__overlay" @click=${()=>this.requestClose("overlay")} tabindex="-1"></div>

        <div
          part="panel"
          class="drawer__panel"
          role="dialog"
          aria-modal="true"
          aria-hidden=${this.open?"false":"true"}
          aria-label=${O(this.noHeader?this.label:void 0)}
          aria-labelledby=${O(this.noHeader?void 0:"title")}
          tabindex="0"
        >
          ${this.noHeader?"":v`
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
    `}};ee.styles=[M,Fh];ee.dependencies={"sl-icon-button":wt};n([T(".drawer")],ee.prototype,"drawer",2);n([T(".drawer__panel")],ee.prototype,"panel",2);n([T(".drawer__overlay")],ee.prototype,"overlay",2);n([d({type:Boolean,reflect:!0})],ee.prototype,"open",2);n([d({reflect:!0})],ee.prototype,"label",2);n([d({reflect:!0})],ee.prototype,"placement",2);n([d({type:Boolean,reflect:!0})],ee.prototype,"contained",2);n([d({attribute:"no-header",type:Boolean,reflect:!0})],ee.prototype,"noHeader",2);n([C("open",{waitUntilFirstUpdate:!0})],ee.prototype,"handleOpenChange",1);n([C("contained",{waitUntilFirstUpdate:!0})],ee.prototype,"handleNoModalChange",1);Z("drawer.showTop",{keyframes:[{opacity:0,translate:"0 -100%"},{opacity:1,translate:"0 0"}],options:{duration:250,easing:"ease"}});Z("drawer.hideTop",{keyframes:[{opacity:1,translate:"0 0"},{opacity:0,translate:"0 -100%"}],options:{duration:250,easing:"ease"}});Z("drawer.showEnd",{keyframes:[{opacity:0,translate:"100%"},{opacity:1,translate:"0"}],rtlKeyframes:[{opacity:0,translate:"-100%"},{opacity:1,translate:"0"}],options:{duration:250,easing:"ease"}});Z("drawer.hideEnd",{keyframes:[{opacity:1,translate:"0"},{opacity:0,translate:"100%"}],rtlKeyframes:[{opacity:1,translate:"0"},{opacity:0,translate:"-100%"}],options:{duration:250,easing:"ease"}});Z("drawer.showBottom",{keyframes:[{opacity:0,translate:"0 100%"},{opacity:1,translate:"0 0"}],options:{duration:250,easing:"ease"}});Z("drawer.hideBottom",{keyframes:[{opacity:1,translate:"0 0"},{opacity:0,translate:"0 100%"}],options:{duration:250,easing:"ease"}});Z("drawer.showStart",{keyframes:[{opacity:0,translate:"-100%"},{opacity:1,translate:"0"}],rtlKeyframes:[{opacity:0,translate:"100%"},{opacity:1,translate:"0"}],options:{duration:250,easing:"ease"}});Z("drawer.hideStart",{keyframes:[{opacity:1,translate:"0"},{opacity:0,translate:"-100%"}],rtlKeyframes:[{opacity:1,translate:"0"},{opacity:0,translate:"100%"}],options:{duration:250,easing:"ease"}});Z("drawer.denyClose",{keyframes:[{scale:1},{scale:1.01},{scale:1}],options:{duration:250}});Z("drawer.overlay.show",{keyframes:[{opacity:0},{opacity:1}],options:{duration:250}});Z("drawer.overlay.hide",{keyframes:[{opacity:1},{opacity:0}],options:{duration:250}});ee.define("sl-drawer");var qh=A`
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
`,Ot=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.open=!1,this.placement="bottom-start",this.disabled=!1,this.stayOpenOnSelect=!1,this.distance=0,this.skidding=0,this.hoist=!1,this.sync=void 0,this.handleKeyDown=e=>{this.open&&e.key==="Escape"&&(e.stopPropagation(),this.hide(),this.focusOnTrigger())},this.handleDocumentKeyDown=e=>{var t;if(e.key==="Escape"&&this.open&&!this.closeWatcher){e.stopPropagation(),this.focusOnTrigger(),this.hide();return}if(e.key==="Tab"){if(this.open&&((t=document.activeElement)==null?void 0:t.tagName.toLowerCase())==="sl-menu-item"){e.preventDefault(),this.hide(),this.focusOnTrigger();return}setTimeout(()=>{var s,i,o;const r=((s=this.containingElement)==null?void 0:s.getRootNode())instanceof ShadowRoot?(o=(i=document.activeElement)==null?void 0:i.shadowRoot)==null?void 0:o.activeElement:document.activeElement;(!this.containingElement||(r==null?void 0:r.closest(this.containingElement.tagName.toLowerCase()))!==this.containingElement)&&this.hide()})}},this.handleDocumentMouseDown=e=>{const t=e.composedPath();this.containingElement&&!t.includes(this.containingElement)&&this.hide()},this.handlePanelSelect=e=>{const t=e.target;!this.stayOpenOnSelect&&t.tagName.toLowerCase()==="sl-menu"&&(this.hide(),this.focusOnTrigger())}}connectedCallback(){super.connectedCallback(),this.containingElement||(this.containingElement=this)}firstUpdated(){this.panel.hidden=!this.open,this.open&&(this.addOpenListeners(),this.popup.active=!0)}disconnectedCallback(){super.disconnectedCallback(),this.removeOpenListeners(),this.hide()}focusOnTrigger(){const e=this.trigger.assignedElements({flatten:!0})[0];typeof(e==null?void 0:e.focus)=="function"&&e.focus()}getMenu(){return this.panel.assignedElements({flatten:!0}).find(e=>e.tagName.toLowerCase()==="sl-menu")}handleTriggerClick(){this.open?this.hide():(this.show(),this.focusOnTrigger())}async handleTriggerKeyDown(e){if([" ","Enter"].includes(e.key)){e.preventDefault(),this.handleTriggerClick();return}const t=this.getMenu();if(t){const s=t.getAllItems(),i=s[0],o=s[s.length-1];["ArrowDown","ArrowUp","Home","End"].includes(e.key)&&(e.preventDefault(),this.open||(this.show(),await this.updateComplete),s.length>0&&this.updateComplete.then(()=>{(e.key==="ArrowDown"||e.key==="Home")&&(t.setCurrentItem(i),i.focus()),(e.key==="ArrowUp"||e.key==="End")&&(t.setCurrentItem(o),o.focus())}))}}handleTriggerKeyUp(e){e.key===" "&&e.preventDefault()}handleTriggerSlotChange(){this.updateAccessibleTrigger()}updateAccessibleTrigger(){const t=this.trigger.assignedElements({flatten:!0}).find(i=>Vh(i).start);let s;if(t){switch(t.tagName.toLowerCase()){case"sl-button":case"sl-icon-button":s=t.button;break;default:s=t}s.setAttribute("aria-haspopup","true"),s.setAttribute("aria-expanded",this.open?"true":"false")}}async show(){if(!this.open)return this.open=!0,Bt(this,"sl-after-show")}async hide(){if(this.open)return this.open=!1,Bt(this,"sl-after-hide")}reposition(){this.popup.reposition()}addOpenListeners(){var e;this.panel.addEventListener("sl-select",this.handlePanelSelect),"CloseWatcher"in window?((e=this.closeWatcher)==null||e.destroy(),this.closeWatcher=new CloseWatcher,this.closeWatcher.onclose=()=>{this.hide(),this.focusOnTrigger()}):this.panel.addEventListener("keydown",this.handleKeyDown),document.addEventListener("keydown",this.handleDocumentKeyDown),document.addEventListener("mousedown",this.handleDocumentMouseDown)}removeOpenListeners(){var e;this.panel&&(this.panel.removeEventListener("sl-select",this.handlePanelSelect),this.panel.removeEventListener("keydown",this.handleKeyDown)),document.removeEventListener("keydown",this.handleDocumentKeyDown),document.removeEventListener("mousedown",this.handleDocumentMouseDown),(e=this.closeWatcher)==null||e.destroy()}async handleOpenChange(){if(this.disabled){this.open=!1;return}if(this.updateAccessibleTrigger(),this.open){this.emit("sl-show"),this.addOpenListeners(),await bt(this),this.panel.hidden=!1,this.popup.active=!0;const{keyframes:e,options:t}=nt(this,"dropdown.show",{dir:this.localize.dir()});await ht(this.popup.popup,e,t),this.emit("sl-after-show")}else{this.emit("sl-hide"),this.removeOpenListeners(),await bt(this);const{keyframes:e,options:t}=nt(this,"dropdown.hide",{dir:this.localize.dir()});await ht(this.popup.popup,e,t),this.panel.hidden=!0,this.popup.active=!1,this.emit("sl-after-hide")}}render(){return v`
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
        sync=${O(this.sync?this.sync:void 0)}
        class=${L({dropdown:!0,"dropdown--open":this.open})}
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
    `}};Ot.styles=[M,qh];Ot.dependencies={"sl-popup":X};n([T(".dropdown")],Ot.prototype,"popup",2);n([T(".dropdown__trigger")],Ot.prototype,"trigger",2);n([T(".dropdown__panel")],Ot.prototype,"panel",2);n([d({type:Boolean,reflect:!0})],Ot.prototype,"open",2);n([d({reflect:!0})],Ot.prototype,"placement",2);n([d({type:Boolean,reflect:!0})],Ot.prototype,"disabled",2);n([d({attribute:"stay-open-on-select",type:Boolean,reflect:!0})],Ot.prototype,"stayOpenOnSelect",2);n([d({attribute:!1})],Ot.prototype,"containingElement",2);n([d({type:Number})],Ot.prototype,"distance",2);n([d({type:Number})],Ot.prototype,"skidding",2);n([d({type:Boolean})],Ot.prototype,"hoist",2);n([d({reflect:!0})],Ot.prototype,"sync",2);n([C("open",{waitUntilFirstUpdate:!0})],Ot.prototype,"handleOpenChange",1);Z("dropdown.show",{keyframes:[{opacity:0,scale:.9},{opacity:1,scale:1}],options:{duration:100,easing:"ease"}});Z("dropdown.hide",{keyframes:[{opacity:1,scale:1},{opacity:0,scale:.9}],options:{duration:100,easing:"ease"}});Ot.define("sl-dropdown");var Kh=A`
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
`,$t=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.isCopying=!1,this.status="rest",this.value="",this.from="",this.disabled=!1,this.copyLabel="",this.successLabel="",this.errorLabel="",this.feedbackDuration=1e3,this.tooltipPlacement="top",this.hoist=!1}async handleCopy(){if(this.disabled||this.isCopying)return;this.isCopying=!0;let e=this.value;if(this.from){const t=this.getRootNode(),s=this.from.includes("."),i=this.from.includes("[")&&this.from.includes("]");let o=this.from,r="";s?[o,r]=this.from.trim().split("."):i&&([o,r]=this.from.trim().replace(/\]$/,"").split("["));const a="getElementById"in t?t.getElementById(o):null;a?i?e=a.getAttribute(r)||"":s?e=a[r]||"":e=a.textContent||"":(this.showStatus("error"),this.emit("sl-error"))}if(!e)this.showStatus("error"),this.emit("sl-error");else try{await navigator.clipboard.writeText(e),this.showStatus("success"),this.emit("sl-copy",{detail:{value:e}})}catch{this.showStatus("error"),this.emit("sl-error")}}async showStatus(e){const t=this.copyLabel||this.localize.term("copy"),s=this.successLabel||this.localize.term("copied"),i=this.errorLabel||this.localize.term("error"),o=e==="success"?this.successIcon:this.errorIcon,r=nt(this,"copy.in",{dir:"ltr"}),a=nt(this,"copy.out",{dir:"ltr"});this.tooltip.content=e==="success"?s:i,await this.copyIcon.animate(a.keyframes,a.options).finished,this.copyIcon.hidden=!0,this.status=e,o.hidden=!1,await o.animate(r.keyframes,r.options).finished,setTimeout(async()=>{await o.animate(a.keyframes,a.options).finished,o.hidden=!0,this.status="rest",this.copyIcon.hidden=!1,await this.copyIcon.animate(r.keyframes,r.options).finished,this.tooltip.content=t,this.isCopying=!1},this.feedbackDuration)}render(){const e=this.copyLabel||this.localize.term("copy");return v`
      <sl-tooltip
        class=${L({"copy-button":!0,"copy-button--success":this.status==="success","copy-button--error":this.status==="error"})}
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
    `}};$t.styles=[M,Kh];$t.dependencies={"sl-icon":it,"sl-tooltip":kt};n([T('slot[name="copy-icon"]')],$t.prototype,"copyIcon",2);n([T('slot[name="success-icon"]')],$t.prototype,"successIcon",2);n([T('slot[name="error-icon"]')],$t.prototype,"errorIcon",2);n([T("sl-tooltip")],$t.prototype,"tooltip",2);n([E()],$t.prototype,"isCopying",2);n([E()],$t.prototype,"status",2);n([d()],$t.prototype,"value",2);n([d()],$t.prototype,"from",2);n([d({type:Boolean,reflect:!0})],$t.prototype,"disabled",2);n([d({attribute:"copy-label"})],$t.prototype,"copyLabel",2);n([d({attribute:"success-label"})],$t.prototype,"successLabel",2);n([d({attribute:"error-label"})],$t.prototype,"errorLabel",2);n([d({attribute:"feedback-duration",type:Number})],$t.prototype,"feedbackDuration",2);n([d({attribute:"tooltip-placement"})],$t.prototype,"tooltipPlacement",2);n([d({type:Boolean})],$t.prototype,"hoist",2);Z("copy.in",{keyframes:[{scale:".25",opacity:".25"},{scale:"1",opacity:"1"}],options:{duration:100}});Z("copy.out",{keyframes:[{scale:"1",opacity:"1"},{scale:".25",opacity:"0"}],options:{duration:100}});$t.define("sl-copy-button");var Yh=A`
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
`,ve=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.open=!1,this.disabled=!1}firstUpdated(){this.body.style.height=this.open?"auto":"0",this.open&&(this.details.open=!0),this.detailsObserver=new MutationObserver(e=>{for(const t of e)t.type==="attributes"&&t.attributeName==="open"&&(this.details.open?this.show():this.hide())}),this.detailsObserver.observe(this.details,{attributes:!0})}disconnectedCallback(){var e;super.disconnectedCallback(),(e=this.detailsObserver)==null||e.disconnect()}handleSummaryClick(e){e.preventDefault(),this.disabled||(this.open?this.hide():this.show(),this.header.focus())}handleSummaryKeyDown(e){(e.key==="Enter"||e.key===" ")&&(e.preventDefault(),this.open?this.hide():this.show()),(e.key==="ArrowUp"||e.key==="ArrowLeft")&&(e.preventDefault(),this.hide()),(e.key==="ArrowDown"||e.key==="ArrowRight")&&(e.preventDefault(),this.show())}async handleOpenChange(){if(this.open){if(this.details.open=!0,this.emit("sl-show",{cancelable:!0}).defaultPrevented){this.open=!1,this.details.open=!1;return}await bt(this.body);const{keyframes:t,options:s}=nt(this,"details.show",{dir:this.localize.dir()});await ht(this.body,lo(t,this.body.scrollHeight),s),this.body.style.height="auto",this.emit("sl-after-show")}else{if(this.emit("sl-hide",{cancelable:!0}).defaultPrevented){this.details.open=!0,this.open=!0;return}await bt(this.body);const{keyframes:t,options:s}=nt(this,"details.hide",{dir:this.localize.dir()});await ht(this.body,lo(t,this.body.scrollHeight),s),this.body.style.height="auto",this.details.open=!1,this.emit("sl-after-hide")}}async show(){if(!(this.open||this.disabled))return this.open=!0,Bt(this,"sl-after-show")}async hide(){if(!(!this.open||this.disabled))return this.open=!1,Bt(this,"sl-after-hide")}render(){const e=this.localize.dir()==="rtl";return v`
      <details
        part="base"
        class=${L({details:!0,"details--open":this.open,"details--disabled":this.disabled,"details--rtl":e})}
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
    `}};ve.styles=[M,Yh];ve.dependencies={"sl-icon":it};n([T(".details")],ve.prototype,"details",2);n([T(".details__header")],ve.prototype,"header",2);n([T(".details__body")],ve.prototype,"body",2);n([T(".details__expand-icon-slot")],ve.prototype,"expandIconSlot",2);n([d({type:Boolean,reflect:!0})],ve.prototype,"open",2);n([d()],ve.prototype,"summary",2);n([d({type:Boolean,reflect:!0})],ve.prototype,"disabled",2);n([C("open",{waitUntilFirstUpdate:!0})],ve.prototype,"handleOpenChange",1);Z("details.show",{keyframes:[{height:"0",opacity:"0"},{height:"auto",opacity:"1"}],options:{duration:250,easing:"linear"}});Z("details.hide",{keyframes:[{height:"auto",opacity:"1"},{height:"0",opacity:"0"}],options:{duration:250,easing:"linear"}});ve.define("sl-details");var Gh=A`
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
`,Pe=class extends P{constructor(){super(...arguments),this.hasSlotController=new Ht(this,"footer"),this.localize=new K(this),this.modal=new hl(this),this.open=!1,this.label="",this.noHeader=!1,this.handleDocumentKeyDown=e=>{e.key==="Escape"&&this.modal.isActive()&&this.open&&(e.stopPropagation(),this.requestClose("keyboard"))}}firstUpdated(){this.dialog.hidden=!this.open,this.open&&(this.addOpenListeners(),this.modal.activate(),hi(this))}disconnectedCallback(){var e;super.disconnectedCallback(),this.modal.deactivate(),ui(this),(e=this.closeWatcher)==null||e.destroy()}requestClose(e){if(this.emit("sl-request-close",{cancelable:!0,detail:{source:e}}).defaultPrevented){const s=nt(this,"dialog.denyClose",{dir:this.localize.dir()});ht(this.panel,s.keyframes,s.options);return}this.hide()}addOpenListeners(){var e;"CloseWatcher"in window?((e=this.closeWatcher)==null||e.destroy(),this.closeWatcher=new CloseWatcher,this.closeWatcher.onclose=()=>this.requestClose("keyboard")):document.addEventListener("keydown",this.handleDocumentKeyDown)}removeOpenListeners(){var e;(e=this.closeWatcher)==null||e.destroy(),document.removeEventListener("keydown",this.handleDocumentKeyDown)}async handleOpenChange(){if(this.open){this.emit("sl-show"),this.addOpenListeners(),this.originalTrigger=document.activeElement,this.modal.activate(),hi(this);const e=this.querySelector("[autofocus]");e&&e.removeAttribute("autofocus"),await Promise.all([bt(this.dialog),bt(this.overlay)]),this.dialog.hidden=!1,requestAnimationFrame(()=>{this.emit("sl-initial-focus",{cancelable:!0}).defaultPrevented||(e?e.focus({preventScroll:!0}):this.panel.focus({preventScroll:!0})),e&&e.setAttribute("autofocus","")});const t=nt(this,"dialog.show",{dir:this.localize.dir()}),s=nt(this,"dialog.overlay.show",{dir:this.localize.dir()});await Promise.all([ht(this.panel,t.keyframes,t.options),ht(this.overlay,s.keyframes,s.options)]),this.emit("sl-after-show")}else{this.emit("sl-hide"),this.removeOpenListeners(),this.modal.deactivate(),await Promise.all([bt(this.dialog),bt(this.overlay)]);const e=nt(this,"dialog.hide",{dir:this.localize.dir()}),t=nt(this,"dialog.overlay.hide",{dir:this.localize.dir()});await Promise.all([ht(this.overlay,t.keyframes,t.options).then(()=>{this.overlay.hidden=!0}),ht(this.panel,e.keyframes,e.options).then(()=>{this.panel.hidden=!0})]),this.dialog.hidden=!0,this.overlay.hidden=!1,this.panel.hidden=!1,ui(this);const s=this.originalTrigger;typeof(s==null?void 0:s.focus)=="function"&&setTimeout(()=>s.focus()),this.emit("sl-after-hide")}}async show(){if(!this.open)return this.open=!0,Bt(this,"sl-after-show")}async hide(){if(this.open)return this.open=!1,Bt(this,"sl-after-hide")}render(){return v`
      <div
        part="base"
        class=${L({dialog:!0,"dialog--open":this.open,"dialog--has-footer":this.hasSlotController.test("footer")})}
      >
        <div part="overlay" class="dialog__overlay" @click=${()=>this.requestClose("overlay")} tabindex="-1"></div>

        <div
          part="panel"
          class="dialog__panel"
          role="dialog"
          aria-modal="true"
          aria-hidden=${this.open?"false":"true"}
          aria-label=${O(this.noHeader?this.label:void 0)}
          aria-labelledby=${O(this.noHeader?void 0:"title")}
          tabindex="-1"
        >
          ${this.noHeader?"":v`
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
    `}};Pe.styles=[M,Gh];Pe.dependencies={"sl-icon-button":wt};n([T(".dialog")],Pe.prototype,"dialog",2);n([T(".dialog__panel")],Pe.prototype,"panel",2);n([T(".dialog__overlay")],Pe.prototype,"overlay",2);n([d({type:Boolean,reflect:!0})],Pe.prototype,"open",2);n([d({reflect:!0})],Pe.prototype,"label",2);n([d({attribute:"no-header",type:Boolean,reflect:!0})],Pe.prototype,"noHeader",2);n([C("open",{waitUntilFirstUpdate:!0})],Pe.prototype,"handleOpenChange",1);Z("dialog.show",{keyframes:[{opacity:0,scale:.8},{opacity:1,scale:1}],options:{duration:250,easing:"ease"}});Z("dialog.hide",{keyframes:[{opacity:1,scale:1},{opacity:0,scale:.8}],options:{duration:250,easing:"ease"}});Z("dialog.denyClose",{keyframes:[{scale:1},{scale:1.02},{scale:1}],options:{duration:250}});Z("dialog.overlay.show",{keyframes:[{opacity:0},{opacity:1}],options:{duration:250}});Z("dialog.overlay.hide",{keyframes:[{opacity:1},{opacity:0}],options:{duration:250}});Pe.define("sl-dialog");yt.define("sl-checkbox");var Xh=A`
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
`,Q=class extends P{constructor(){super(...arguments),this.formControlController=new Fe(this,{assumeInteractionOn:["click"]}),this.hasSlotController=new Ht(this,"[default]","prefix","suffix"),this.localize=new K(this),this.hasFocus=!1,this.invalid=!1,this.title="",this.variant="default",this.size="medium",this.caret=!1,this.disabled=!1,this.loading=!1,this.outline=!1,this.pill=!1,this.circle=!1,this.type="button",this.name="",this.value="",this.href="",this.rel="noreferrer noopener"}get validity(){return this.isButton()?this.button.validity:So}get validationMessage(){return this.isButton()?this.button.validationMessage:""}firstUpdated(){this.isButton()&&this.formControlController.updateValidity()}handleBlur(){this.hasFocus=!1,this.emit("sl-blur")}handleFocus(){this.hasFocus=!0,this.emit("sl-focus")}handleClick(){this.type==="submit"&&this.formControlController.submit(this),this.type==="reset"&&this.formControlController.reset(this)}handleInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}isButton(){return!this.href}isLink(){return!!this.href}handleDisabledChange(){this.isButton()&&this.formControlController.setValidity(this.disabled)}click(){this.button.click()}focus(e){this.button.focus(e)}blur(){this.button.blur()}checkValidity(){return this.isButton()?this.button.checkValidity():!0}getForm(){return this.formControlController.getForm()}reportValidity(){return this.isButton()?this.button.reportValidity():!0}setCustomValidity(e){this.isButton()&&(this.button.setCustomValidity(e),this.formControlController.updateValidity())}render(){const e=this.isLink(),t=e?co`a`:co`button`;return ci`
      <${t}
        part="base"
        class=${L({button:!0,"button--default":this.variant==="default","button--primary":this.variant==="primary","button--success":this.variant==="success","button--neutral":this.variant==="neutral","button--warning":this.variant==="warning","button--danger":this.variant==="danger","button--text":this.variant==="text","button--small":this.size==="small","button--medium":this.size==="medium","button--large":this.size==="large","button--caret":this.caret,"button--circle":this.circle,"button--disabled":this.disabled,"button--focused":this.hasFocus,"button--loading":this.loading,"button--standard":!this.outline,"button--outline":this.outline,"button--pill":this.pill,"button--rtl":this.localize.dir()==="rtl","button--has-label":this.hasSlotController.test("[default]"),"button--has-prefix":this.hasSlotController.test("prefix"),"button--has-suffix":this.hasSlotController.test("suffix")})}
        ?disabled=${O(e?void 0:this.disabled)}
        type=${O(e?void 0:this.type)}
        title=${this.title}
        name=${O(e?void 0:this.name)}
        value=${O(e?void 0:this.value)}
        href=${O(e&&!this.disabled?this.href:void 0)}
        target=${O(e?this.target:void 0)}
        download=${O(e?this.download:void 0)}
        rel=${O(e?this.rel:void 0)}
        role=${O(e?void 0:"button")}
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
    `}};Q.styles=[M,ol];Q.dependencies={"sl-icon":it,"sl-spinner":zi};n([T(".button")],Q.prototype,"button",2);n([E()],Q.prototype,"hasFocus",2);n([E()],Q.prototype,"invalid",2);n([d()],Q.prototype,"title",2);n([d({reflect:!0})],Q.prototype,"variant",2);n([d({reflect:!0})],Q.prototype,"size",2);n([d({type:Boolean,reflect:!0})],Q.prototype,"caret",2);n([d({type:Boolean,reflect:!0})],Q.prototype,"disabled",2);n([d({type:Boolean,reflect:!0})],Q.prototype,"loading",2);n([d({type:Boolean,reflect:!0})],Q.prototype,"outline",2);n([d({type:Boolean,reflect:!0})],Q.prototype,"pill",2);n([d({type:Boolean,reflect:!0})],Q.prototype,"circle",2);n([d()],Q.prototype,"type",2);n([d()],Q.prototype,"name",2);n([d()],Q.prototype,"value",2);n([d()],Q.prototype,"href",2);n([d()],Q.prototype,"target",2);n([d()],Q.prototype,"rel",2);n([d()],Q.prototype,"download",2);n([d()],Q.prototype,"form",2);n([d({attribute:"formaction"})],Q.prototype,"formAction",2);n([d({attribute:"formenctype"})],Q.prototype,"formEnctype",2);n([d({attribute:"formmethod"})],Q.prototype,"formMethod",2);n([d({attribute:"formnovalidate",type:Boolean})],Q.prototype,"formNoValidate",2);n([d({attribute:"formtarget"})],Q.prototype,"formTarget",2);n([C("disabled",{waitUntilFirstUpdate:!0})],Q.prototype,"handleDisabledChange",1);function Tt(e,t){Zh(e)&&(e="100%");const s=Qh(e);return e=t===360?e:Math.min(t,Math.max(0,parseFloat(e))),s&&(e=parseInt(String(e*t),10)/100),Math.abs(e-t)<1e-6?1:(t===360?e=(e<0?e%t+t:e%t)/parseFloat(String(t)):e=e%t/parseFloat(String(t)),e)}function Yi(e){return Math.min(1,Math.max(0,e))}function Zh(e){return typeof e=="string"&&e.indexOf(".")!==-1&&parseFloat(e)===1}function Qh(e){return typeof e=="string"&&e.indexOf("%")!==-1}function ul(e){return e=parseFloat(e),(isNaN(e)||e<0||e>1)&&(e=1),e}function Gi(e){return Number(e)<=1?`${Number(e)*100}%`:e}function ss(e){return e.length===1?"0"+e:String(e)}function Jh(e,t,s){return{r:Tt(e,255)*255,g:Tt(t,255)*255,b:Tt(s,255)*255}}function nn(e,t,s){e=Tt(e,255),t=Tt(t,255),s=Tt(s,255);const i=Math.max(e,t,s),o=Math.min(e,t,s);let r=0,a=0;const l=(i+o)/2;if(i===o)a=0,r=0;else{const c=i-o;switch(a=l>.5?c/(2-i-o):c/(i+o),i){case e:r=(t-s)/c+(t<s?6:0);break;case t:r=(s-e)/c+2;break;case s:r=(e-t)/c+4;break}r/=6}return{h:r,s:a,l}}function ir(e,t,s){return s<0&&(s+=1),s>1&&(s-=1),s<1/6?e+(t-e)*(6*s):s<1/2?t:s<2/3?e+(t-e)*(2/3-s)*6:e}function tu(e,t,s){let i,o,r;if(e=Tt(e,360),t=Tt(t,100),s=Tt(s,100),t===0)o=s,r=s,i=s;else{const a=s<.5?s*(1+t):s+t-s*t,l=2*s-a;i=ir(l,a,e+1/3),o=ir(l,a,e),r=ir(l,a,e-1/3)}return{r:i*255,g:o*255,b:r*255}}function ln(e,t,s){e=Tt(e,255),t=Tt(t,255),s=Tt(s,255);const i=Math.max(e,t,s),o=Math.min(e,t,s);let r=0;const a=i,l=i-o,c=i===0?0:l/i;if(i===o)r=0;else{switch(i){case e:r=(t-s)/l+(t<s?6:0);break;case t:r=(s-e)/l+2;break;case s:r=(e-t)/l+4;break}r/=6}return{h:r,s:c,v:a}}function eu(e,t,s){e=Tt(e,360)*6,t=Tt(t,100),s=Tt(s,100);const i=Math.floor(e),o=e-i,r=s*(1-t),a=s*(1-o*t),l=s*(1-(1-o)*t),c=i%6,h=[s,a,r,r,l,s][c],u=[l,s,s,a,r,r][c],p=[r,r,l,s,s,a][c];return{r:h*255,g:u*255,b:p*255}}function cn(e,t,s,i){const o=[ss(Math.round(e).toString(16)),ss(Math.round(t).toString(16)),ss(Math.round(s).toString(16))];return i&&o[0].startsWith(o[0].charAt(1))&&o[1].startsWith(o[1].charAt(1))&&o[2].startsWith(o[2].charAt(1))?o[0].charAt(0)+o[1].charAt(0)+o[2].charAt(0):o.join("")}function su(e,t,s,i,o){const r=[ss(Math.round(e).toString(16)),ss(Math.round(t).toString(16)),ss(Math.round(s).toString(16)),ss(ou(i))];return o&&r[0].startsWith(r[0].charAt(1))&&r[1].startsWith(r[1].charAt(1))&&r[2].startsWith(r[2].charAt(1))&&r[3].startsWith(r[3].charAt(1))?r[0].charAt(0)+r[1].charAt(0)+r[2].charAt(0)+r[3].charAt(0):r.join("")}function iu(e,t,s,i){const o=e/100,r=t/100,a=s/100,l=i/100,c=255*(1-o)*(1-l),h=255*(1-r)*(1-l),u=255*(1-a)*(1-l);return{r:c,g:h,b:u}}function dn(e,t,s){let i=1-e/255,o=1-t/255,r=1-s/255,a=Math.min(i,o,r);return a===1?(i=0,o=0,r=0):(i=(i-a)/(1-a)*100,o=(o-a)/(1-a)*100,r=(r-a)/(1-a)*100),a*=100,{c:Math.round(i),m:Math.round(o),y:Math.round(r),k:Math.round(a)}}function ou(e){return Math.round(parseFloat(e)*255).toString(16)}function hn(e){return Yt(e)/255}function Yt(e){return parseInt(e,16)}function ru(e){return{r:e>>16,g:(e&65280)>>8,b:e&255}}const kr={aliceblue:"#f0f8ff",antiquewhite:"#faebd7",aqua:"#00ffff",aquamarine:"#7fffd4",azure:"#f0ffff",beige:"#f5f5dc",bisque:"#ffe4c4",black:"#000000",blanchedalmond:"#ffebcd",blue:"#0000ff",blueviolet:"#8a2be2",brown:"#a52a2a",burlywood:"#deb887",cadetblue:"#5f9ea0",chartreuse:"#7fff00",chocolate:"#d2691e",coral:"#ff7f50",cornflowerblue:"#6495ed",cornsilk:"#fff8dc",crimson:"#dc143c",cyan:"#00ffff",darkblue:"#00008b",darkcyan:"#008b8b",darkgoldenrod:"#b8860b",darkgray:"#a9a9a9",darkgreen:"#006400",darkgrey:"#a9a9a9",darkkhaki:"#bdb76b",darkmagenta:"#8b008b",darkolivegreen:"#556b2f",darkorange:"#ff8c00",darkorchid:"#9932cc",darkred:"#8b0000",darksalmon:"#e9967a",darkseagreen:"#8fbc8f",darkslateblue:"#483d8b",darkslategray:"#2f4f4f",darkslategrey:"#2f4f4f",darkturquoise:"#00ced1",darkviolet:"#9400d3",deeppink:"#ff1493",deepskyblue:"#00bfff",dimgray:"#696969",dimgrey:"#696969",dodgerblue:"#1e90ff",firebrick:"#b22222",floralwhite:"#fffaf0",forestgreen:"#228b22",fuchsia:"#ff00ff",gainsboro:"#dcdcdc",ghostwhite:"#f8f8ff",goldenrod:"#daa520",gold:"#ffd700",gray:"#808080",green:"#008000",greenyellow:"#adff2f",grey:"#808080",honeydew:"#f0fff0",hotpink:"#ff69b4",indianred:"#cd5c5c",indigo:"#4b0082",ivory:"#fffff0",khaki:"#f0e68c",lavenderblush:"#fff0f5",lavender:"#e6e6fa",lawngreen:"#7cfc00",lemonchiffon:"#fffacd",lightblue:"#add8e6",lightcoral:"#f08080",lightcyan:"#e0ffff",lightgoldenrodyellow:"#fafad2",lightgray:"#d3d3d3",lightgreen:"#90ee90",lightgrey:"#d3d3d3",lightpink:"#ffb6c1",lightsalmon:"#ffa07a",lightseagreen:"#20b2aa",lightskyblue:"#87cefa",lightslategray:"#778899",lightslategrey:"#778899",lightsteelblue:"#b0c4de",lightyellow:"#ffffe0",lime:"#00ff00",limegreen:"#32cd32",linen:"#faf0e6",magenta:"#ff00ff",maroon:"#800000",mediumaquamarine:"#66cdaa",mediumblue:"#0000cd",mediumorchid:"#ba55d3",mediumpurple:"#9370db",mediumseagreen:"#3cb371",mediumslateblue:"#7b68ee",mediumspringgreen:"#00fa9a",mediumturquoise:"#48d1cc",mediumvioletred:"#c71585",midnightblue:"#191970",mintcream:"#f5fffa",mistyrose:"#ffe4e1",moccasin:"#ffe4b5",navajowhite:"#ffdead",navy:"#000080",oldlace:"#fdf5e6",olive:"#808000",olivedrab:"#6b8e23",orange:"#ffa500",orangered:"#ff4500",orchid:"#da70d6",palegoldenrod:"#eee8aa",palegreen:"#98fb98",paleturquoise:"#afeeee",palevioletred:"#db7093",papayawhip:"#ffefd5",peachpuff:"#ffdab9",peru:"#cd853f",pink:"#ffc0cb",plum:"#dda0dd",powderblue:"#b0e0e6",purple:"#800080",rebeccapurple:"#663399",red:"#ff0000",rosybrown:"#bc8f8f",royalblue:"#4169e1",saddlebrown:"#8b4513",salmon:"#fa8072",sandybrown:"#f4a460",seagreen:"#2e8b57",seashell:"#fff5ee",sienna:"#a0522d",silver:"#c0c0c0",skyblue:"#87ceeb",slateblue:"#6a5acd",slategray:"#708090",slategrey:"#708090",snow:"#fffafa",springgreen:"#00ff7f",steelblue:"#4682b4",tan:"#d2b48c",teal:"#008080",thistle:"#d8bfd8",tomato:"#ff6347",turquoise:"#40e0d0",violet:"#ee82ee",wheat:"#f5deb3",white:"#ffffff",whitesmoke:"#f5f5f5",yellow:"#ffff00",yellowgreen:"#9acd32"};function au(e){let t={r:0,g:0,b:0},s=1,i=null,o=null,r=null,a=!1,l=!1;return typeof e=="string"&&(e=cu(e)),typeof e=="object"&&(Kt(e.r)&&Kt(e.g)&&Kt(e.b)?(t=Jh(e.r,e.g,e.b),a=!0,l=String(e.r).substr(-1)==="%"?"prgb":"rgb"):Kt(e.h)&&Kt(e.s)&&Kt(e.v)?(i=Gi(e.s),o=Gi(e.v),t=eu(e.h,i,o),a=!0,l="hsv"):Kt(e.h)&&Kt(e.s)&&Kt(e.l)?(i=Gi(e.s),r=Gi(e.l),t=tu(e.h,i,r),a=!0,l="hsl"):Kt(e.c)&&Kt(e.m)&&Kt(e.y)&&Kt(e.k)&&(t=iu(e.c,e.m,e.y,e.k),a=!0,l="cmyk"),Object.prototype.hasOwnProperty.call(e,"a")&&(s=e.a)),s=ul(s),{ok:a,format:e.format||l,r:Math.min(255,Math.max(t.r,0)),g:Math.min(255,Math.max(t.g,0)),b:Math.min(255,Math.max(t.b,0)),a:s}}const nu="[-\\+]?\\d+%?",lu="[-\\+]?\\d*\\.\\d+%?",He="(?:"+lu+")|(?:"+nu+")",or="[\\s|\\(]+("+He+")[,|\\s]+("+He+")[,|\\s]+("+He+")\\s*\\)?",Xi="[\\s|\\(]+("+He+")[,|\\s]+("+He+")[,|\\s]+("+He+")[,|\\s]+("+He+")\\s*\\)?",re={CSS_UNIT:new RegExp(He),rgb:new RegExp("rgb"+or),rgba:new RegExp("rgba"+Xi),hsl:new RegExp("hsl"+or),hsla:new RegExp("hsla"+Xi),hsv:new RegExp("hsv"+or),hsva:new RegExp("hsva"+Xi),cmyk:new RegExp("cmyk"+Xi),hex3:/^#?([0-9a-fA-F]{1})([0-9a-fA-F]{1})([0-9a-fA-F]{1})$/,hex6:/^#?([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})$/,hex4:/^#?([0-9a-fA-F]{1})([0-9a-fA-F]{1})([0-9a-fA-F]{1})([0-9a-fA-F]{1})$/,hex8:/^#?([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})$/};function cu(e){if(e=e.trim().toLowerCase(),e.length===0)return!1;let t=!1;if(kr[e])e=kr[e],t=!0;else if(e==="transparent")return{r:0,g:0,b:0,a:0,format:"name"};let s=re.rgb.exec(e);return s?{r:s[1],g:s[2],b:s[3]}:(s=re.rgba.exec(e),s?{r:s[1],g:s[2],b:s[3],a:s[4]}:(s=re.hsl.exec(e),s?{h:s[1],s:s[2],l:s[3]}:(s=re.hsla.exec(e),s?{h:s[1],s:s[2],l:s[3],a:s[4]}:(s=re.hsv.exec(e),s?{h:s[1],s:s[2],v:s[3]}:(s=re.hsva.exec(e),s?{h:s[1],s:s[2],v:s[3],a:s[4]}:(s=re.cmyk.exec(e),s?{c:s[1],m:s[2],y:s[3],k:s[4]}:(s=re.hex8.exec(e),s?{r:Yt(s[1]),g:Yt(s[2]),b:Yt(s[3]),a:hn(s[4]),format:t?"name":"hex8"}:(s=re.hex6.exec(e),s?{r:Yt(s[1]),g:Yt(s[2]),b:Yt(s[3]),format:t?"name":"hex"}:(s=re.hex4.exec(e),s?{r:Yt(s[1]+s[1]),g:Yt(s[2]+s[2]),b:Yt(s[3]+s[3]),a:hn(s[4]+s[4]),format:t?"name":"hex8"}:(s=re.hex3.exec(e),s?{r:Yt(s[1]+s[1]),g:Yt(s[2]+s[2]),b:Yt(s[3]+s[3]),format:t?"name":"hex"}:!1))))))))))}function Kt(e){return typeof e=="number"?!Number.isNaN(e):re.CSS_UNIT.test(e)}class dt{constructor(t="",s={}){if(t instanceof dt)return t;typeof t=="number"&&(t=ru(t)),this.originalInput=t;const i=au(t);this.originalInput=t,this.r=i.r,this.g=i.g,this.b=i.b,this.a=i.a,this.roundA=Math.round(100*this.a)/100,this.format=s.format??i.format,this.gradientType=s.gradientType,this.r<1&&(this.r=Math.round(this.r)),this.g<1&&(this.g=Math.round(this.g)),this.b<1&&(this.b=Math.round(this.b)),this.isValid=i.ok}isDark(){return this.getBrightness()<128}isLight(){return!this.isDark()}getBrightness(){const t=this.toRgb();return(t.r*299+t.g*587+t.b*114)/1e3}getLuminance(){const t=this.toRgb();let s,i,o;const r=t.r/255,a=t.g/255,l=t.b/255;return r<=.03928?s=r/12.92:s=Math.pow((r+.055)/1.055,2.4),a<=.03928?i=a/12.92:i=Math.pow((a+.055)/1.055,2.4),l<=.03928?o=l/12.92:o=Math.pow((l+.055)/1.055,2.4),.2126*s+.7152*i+.0722*o}getAlpha(){return this.a}setAlpha(t){return this.a=ul(t),this.roundA=Math.round(100*this.a)/100,this}isMonochrome(){const{s:t}=this.toHsl();return t===0}toHsv(){const t=ln(this.r,this.g,this.b);return{h:t.h*360,s:t.s,v:t.v,a:this.a}}toHsvString(){const t=ln(this.r,this.g,this.b),s=Math.round(t.h*360),i=Math.round(t.s*100),o=Math.round(t.v*100);return this.a===1?`hsv(${s}, ${i}%, ${o}%)`:`hsva(${s}, ${i}%, ${o}%, ${this.roundA})`}toHsl(){const t=nn(this.r,this.g,this.b);return{h:t.h*360,s:t.s,l:t.l,a:this.a}}toHslString(){const t=nn(this.r,this.g,this.b),s=Math.round(t.h*360),i=Math.round(t.s*100),o=Math.round(t.l*100);return this.a===1?`hsl(${s}, ${i}%, ${o}%)`:`hsla(${s}, ${i}%, ${o}%, ${this.roundA})`}toHex(t=!1){return cn(this.r,this.g,this.b,t)}toHexString(t=!1){return"#"+this.toHex(t)}toHex8(t=!1){return su(this.r,this.g,this.b,this.a,t)}toHex8String(t=!1){return"#"+this.toHex8(t)}toHexShortString(t=!1){return this.a===1?this.toHexString(t):this.toHex8String(t)}toRgb(){return{r:Math.round(this.r),g:Math.round(this.g),b:Math.round(this.b),a:this.a}}toRgbString(){const t=Math.round(this.r),s=Math.round(this.g),i=Math.round(this.b);return this.a===1?`rgb(${t}, ${s}, ${i})`:`rgba(${t}, ${s}, ${i}, ${this.roundA})`}toPercentageRgb(){const t=s=>`${Math.round(Tt(s,255)*100)}%`;return{r:t(this.r),g:t(this.g),b:t(this.b),a:this.a}}toPercentageRgbString(){const t=s=>Math.round(Tt(s,255)*100);return this.a===1?`rgb(${t(this.r)}%, ${t(this.g)}%, ${t(this.b)}%)`:`rgba(${t(this.r)}%, ${t(this.g)}%, ${t(this.b)}%, ${this.roundA})`}toCmyk(){return{...dn(this.r,this.g,this.b)}}toCmykString(){const{c:t,m:s,y:i,k:o}=dn(this.r,this.g,this.b);return`cmyk(${t}, ${s}, ${i}, ${o})`}toName(){if(this.a===0)return"transparent";if(this.a<1)return!1;const t="#"+cn(this.r,this.g,this.b,!1);for(const[s,i]of Object.entries(kr))if(t===i)return s;return!1}toString(t){const s=!!t;t=t??this.format;let i=!1;const o=this.a<1&&this.a>=0;return!s&&o&&(t.startsWith("hex")||t==="name")?t==="name"&&this.a===0?this.toName():this.toRgbString():(t==="rgb"&&(i=this.toRgbString()),t==="prgb"&&(i=this.toPercentageRgbString()),(t==="hex"||t==="hex6")&&(i=this.toHexString()),t==="hex3"&&(i=this.toHexString(!0)),t==="hex4"&&(i=this.toHex8String(!0)),t==="hex8"&&(i=this.toHex8String()),t==="name"&&(i=this.toName()),t==="hsl"&&(i=this.toHslString()),t==="hsv"&&(i=this.toHsvString()),t==="cmyk"&&(i=this.toCmykString()),i||this.toHexString())}toNumber(){return(Math.round(this.r)<<16)+(Math.round(this.g)<<8)+Math.round(this.b)}clone(){return new dt(this.toString())}lighten(t=10){const s=this.toHsl();return s.l+=t/100,s.l=Yi(s.l),new dt(s)}brighten(t=10){const s=this.toRgb();return s.r=Math.max(0,Math.min(255,s.r-Math.round(255*-(t/100)))),s.g=Math.max(0,Math.min(255,s.g-Math.round(255*-(t/100)))),s.b=Math.max(0,Math.min(255,s.b-Math.round(255*-(t/100)))),new dt(s)}darken(t=10){const s=this.toHsl();return s.l-=t/100,s.l=Yi(s.l),new dt(s)}tint(t=10){return this.mix("white",t)}shade(t=10){return this.mix("black",t)}desaturate(t=10){const s=this.toHsl();return s.s-=t/100,s.s=Yi(s.s),new dt(s)}saturate(t=10){const s=this.toHsl();return s.s+=t/100,s.s=Yi(s.s),new dt(s)}greyscale(){return this.desaturate(100)}spin(t){const s=this.toHsl(),i=(s.h+t)%360;return s.h=i<0?360+i:i,new dt(s)}mix(t,s=50){const i=this.toRgb(),o=new dt(t).toRgb(),r=s/100,a={r:(o.r-i.r)*r+i.r,g:(o.g-i.g)*r+i.g,b:(o.b-i.b)*r+i.b,a:(o.a-i.a)*r+i.a};return new dt(a)}analogous(t=6,s=30){const i=this.toHsl(),o=360/s,r=[this];for(i.h=(i.h-(o*t>>1)+720)%360;--t;)i.h=(i.h+o)%360,r.push(new dt(i));return r}complement(){const t=this.toHsl();return t.h=(t.h+180)%360,new dt(t)}monochromatic(t=6){const s=this.toHsv(),{h:i}=s,{s:o}=s;let{v:r}=s;const a=[],l=1/t;for(;t--;)a.push(new dt({h:i,s:o,v:r})),r=(r+l)%1;return a}splitcomplement(){const t=this.toHsl(),{h:s}=t;return[this,new dt({h:(s+72)%360,s:t.s,l:t.l}),new dt({h:(s+216)%360,s:t.s,l:t.l})]}onBackground(t){const s=this.toRgb(),i=new dt(t).toRgb(),o=s.a+i.a*(1-s.a);return new dt({r:(s.r*s.a+i.r*i.a*(1-s.a))/o,g:(s.g*s.a+i.g*i.a*(1-s.a))/o,b:(s.b*s.a+i.b*i.a*(1-s.a))/o,a:o})}triad(){return this.polyad(3)}tetrad(){return this.polyad(4)}polyad(t){const s=this.toHsl(),{h:i}=s,o=[this],r=360/t;for(let a=1;a<t;a++)o.push(new dt({h:(i+a*r)%360,s:s.s,l:s.l}));return o}equals(t){const s=new dt(t);return this.format==="cmyk"||s.format==="cmyk"?this.toCmykString()===s.toCmykString():this.toRgbString()===s.toRgbString()}}var un="EyeDropper"in window,W=class extends P{constructor(){super(),this.formControlController=new Fe(this),this.isSafeValue=!1,this.localize=new K(this),this.hasFocus=!1,this.isDraggingGridHandle=!1,this.isEmpty=!1,this.inputValue="",this.hue=0,this.saturation=100,this.brightness=100,this.alpha=100,this.value="",this.defaultValue="",this.label="",this.format="hex",this.inline=!1,this.size="medium",this.noFormatToggle=!1,this.name="",this.disabled=!1,this.hoist=!1,this.opacity=!1,this.uppercase=!1,this.swatches="",this.form="",this.required=!1,this.handleFocusIn=()=>{this.hasFocus=!0,this.emit("sl-focus")},this.handleFocusOut=()=>{this.hasFocus=!1,this.emit("sl-blur")},this.addEventListener("focusin",this.handleFocusIn),this.addEventListener("focusout",this.handleFocusOut)}get validity(){return this.input.validity}get validationMessage(){return this.input.validationMessage}firstUpdated(){this.input.updateComplete.then(()=>{this.formControlController.updateValidity()})}handleCopy(){this.input.select(),document.execCommand("copy"),this.previewButton.focus(),this.previewButton.classList.add("color-picker__preview-color--copied"),this.previewButton.addEventListener("animationend",()=>{this.previewButton.classList.remove("color-picker__preview-color--copied")})}handleFormatToggle(){const e=["hex","rgb","hsl","hsv"],t=(e.indexOf(this.format)+1)%e.length;this.format=e[t],this.setColor(this.value),this.emit("sl-change"),this.emit("sl-input")}handleAlphaDrag(e){const t=this.shadowRoot.querySelector(".color-picker__slider.color-picker__alpha"),s=t.querySelector(".color-picker__slider-handle"),{width:i}=t.getBoundingClientRect();let o=this.value,r=this.value;s.focus(),e.preventDefault(),di(t,{onMove:a=>{this.alpha=gt(a/i*100,0,100),this.syncValues(),this.value!==r&&(r=this.value,this.emit("sl-input"))},onStop:()=>{this.value!==o&&(o=this.value,this.emit("sl-change"))},initialEvent:e})}handleHueDrag(e){const t=this.shadowRoot.querySelector(".color-picker__slider.color-picker__hue"),s=t.querySelector(".color-picker__slider-handle"),{width:i}=t.getBoundingClientRect();let o=this.value,r=this.value;s.focus(),e.preventDefault(),di(t,{onMove:a=>{this.hue=gt(a/i*360,0,360),this.syncValues(),this.value!==r&&(r=this.value,this.emit("sl-input"))},onStop:()=>{this.value!==o&&(o=this.value,this.emit("sl-change"))},initialEvent:e})}handleGridDrag(e){const t=this.shadowRoot.querySelector(".color-picker__grid"),s=t.querySelector(".color-picker__grid-handle"),{width:i,height:o}=t.getBoundingClientRect();let r=this.value,a=this.value;s.focus(),e.preventDefault(),this.isDraggingGridHandle=!0,di(t,{onMove:(l,c)=>{this.saturation=gt(l/i*100,0,100),this.brightness=gt(100-c/o*100,0,100),this.syncValues(),this.value!==a&&(a=this.value,this.emit("sl-input"))},onStop:()=>{this.isDraggingGridHandle=!1,this.value!==r&&(r=this.value,this.emit("sl-change"))},initialEvent:e})}handleAlphaKeyDown(e){const t=e.shiftKey?10:1,s=this.value;e.key==="ArrowLeft"&&(e.preventDefault(),this.alpha=gt(this.alpha-t,0,100),this.syncValues()),e.key==="ArrowRight"&&(e.preventDefault(),this.alpha=gt(this.alpha+t,0,100),this.syncValues()),e.key==="Home"&&(e.preventDefault(),this.alpha=0,this.syncValues()),e.key==="End"&&(e.preventDefault(),this.alpha=100,this.syncValues()),this.value!==s&&(this.emit("sl-change"),this.emit("sl-input"))}handleHueKeyDown(e){const t=e.shiftKey?10:1,s=this.value;e.key==="ArrowLeft"&&(e.preventDefault(),this.hue=gt(this.hue-t,0,360),this.syncValues()),e.key==="ArrowRight"&&(e.preventDefault(),this.hue=gt(this.hue+t,0,360),this.syncValues()),e.key==="Home"&&(e.preventDefault(),this.hue=0,this.syncValues()),e.key==="End"&&(e.preventDefault(),this.hue=360,this.syncValues()),this.value!==s&&(this.emit("sl-change"),this.emit("sl-input"))}handleGridKeyDown(e){const t=e.shiftKey?10:1,s=this.value;e.key==="ArrowLeft"&&(e.preventDefault(),this.saturation=gt(this.saturation-t,0,100),this.syncValues()),e.key==="ArrowRight"&&(e.preventDefault(),this.saturation=gt(this.saturation+t,0,100),this.syncValues()),e.key==="ArrowUp"&&(e.preventDefault(),this.brightness=gt(this.brightness+t,0,100),this.syncValues()),e.key==="ArrowDown"&&(e.preventDefault(),this.brightness=gt(this.brightness-t,0,100),this.syncValues()),this.value!==s&&(this.emit("sl-change"),this.emit("sl-input"))}handleInputChange(e){const t=e.target,s=this.value;e.stopPropagation(),this.input.value?(this.setColor(t.value),t.value=this.value):this.value="",this.value!==s&&(this.emit("sl-change"),this.emit("sl-input"))}handleInputInput(e){this.formControlController.updateValidity(),e.stopPropagation()}handleInputKeyDown(e){if(e.key==="Enter"){const t=this.value;this.input.value?(this.setColor(this.input.value),this.input.value=this.value,this.value!==t&&(this.emit("sl-change"),this.emit("sl-input")),setTimeout(()=>this.input.select())):this.hue=0}}handleInputInvalid(e){this.formControlController.setValidity(!1),this.formControlController.emitInvalidEvent(e)}handleTouchMove(e){e.preventDefault()}parseColor(e){const t=new dt(e);if(!t.isValid)return null;const s=t.toHsl(),i={h:s.h,s:s.s*100,l:s.l*100,a:s.a},o=t.toRgb(),r=t.toHexString(),a=t.toHex8String(),l=t.toHsv(),c={h:l.h,s:l.s*100,v:l.v*100,a:l.a};return{hsl:{h:i.h,s:i.s,l:i.l,string:this.setLetterCase(`hsl(${Math.round(i.h)}, ${Math.round(i.s)}%, ${Math.round(i.l)}%)`)},hsla:{h:i.h,s:i.s,l:i.l,a:i.a,string:this.setLetterCase(`hsla(${Math.round(i.h)}, ${Math.round(i.s)}%, ${Math.round(i.l)}%, ${i.a.toFixed(2).toString()})`)},hsv:{h:c.h,s:c.s,v:c.v,string:this.setLetterCase(`hsv(${Math.round(c.h)}, ${Math.round(c.s)}%, ${Math.round(c.v)}%)`)},hsva:{h:c.h,s:c.s,v:c.v,a:c.a,string:this.setLetterCase(`hsva(${Math.round(c.h)}, ${Math.round(c.s)}%, ${Math.round(c.v)}%, ${c.a.toFixed(2).toString()})`)},rgb:{r:o.r,g:o.g,b:o.b,string:this.setLetterCase(`rgb(${Math.round(o.r)}, ${Math.round(o.g)}, ${Math.round(o.b)})`)},rgba:{r:o.r,g:o.g,b:o.b,a:o.a,string:this.setLetterCase(`rgba(${Math.round(o.r)}, ${Math.round(o.g)}, ${Math.round(o.b)}, ${o.a.toFixed(2).toString()})`)},hex:this.setLetterCase(r),hexa:this.setLetterCase(a)}}setColor(e){const t=this.parseColor(e);return t===null?!1:(this.hue=t.hsva.h,this.saturation=t.hsva.s,this.brightness=t.hsva.v,this.alpha=this.opacity?t.hsva.a*100:100,this.syncValues(),!0)}setLetterCase(e){return typeof e!="string"?"":this.uppercase?e.toUpperCase():e.toLowerCase()}async syncValues(){const e=this.parseColor(`hsva(${this.hue}, ${this.saturation}%, ${this.brightness}%, ${this.alpha/100})`);e!==null&&(this.format==="hsl"?this.inputValue=this.opacity?e.hsla.string:e.hsl.string:this.format==="rgb"?this.inputValue=this.opacity?e.rgba.string:e.rgb.string:this.format==="hsv"?this.inputValue=this.opacity?e.hsva.string:e.hsv.string:this.inputValue=this.opacity?e.hexa:e.hex,this.isSafeValue=!0,this.value=this.inputValue,await this.updateComplete,this.isSafeValue=!1)}handleAfterHide(){this.previewButton.classList.remove("color-picker__preview-color--copied")}handleEyeDropper(){if(!un)return;new EyeDropper().open().then(t=>{const s=this.value;this.setColor(t.sRGBHex),this.value!==s&&(this.emit("sl-change"),this.emit("sl-input"))}).catch(()=>{})}selectSwatch(e){const t=this.value;this.disabled||(this.setColor(e),this.value!==t&&(this.emit("sl-change"),this.emit("sl-input")))}getHexString(e,t,s,i=100){const o=new dt(`hsva(${e}, ${t}%, ${s}%, ${i/100})`);return o.isValid?o.toHex8String():""}stopNestedEventPropagation(e){e.stopImmediatePropagation()}handleFormatChange(){this.syncValues()}handleOpacityChange(){this.alpha=100}handleValueChange(e,t){if(this.isEmpty=!t,t||(this.hue=0,this.saturation=0,this.brightness=100,this.alpha=100),!this.isSafeValue){const s=this.parseColor(t);s!==null?(this.inputValue=this.value,this.hue=s.hsva.h,this.saturation=s.hsva.s,this.brightness=s.hsva.v,this.alpha=s.hsva.a*100,this.syncValues()):this.inputValue=e??""}}focus(e){this.inline?this.base.focus(e):this.trigger.focus(e)}blur(){var e;const t=this.inline?this.base:this.trigger;this.hasFocus&&(t.focus({preventScroll:!0}),t.blur()),(e=this.dropdown)!=null&&e.open&&this.dropdown.hide()}getFormattedValue(e="hex"){const t=this.parseColor(`hsva(${this.hue}, ${this.saturation}%, ${this.brightness}%, ${this.alpha/100})`);if(t===null)return"";switch(e){case"hex":return t.hex;case"hexa":return t.hexa;case"rgb":return t.rgb.string;case"rgba":return t.rgba.string;case"hsl":return t.hsl.string;case"hsla":return t.hsla.string;case"hsv":return t.hsv.string;case"hsva":return t.hsva.string;default:return""}}checkValidity(){return this.input.checkValidity()}getForm(){return this.formControlController.getForm()}reportValidity(){return!this.inline&&!this.validity.valid?(this.dropdown.show(),this.addEventListener("sl-after-show",()=>this.input.reportValidity(),{once:!0}),this.disabled||this.formControlController.emitInvalidEvent(),!1):this.input.reportValidity()}setCustomValidity(e){this.input.setCustomValidity(e),this.formControlController.updateValidity()}render(){const e=this.saturation,t=100-this.brightness,s=Array.isArray(this.swatches)?this.swatches:this.swatches.split(";").filter(o=>o.trim()!==""),i=v`
      <div
        part="base"
        class=${L({"color-picker":!0,"color-picker--inline":this.inline,"color-picker--disabled":this.disabled,"color-picker--focused":this.hasFocus})}
        aria-disabled=${this.disabled?"true":"false"}
        aria-labelledby="label"
        tabindex=${this.inline?"0":"-1"}
      >
        ${this.inline?v`
              <sl-visually-hidden id="label">
                <slot name="label">${this.label}</slot>
              </sl-visually-hidden>
            `:null}

        <div
          part="grid"
          class="color-picker__grid"
          style=${jt({backgroundColor:this.getHexString(this.hue,100,100)})}
          @pointerdown=${this.handleGridDrag}
          @touchmove=${this.handleTouchMove}
        >
          <span
            part="grid-handle"
            class=${L({"color-picker__grid-handle":!0,"color-picker__grid-handle--dragging":this.isDraggingGridHandle})}
            style=${jt({top:`${t}%`,left:`${e}%`,backgroundColor:this.getHexString(this.hue,this.saturation,this.brightness,this.alpha)})}
            role="application"
            aria-label="HSV"
            tabindex=${O(this.disabled?void 0:"0")}
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
                style=${jt({left:`${this.hue===0?0:100/(360/this.hue)}%`})}
                role="slider"
                aria-label="hue"
                aria-orientation="horizontal"
                aria-valuemin="0"
                aria-valuemax="360"
                aria-valuenow=${`${Math.round(this.hue)}`}
                tabindex=${O(this.disabled?void 0:"0")}
                @keydown=${this.handleHueKeyDown}
              ></span>
            </div>

            ${this.opacity?v`
                  <div
                    part="slider opacity-slider"
                    class="color-picker__alpha color-picker__slider color-picker__transparent-bg"
                    @pointerdown="${this.handleAlphaDrag}"
                    @touchmove=${this.handleTouchMove}
                  >
                    <div
                      class="color-picker__alpha-gradient"
                      style=${jt({backgroundImage:`linear-gradient(
                          to right,
                          ${this.getHexString(this.hue,this.saturation,this.brightness,0)} 0%,
                          ${this.getHexString(this.hue,this.saturation,this.brightness,100)} 100%
                        )`})}
                    ></div>
                    <span
                      part="slider-handle opacity-slider-handle"
                      class="color-picker__slider-handle"
                      style=${jt({left:`${this.alpha}%`})}
                      role="slider"
                      aria-label="alpha"
                      aria-orientation="horizontal"
                      aria-valuemin="0"
                      aria-valuemax="100"
                      aria-valuenow=${Math.round(this.alpha)}
                      tabindex=${O(this.disabled?void 0:"0")}
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
            style=${jt({"--preview-color":this.getHexString(this.hue,this.saturation,this.brightness,this.alpha)})}
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
            ${this.noFormatToggle?"":v`
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
            ${un?v`
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

        ${s.length>0?v`
              <div part="swatches" class="color-picker__swatches">
                ${s.map(o=>{const r=this.parseColor(o);return r?v`
                    <div
                      part="swatch"
                      class="color-picker__swatch color-picker__transparent-bg"
                      tabindex=${O(this.disabled?void 0:"0")}
                      role="button"
                      aria-label=${o}
                      @click=${()=>this.selectSwatch(o)}
                      @keydown=${a=>!this.disabled&&a.key==="Enter"&&this.setColor(r.hexa)}
                    >
                      <div
                        class="color-picker__swatch-color"
                        style=${jt({backgroundColor:r.hexa})}
                      ></div>
                    </div>
                  `:(console.error(`Unable to parse swatch color: "${o}"`,this),"")})}
              </div>
            `:""}
      </div>
    `;return this.inline?i:v`
      <sl-dropdown
        class="color-dropdown"
        aria-disabled=${this.disabled?"true":"false"}
        .containing-element=${this}
        ?disabled=${this.disabled}
        ?hoist=${this.hoist}
        @sl-after-hide=${this.handleAfterHide}
      >
        <button
          part="trigger"
          slot="trigger"
          class=${L({"color-dropdown__trigger":!0,"color-dropdown__trigger--disabled":this.disabled,"color-dropdown__trigger--small":this.size==="small","color-dropdown__trigger--medium":this.size==="medium","color-dropdown__trigger--large":this.size==="large","color-dropdown__trigger--empty":this.isEmpty,"color-dropdown__trigger--focused":this.hasFocus,"color-picker__transparent-bg":!0})}
          style=${jt({color:this.getHexString(this.hue,this.saturation,this.brightness,this.alpha)})}
          type="button"
        >
          <sl-visually-hidden>
            <slot name="label">${this.label}</slot>
          </sl-visually-hidden>
        </button>
        ${i}
      </sl-dropdown>
    `}};W.styles=[M,Xh];W.dependencies={"sl-button-group":ps,"sl-button":Q,"sl-dropdown":Ot,"sl-icon":it,"sl-input":H,"sl-visually-hidden":Ir};n([T('[part~="base"]')],W.prototype,"base",2);n([T('[part~="input"]')],W.prototype,"input",2);n([T(".color-dropdown")],W.prototype,"dropdown",2);n([T('[part~="preview"]')],W.prototype,"previewButton",2);n([T('[part~="trigger"]')],W.prototype,"trigger",2);n([E()],W.prototype,"hasFocus",2);n([E()],W.prototype,"isDraggingGridHandle",2);n([E()],W.prototype,"isEmpty",2);n([E()],W.prototype,"inputValue",2);n([E()],W.prototype,"hue",2);n([E()],W.prototype,"saturation",2);n([E()],W.prototype,"brightness",2);n([E()],W.prototype,"alpha",2);n([d()],W.prototype,"value",2);n([cs()],W.prototype,"defaultValue",2);n([d()],W.prototype,"label",2);n([d()],W.prototype,"format",2);n([d({type:Boolean,reflect:!0})],W.prototype,"inline",2);n([d({reflect:!0})],W.prototype,"size",2);n([d({attribute:"no-format-toggle",type:Boolean})],W.prototype,"noFormatToggle",2);n([d()],W.prototype,"name",2);n([d({type:Boolean,reflect:!0})],W.prototype,"disabled",2);n([d({type:Boolean})],W.prototype,"hoist",2);n([d({type:Boolean})],W.prototype,"opacity",2);n([d({type:Boolean})],W.prototype,"uppercase",2);n([d()],W.prototype,"swatches",2);n([d({reflect:!0})],W.prototype,"form",2);n([d({type:Boolean,reflect:!0})],W.prototype,"required",2);n([Ci({passive:!1})],W.prototype,"handleTouchMove",1);n([C("format",{waitUntilFirstUpdate:!0})],W.prototype,"handleFormatChange",1);n([C("opacity",{waitUntilFirstUpdate:!0})],W.prototype,"handleOpacityChange",1);n([C("value")],W.prototype,"handleValueChange",1);W.define("sl-color-picker");var du=A`
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
`,pl=class extends P{constructor(){super(...arguments),this.hasSlotController=new Ht(this,"footer","header","image")}render(){return v`
      <div
        part="base"
        class=${L({card:!0,"card--has-footer":this.hasSlotController.test("footer"),"card--has-image":this.hasSlotController.test("image"),"card--has-header":this.hasSlotController.test("header")})}
      >
        <slot name="image" part="image" class="card__image"></slot>
        <slot name="header" part="header" class="card__header"></slot>
        <slot part="body" class="card__body"></slot>
        <slot name="footer" part="footer" class="card__footer"></slot>
      </div>
    `}};pl.styles=[M,du];pl.define("sl-card");var hu=class{constructor(e,t){this.timerId=0,this.activeInteractions=0,this.paused=!1,this.stopped=!0,this.pause=()=>{this.activeInteractions++||(this.paused=!0,this.host.requestUpdate())},this.resume=()=>{--this.activeInteractions||(this.paused=!1,this.host.requestUpdate())},e.addController(this),this.host=e,this.tickCallback=t}hostConnected(){this.host.addEventListener("mouseenter",this.pause),this.host.addEventListener("mouseleave",this.resume),this.host.addEventListener("focusin",this.pause),this.host.addEventListener("focusout",this.resume),this.host.addEventListener("touchstart",this.pause,{passive:!0}),this.host.addEventListener("touchend",this.resume)}hostDisconnected(){this.stop(),this.host.removeEventListener("mouseenter",this.pause),this.host.removeEventListener("mouseleave",this.resume),this.host.removeEventListener("focusin",this.pause),this.host.removeEventListener("focusout",this.resume),this.host.removeEventListener("touchstart",this.pause),this.host.removeEventListener("touchend",this.resume)}start(e){this.stop(),this.stopped=!1,this.timerId=window.setInterval(()=>{this.paused||this.tickCallback()},e)}stop(){clearInterval(this.timerId),this.stopped=!0,this.host.requestUpdate()}},uu=A`
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
 */function*pu(e,t){if(e!==void 0){let s=0;for(const i of e)yield t(i,s++)}}/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function*fu(e,t,s=1){const i=t===void 0?0:e;t??(t=e);for(let o=i;s>0?o<t:t<o;o+=s)yield o}var pt=class extends P{constructor(){super(...arguments),this.loop=!1,this.navigation=!1,this.pagination=!1,this.autoplay=!1,this.autoplayInterval=3e3,this.slidesPerPage=1,this.slidesPerMove=1,this.orientation="horizontal",this.mouseDragging=!1,this.activeSlide=0,this.scrolling=!1,this.dragging=!1,this.autoplayController=new hu(this,()=>this.next()),this.localize=new K(this),this.pendingSlideChange=!1,this.handleMouseDrag=e=>{this.dragging||(this.scrollContainer.style.setProperty("scroll-snap-type","none"),this.dragging=!0),this.scrollContainer.scrollBy({left:-e.movementX,top:-e.movementY,behavior:"instant"})},this.handleMouseDragEnd=()=>{const e=this.scrollContainer;document.removeEventListener("pointermove",this.handleMouseDrag,{capture:!0});const t=e.scrollLeft,s=e.scrollTop;e.style.removeProperty("scroll-snap-type"),e.style.setProperty("overflow","hidden");const i=e.scrollLeft,o=e.scrollTop;e.style.removeProperty("overflow"),e.style.setProperty("scroll-snap-type","none"),e.scrollTo({left:t,top:s,behavior:"instant"}),requestAnimationFrame(async()=>{(t!==i||s!==o)&&(e.scrollTo({left:i,top:o,behavior:mr()?"auto":"smooth"}),await Bt(e,"scrollend")),e.style.removeProperty("scroll-snap-type"),this.dragging=!1,this.handleScrollEnd()})},this.handleSlotChange=e=>{e.some(s=>[...s.addedNodes,...s.removedNodes].some(i=>this.isCarouselItem(i)&&!i.hasAttribute("data-clone")))&&this.initializeSlides(),this.requestUpdate()}}connectedCallback(){super.connectedCallback(),this.setAttribute("role","region"),this.setAttribute("aria-label",this.localize.term("carousel"))}disconnectedCallback(){var e;super.disconnectedCallback(),(e=this.mutationObserver)==null||e.disconnect()}firstUpdated(){this.initializeSlides(),this.mutationObserver=new MutationObserver(this.handleSlotChange),this.mutationObserver.observe(this,{childList:!0,subtree:!0})}willUpdate(e){(e.has("slidesPerMove")||e.has("slidesPerPage"))&&(this.slidesPerMove=Math.min(this.slidesPerMove,this.slidesPerPage))}getPageCount(){const e=this.getSlides().length,{slidesPerPage:t,slidesPerMove:s,loop:i}=this,o=i?e/s:(e-t)/s+1;return Math.ceil(o)}getCurrentPage(){return Math.ceil(this.activeSlide/this.slidesPerMove)}canScrollNext(){return this.loop||this.getCurrentPage()<this.getPageCount()-1}canScrollPrev(){return this.loop||this.getCurrentPage()>0}getSlides({excludeClones:e=!0}={}){return[...this.children].filter(t=>this.isCarouselItem(t)&&(!e||!t.hasAttribute("data-clone")))}handleKeyDown(e){if(["ArrowLeft","ArrowRight","ArrowUp","ArrowDown","Home","End"].includes(e.key)){const t=e.target,s=this.localize.dir()==="rtl",i=t.closest('[part~="pagination-item"]')!==null,o=e.key==="ArrowDown"||!s&&e.key==="ArrowRight"||s&&e.key==="ArrowLeft",r=e.key==="ArrowUp"||!s&&e.key==="ArrowLeft"||s&&e.key==="ArrowRight";e.preventDefault(),r&&this.previous(),o&&this.next(),e.key==="Home"&&this.goToSlide(0),e.key==="End"&&this.goToSlide(this.getSlides().length-1),i&&this.updateComplete.then(()=>{var a;const l=(a=this.shadowRoot)==null?void 0:a.querySelector('[part~="pagination-item--active"]');l&&l.focus()})}}handleMouseDragStart(e){this.mouseDragging&&e.button===0&&(e.preventDefault(),document.addEventListener("pointermove",this.handleMouseDrag,{capture:!0,passive:!0}),document.addEventListener("pointerup",this.handleMouseDragEnd,{capture:!0,once:!0}))}handleScroll(){this.scrolling=!0,this.pendingSlideChange||this.synchronizeSlides()}synchronizeSlides(){const e=new IntersectionObserver(t=>{e.disconnect();for(const l of t){const c=l.target;c.toggleAttribute("inert",!l.isIntersecting),c.classList.toggle("--in-view",l.isIntersecting),c.setAttribute("aria-hidden",l.isIntersecting?"false":"true")}const s=t.find(l=>l.isIntersecting);if(!s)return;const i=this.getSlides({excludeClones:!1}),o=this.getSlides().length,r=i.indexOf(s.target),a=this.loop?r-this.slidesPerPage:r;if(this.activeSlide=(Math.ceil(a/this.slidesPerMove)*this.slidesPerMove+o)%o,!this.scrolling&&this.loop&&s.target.hasAttribute("data-clone")){const l=Number(s.target.getAttribute("data-clone"));this.goToSlide(l,"instant")}},{root:this.scrollContainer,threshold:.6});this.getSlides({excludeClones:!1}).forEach(t=>{e.observe(t)})}handleScrollEnd(){!this.scrolling||this.dragging||(this.scrolling=!1,this.pendingSlideChange=!1,this.synchronizeSlides())}isCarouselItem(e){return e instanceof Element&&e.tagName.toLowerCase()==="sl-carousel-item"}initializeSlides(){this.getSlides({excludeClones:!1}).forEach((e,t)=>{e.classList.remove("--in-view"),e.classList.remove("--is-active"),e.setAttribute("aria-label",this.localize.term("slideNum",t+1)),e.hasAttribute("data-clone")&&e.remove()}),this.updateSlidesSnap(),this.loop&&this.createClones(),this.synchronizeSlides(),this.goToSlide(this.activeSlide,"auto")}createClones(){const e=this.getSlides(),t=this.slidesPerPage,s=e.slice(-t),i=e.slice(0,t);s.reverse().forEach((o,r)=>{const a=o.cloneNode(!0);a.setAttribute("data-clone",String(e.length-r-1)),this.prepend(a)}),i.forEach((o,r)=>{const a=o.cloneNode(!0);a.setAttribute("data-clone",String(r)),this.append(a)})}handleSlideChange(){const e=this.getSlides();e.forEach((t,s)=>{t.classList.toggle("--is-active",s===this.activeSlide)}),this.hasUpdated&&this.emit("sl-slide-change",{detail:{index:this.activeSlide,slide:e[this.activeSlide]}})}updateSlidesSnap(){const e=this.getSlides(),t=this.slidesPerMove;e.forEach((s,i)=>{(i+t)%t===0?s.style.removeProperty("scroll-snap-align"):s.style.setProperty("scroll-snap-align","none")})}handleAutoplayChange(){this.autoplayController.stop(),this.autoplay&&this.autoplayController.start(this.autoplayInterval)}previous(e="smooth"){this.goToSlide(this.activeSlide-this.slidesPerMove,e)}next(e="smooth"){this.goToSlide(this.activeSlide+this.slidesPerMove,e)}goToSlide(e,t="smooth"){const{slidesPerPage:s,loop:i}=this,o=this.getSlides(),r=this.getSlides({excludeClones:!1});if(!o.length)return;const a=i?(e+o.length)%o.length:gt(e,0,o.length-s);this.activeSlide=a;const l=this.localize.dir()==="rtl",c=gt(e+(i?s:0)+(l?s-1:0),0,r.length-1),h=r[c];this.scrollToSlide(h,mr()?"auto":t)}scrollToSlide(e,t="smooth"){const s=this.scrollContainer,i=s.getBoundingClientRect(),o=e.getBoundingClientRect(),r=o.left-i.left,a=o.top-i.top;(r||a)&&(this.pendingSlideChange=!0,s.scrollTo({left:r+s.scrollLeft,top:a+s.scrollTop,behavior:t}))}render(){const{slidesPerMove:e,scrolling:t}=this,s=this.getPageCount(),i=this.getCurrentPage(),o=this.canScrollPrev(),r=this.canScrollNext(),a=this.localize.dir()==="rtl";return v`
      <div part="base" class="carousel">
        <div
          id="scroll-container"
          part="scroll-container"
          class="${L({carousel__slides:!0,"carousel__slides--horizontal":this.orientation==="horizontal","carousel__slides--vertical":this.orientation==="vertical","carousel__slides--dragging":this.dragging})}"
          style="--slides-per-page: ${this.slidesPerPage};"
          aria-busy="${t?"true":"false"}"
          aria-atomic="true"
          tabindex="0"
          @keydown=${this.handleKeyDown}
          @mousedown="${this.handleMouseDragStart}"
          @scroll="${this.handleScroll}"
          @scrollend=${this.handleScrollEnd}
        >
          <slot></slot>
        </div>

        ${this.navigation?v`
              <div part="navigation" class="carousel__navigation">
                <button
                  part="navigation-button navigation-button--previous"
                  class="${L({"carousel__navigation-button":!0,"carousel__navigation-button--previous":!0,"carousel__navigation-button--disabled":!o})}"
                  aria-label="${this.localize.term("previousSlide")}"
                  aria-controls="scroll-container"
                  aria-disabled="${o?"false":"true"}"
                  @click=${o?()=>this.previous():null}
                >
                  <slot name="previous-icon">
                    <sl-icon library="system" name="${a?"chevron-left":"chevron-right"}"></sl-icon>
                  </slot>
                </button>

                <button
                  part="navigation-button navigation-button--next"
                  class=${L({"carousel__navigation-button":!0,"carousel__navigation-button--next":!0,"carousel__navigation-button--disabled":!r})}
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
        ${this.pagination?v`
              <div part="pagination" role="tablist" class="carousel__pagination" aria-controls="scroll-container">
                ${pu(fu(s),l=>{const c=l===i;return v`
                    <button
                      part="pagination-item ${c?"pagination-item--active":""}"
                      class="${L({"carousel__pagination-item":!0,"carousel__pagination-item--active":c})}"
                      role="tab"
                      aria-selected="${c?"true":"false"}"
                      aria-label="${this.localize.term("goToSlide",l+1,s)}"
                      tabindex=${c?"0":"-1"}
                      @click=${()=>this.goToSlide(l*e)}
                      @keydown=${this.handleKeyDown}
                    ></button>
                  `})}
              </div>
            `:""}
      </div>
    `}};pt.styles=[M,uu];pt.dependencies={"sl-icon":it};n([d({type:Boolean,reflect:!0})],pt.prototype,"loop",2);n([d({type:Boolean,reflect:!0})],pt.prototype,"navigation",2);n([d({type:Boolean,reflect:!0})],pt.prototype,"pagination",2);n([d({type:Boolean,reflect:!0})],pt.prototype,"autoplay",2);n([d({type:Number,attribute:"autoplay-interval"})],pt.prototype,"autoplayInterval",2);n([d({type:Number,attribute:"slides-per-page"})],pt.prototype,"slidesPerPage",2);n([d({type:Number,attribute:"slides-per-move"})],pt.prototype,"slidesPerMove",2);n([d()],pt.prototype,"orientation",2);n([d({type:Boolean,reflect:!0,attribute:"mouse-dragging"})],pt.prototype,"mouseDragging",2);n([T(".carousel__slides")],pt.prototype,"scrollContainer",2);n([T(".carousel__pagination")],pt.prototype,"paginationContainer",2);n([E()],pt.prototype,"activeSlide",2);n([E()],pt.prototype,"scrolling",2);n([E()],pt.prototype,"dragging",2);n([Ci({passive:!0})],pt.prototype,"handleScroll",1);n([C("loop",{waitUntilFirstUpdate:!0}),C("slidesPerPage",{waitUntilFirstUpdate:!0})],pt.prototype,"initializeSlides",1);n([C("activeSlide")],pt.prototype,"handleSlideChange",1);n([C("slidesPerMove")],pt.prototype,"updateSlidesSnap",1);n([C("autoplay")],pt.prototype,"handleAutoplayChange",1);pt.define("sl-carousel");var mu=(e,t)=>{let s=0;return function(...i){window.clearTimeout(s),s=window.setTimeout(()=>{e.call(this,...i)},t)}},pn=(e,t,s)=>{const i=e[t];e[t]=function(...o){i.call(this,...o),s.call(this,i,...o)}},gu="onscrollend"in window;if(!gu){const e=new Set,t=new WeakMap,s=o=>{for(const r of o.changedTouches)e.add(r.identifier)},i=o=>{for(const r of o.changedTouches)e.delete(r.identifier)};document.addEventListener("touchstart",s,!0),document.addEventListener("touchend",i,!0),document.addEventListener("touchcancel",i,!0),pn(EventTarget.prototype,"addEventListener",function(o,r){if(r!=="scrollend")return;const a=mu(()=>{e.size?a():this.dispatchEvent(new Event("scrollend"))},100);o.call(this,"scroll",a,{passive:!0}),t.set(this,a)}),pn(EventTarget.prototype,"removeEventListener",function(o,r){if(r!=="scrollend")return;const a=t.get(this);a&&o.call(this,"scroll",a,{passive:!0})})}var bu=A`
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
`,fl=class extends P{connectedCallback(){super.connectedCallback(),this.setAttribute("role","group")}render(){return v` <slot></slot> `}};fl.styles=[M,bu];fl.define("sl-carousel-item");Q.define("sl-button");ps.define("sl-button-group");var vu=A`
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
`,De=class extends P{constructor(){super(...arguments),this.hasError=!1,this.image="",this.label="",this.initials="",this.loading="eager",this.shape="circle"}handleImageChange(){this.hasError=!1}handleImageLoadError(){this.hasError=!0,this.emit("sl-error")}render(){const e=v`
      <img
        part="image"
        class="avatar__image"
        src="${this.image}"
        loading="${this.loading}"
        alt=""
        @error="${this.handleImageLoadError}"
      />
    `;let t=v``;return this.initials?t=v`<div part="initials" class="avatar__initials">${this.initials}</div>`:t=v`
        <div part="icon" class="avatar__icon" aria-hidden="true">
          <slot name="icon">
            <sl-icon name="person-fill" library="system"></sl-icon>
          </slot>
        </div>
      `,v`
      <div
        part="base"
        class=${L({avatar:!0,"avatar--circle":this.shape==="circle","avatar--rounded":this.shape==="rounded","avatar--square":this.shape==="square"})}
        role="img"
        aria-label=${this.label}
      >
        ${this.image&&!this.hasError?e:t}
      </div>
    `}};De.styles=[M,vu];De.dependencies={"sl-icon":it};n([E()],De.prototype,"hasError",2);n([d()],De.prototype,"image",2);n([d()],De.prototype,"label",2);n([d()],De.prototype,"initials",2);n([d()],De.prototype,"loading",2);n([d({reflect:!0})],De.prototype,"shape",2);n([C("image")],De.prototype,"handleImageChange",1);De.define("sl-avatar");var yu=A`
  .breadcrumb {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
  }
`,Fs=class extends P{constructor(){super(...arguments),this.localize=new K(this),this.separatorDir=this.localize.dir(),this.label=""}getSeparator(){const t=this.separatorSlot.assignedElements({flatten:!0})[0].cloneNode(!0);return[t,...t.querySelectorAll("[id]")].forEach(s=>s.removeAttribute("id")),t.setAttribute("data-default",""),t.slot="separator",t}handleSlotChange(){const e=[...this.defaultSlot.assignedElements({flatten:!0})].filter(t=>t.tagName.toLowerCase()==="sl-breadcrumb-item");e.forEach((t,s)=>{const i=t.querySelector('[slot="separator"]');i===null?t.append(this.getSeparator()):i.hasAttribute("data-default")&&i.replaceWith(this.getSeparator()),s===e.length-1?t.setAttribute("aria-current","page"):t.removeAttribute("aria-current")})}render(){return this.separatorDir!==this.localize.dir()&&(this.separatorDir=this.localize.dir(),this.updateComplete.then(()=>this.handleSlotChange())),v`
      <nav part="base" class="breadcrumb" aria-label=${this.label}>
        <slot @slotchange=${this.handleSlotChange}></slot>
      </nav>

      <span hidden aria-hidden="true">
        <slot name="separator">
          <sl-icon name=${this.localize.dir()==="rtl"?"chevron-left":"chevron-right"} library="system"></sl-icon>
        </slot>
      </span>
    `}};Fs.styles=[M,yu];Fs.dependencies={"sl-icon":it};n([T("slot")],Fs.prototype,"defaultSlot",2);n([T('slot[name="separator"]')],Fs.prototype,"separatorSlot",2);n([d()],Fs.prototype,"label",2);Fs.define("sl-breadcrumb");var wu=A`
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
`,Xe=class extends P{constructor(){super(...arguments),this.hasSlotController=new Ht(this,"prefix","suffix"),this.renderType="button",this.rel="noreferrer noopener"}setRenderType(){const e=this.defaultSlot.assignedElements({flatten:!0}).filter(t=>t.tagName.toLowerCase()==="sl-dropdown").length>0;if(this.href){this.renderType="link";return}if(e){this.renderType="dropdown";return}this.renderType="button"}hrefChanged(){this.setRenderType()}handleSlotChange(){this.setRenderType()}render(){return v`
      <div
        part="base"
        class=${L({"breadcrumb-item":!0,"breadcrumb-item--has-prefix":this.hasSlotController.test("prefix"),"breadcrumb-item--has-suffix":this.hasSlotController.test("suffix")})}
      >
        <span part="prefix" class="breadcrumb-item__prefix">
          <slot name="prefix"></slot>
        </span>

        ${this.renderType==="link"?v`
              <a
                part="label"
                class="breadcrumb-item__label breadcrumb-item__label--link"
                href="${this.href}"
                target="${O(this.target?this.target:void 0)}"
                rel=${O(this.target?this.rel:void 0)}
              >
                <slot @slotchange=${this.handleSlotChange}></slot>
              </a>
            `:""}
        ${this.renderType==="button"?v`
              <button part="label" type="button" class="breadcrumb-item__label breadcrumb-item__label--button">
                <slot @slotchange=${this.handleSlotChange}></slot>
              </button>
            `:""}
        ${this.renderType==="dropdown"?v`
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
    `}};Xe.styles=[M,wu];n([T("slot:not([name])")],Xe.prototype,"defaultSlot",2);n([E()],Xe.prototype,"renderType",2);n([d()],Xe.prototype,"href",2);n([d()],Xe.prototype,"target",2);n([d()],Xe.prototype,"rel",2);n([C("href",{waitUntilFirstUpdate:!0})],Xe.prototype,"hrefChanged",1);Xe.define("sl-breadcrumb-item");var xu=A`
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
`,ye=class extends P{constructor(){super(...arguments),this.isLoaded=!1}handleClick(){this.play=!this.play}handleLoad(){const e=document.createElement("canvas"),{width:t,height:s}=this.animatedImage;e.width=t,e.height=s,e.getContext("2d").drawImage(this.animatedImage,0,0,t,s),this.frozenFrame=e.toDataURL("image/gif"),this.isLoaded||(this.emit("sl-load"),this.isLoaded=!0)}handleError(){this.emit("sl-error")}handlePlayChange(){this.play&&(this.animatedImage.src="",this.animatedImage.src=this.src)}handleSrcChange(){this.isLoaded=!1}render(){return v`
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

        ${this.isLoaded?v`
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
    `}};ye.styles=[M,xu];ye.dependencies={"sl-icon":it};n([T(".animated-image__animated")],ye.prototype,"animatedImage",2);n([E()],ye.prototype,"frozenFrame",2);n([E()],ye.prototype,"isLoaded",2);n([d()],ye.prototype,"src",2);n([d()],ye.prototype,"alt",2);n([d({type:Boolean,reflect:!0})],ye.prototype,"play",2);n([C("play",{waitUntilFirstUpdate:!0})],ye.prototype,"handlePlayChange",1);n([C("src")],ye.prototype,"handleSrcChange",1);ye.define("sl-animated-image");var _u=A`
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
`,Di=class extends P{constructor(){super(...arguments),this.variant="primary",this.pill=!1,this.pulse=!1}render(){return v`
      <span
        part="base"
        class=${L({badge:!0,"badge--primary":this.variant==="primary","badge--success":this.variant==="success","badge--neutral":this.variant==="neutral","badge--warning":this.variant==="warning","badge--danger":this.variant==="danger","badge--pill":this.pill,"badge--pulse":this.pulse})}
        role="status"
      >
        <slot></slot>
      </span>
    `}};Di.styles=[M,_u];n([d({reflect:!0})],Di.prototype,"variant",2);n([d({type:Boolean,reflect:!0})],Di.prototype,"pill",2);n([d({type:Boolean,reflect:!0})],Di.prototype,"pulse",2);Di.define("sl-badge");var ku=A`
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
    padding-inline-end: var(--sl-spacing-medium);
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
`,$s=Object.assign(document.createElement("div"),{className:"sl-toast-stack"}),se=class extends P{constructor(){super(...arguments),this.hasSlotController=new Ht(this,"icon","suffix"),this.localize=new K(this),this.open=!1,this.closable=!1,this.variant="primary",this.duration=1/0,this.remainingTime=this.duration}firstUpdated(){this.base.hidden=!this.open}restartAutoHide(){this.handleCountdownChange(),clearTimeout(this.autoHideTimeout),clearInterval(this.remainingTimeInterval),this.open&&this.duration<1/0&&(this.autoHideTimeout=window.setTimeout(()=>this.hide(),this.duration),this.remainingTime=this.duration,this.remainingTimeInterval=window.setInterval(()=>{this.remainingTime-=100},100))}pauseAutoHide(){var e;(e=this.countdownAnimation)==null||e.pause(),clearTimeout(this.autoHideTimeout),clearInterval(this.remainingTimeInterval)}resumeAutoHide(){var e;this.duration<1/0&&(this.autoHideTimeout=window.setTimeout(()=>this.hide(),this.remainingTime),this.remainingTimeInterval=window.setInterval(()=>{this.remainingTime-=100},100),(e=this.countdownAnimation)==null||e.play())}handleCountdownChange(){if(this.open&&this.duration<1/0&&this.countdown){const{countdownElement:e}=this,t="100%",s="0";this.countdownAnimation=e.animate([{width:t},{width:s}],{duration:this.duration,easing:"linear"})}}handleCloseClick(){this.hide()}async handleOpenChange(){if(this.open){this.emit("sl-show"),this.duration<1/0&&this.restartAutoHide(),await bt(this.base),this.base.hidden=!1;const{keyframes:e,options:t}=nt(this,"alert.show",{dir:this.localize.dir()});await ht(this.base,e,t),this.emit("sl-after-show")}else{this.emit("sl-hide"),clearTimeout(this.autoHideTimeout),clearInterval(this.remainingTimeInterval),await bt(this.base);const{keyframes:e,options:t}=nt(this,"alert.hide",{dir:this.localize.dir()});await ht(this.base,e,t),this.base.hidden=!0,this.emit("sl-after-hide")}}handleDurationChange(){this.restartAutoHide()}async show(){if(!this.open)return this.open=!0,Bt(this,"sl-after-show")}async hide(){if(this.open)return this.open=!1,Bt(this,"sl-after-hide")}async toast(){return new Promise(e=>{this.handleCountdownChange(),$s.parentElement===null&&document.body.append($s),$s.appendChild(this),requestAnimationFrame(()=>{this.clientWidth,this.show()}),this.addEventListener("sl-after-hide",()=>{$s.removeChild(this),e(),$s.querySelector("sl-alert")===null&&$s.remove()},{once:!0})})}render(){return v`
      <div
        part="base"
        class=${L({alert:!0,"alert--open":this.open,"alert--closable":this.closable,"alert--has-countdown":!!this.countdown,"alert--has-icon":this.hasSlotController.test("icon"),"alert--primary":this.variant==="primary","alert--success":this.variant==="success","alert--neutral":this.variant==="neutral","alert--warning":this.variant==="warning","alert--danger":this.variant==="danger"})}
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

        ${this.closable?v`
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

        ${this.countdown?v`
              <div
                class=${L({alert__countdown:!0,"alert__countdown--ltr":this.countdown==="ltr"})}
              >
                <div class="alert__countdown-elapsed"></div>
              </div>
            `:""}
      </div>
    `}};se.styles=[M,ku];se.dependencies={"sl-icon-button":wt};n([T('[part~="base"]')],se.prototype,"base",2);n([T(".alert__countdown-elapsed")],se.prototype,"countdownElement",2);n([d({type:Boolean,reflect:!0})],se.prototype,"open",2);n([d({type:Boolean,reflect:!0})],se.prototype,"closable",2);n([d({reflect:!0})],se.prototype,"variant",2);n([d({type:Number})],se.prototype,"duration",2);n([d({type:String,reflect:!0})],se.prototype,"countdown",2);n([E()],se.prototype,"remainingTime",2);n([C("open",{waitUntilFirstUpdate:!0})],se.prototype,"handleOpenChange",1);n([C("duration")],se.prototype,"handleDurationChange",1);Z("alert.show",{keyframes:[{opacity:0,scale:.8},{opacity:1,scale:1}],options:{duration:250,easing:"ease"}});Z("alert.hide",{keyframes:[{opacity:1,scale:1},{opacity:0,scale:.8}],options:{duration:250,easing:"ease"}});se.define("sl-alert");const $u=[{offset:0,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)",transform:"translate3d(0, 0, 0)"},{offset:.2,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)",transform:"translate3d(0, 0, 0)"},{offset:.4,easing:"cubic-bezier(0.755, 0.05, 0.855, 0.06)",transform:"translate3d(0, -30px, 0) scaleY(1.1)"},{offset:.43,easing:"cubic-bezier(0.755, 0.05, 0.855, 0.06)",transform:"translate3d(0, -30px, 0) scaleY(1.1)"},{offset:.53,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)",transform:"translate3d(0, 0, 0)"},{offset:.7,easing:"cubic-bezier(0.755, 0.05, 0.855, 0.06)",transform:"translate3d(0, -15px, 0) scaleY(1.05)"},{offset:.8,"transition-timing-function":"cubic-bezier(0.215, 0.61, 0.355, 1)",transform:"translate3d(0, 0, 0) scaleY(0.95)"},{offset:.9,transform:"translate3d(0, -4px, 0) scaleY(1.02)"},{offset:1,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)",transform:"translate3d(0, 0, 0)"}],Cu=[{offset:0,opacity:"1"},{offset:.25,opacity:"0"},{offset:.5,opacity:"1"},{offset:.75,opacity:"0"},{offset:1,opacity:"1"}],Su=[{offset:0,transform:"translateX(0)"},{offset:.065,transform:"translateX(-6px) rotateY(-9deg)"},{offset:.185,transform:"translateX(5px) rotateY(7deg)"},{offset:.315,transform:"translateX(-3px) rotateY(-5deg)"},{offset:.435,transform:"translateX(2px) rotateY(3deg)"},{offset:.5,transform:"translateX(0)"}],Au=[{offset:0,transform:"scale(1)"},{offset:.14,transform:"scale(1.3)"},{offset:.28,transform:"scale(1)"},{offset:.42,transform:"scale(1.3)"},{offset:.7,transform:"scale(1)"}],Tu=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:.111,transform:"translate3d(0, 0, 0)"},{offset:.222,transform:"skewX(-12.5deg) skewY(-12.5deg)"},{offset:.33299999999999996,transform:"skewX(6.25deg) skewY(6.25deg)"},{offset:.444,transform:"skewX(-3.125deg) skewY(-3.125deg)"},{offset:.555,transform:"skewX(1.5625deg) skewY(1.5625deg)"},{offset:.6659999999999999,transform:"skewX(-0.78125deg) skewY(-0.78125deg)"},{offset:.777,transform:"skewX(0.390625deg) skewY(0.390625deg)"},{offset:.888,transform:"skewX(-0.1953125deg) skewY(-0.1953125deg)"},{offset:1,transform:"translate3d(0, 0, 0)"}],zu=[{offset:0,transform:"scale3d(1, 1, 1)"},{offset:.5,transform:"scale3d(1.05, 1.05, 1.05)"},{offset:1,transform:"scale3d(1, 1, 1)"}],Eu=[{offset:0,transform:"scale3d(1, 1, 1)"},{offset:.3,transform:"scale3d(1.25, 0.75, 1)"},{offset:.4,transform:"scale3d(0.75, 1.25, 1)"},{offset:.5,transform:"scale3d(1.15, 0.85, 1)"},{offset:.65,transform:"scale3d(0.95, 1.05, 1)"},{offset:.75,transform:"scale3d(1.05, 0.95, 1)"},{offset:1,transform:"scale3d(1, 1, 1)"}],Ou=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:.1,transform:"translate3d(-10px, 0, 0)"},{offset:.2,transform:"translate3d(10px, 0, 0)"},{offset:.3,transform:"translate3d(-10px, 0, 0)"},{offset:.4,transform:"translate3d(10px, 0, 0)"},{offset:.5,transform:"translate3d(-10px, 0, 0)"},{offset:.6,transform:"translate3d(10px, 0, 0)"},{offset:.7,transform:"translate3d(-10px, 0, 0)"},{offset:.8,transform:"translate3d(10px, 0, 0)"},{offset:.9,transform:"translate3d(-10px, 0, 0)"},{offset:1,transform:"translate3d(0, 0, 0)"}],Pu=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:.1,transform:"translate3d(-10px, 0, 0)"},{offset:.2,transform:"translate3d(10px, 0, 0)"},{offset:.3,transform:"translate3d(-10px, 0, 0)"},{offset:.4,transform:"translate3d(10px, 0, 0)"},{offset:.5,transform:"translate3d(-10px, 0, 0)"},{offset:.6,transform:"translate3d(10px, 0, 0)"},{offset:.7,transform:"translate3d(-10px, 0, 0)"},{offset:.8,transform:"translate3d(10px, 0, 0)"},{offset:.9,transform:"translate3d(-10px, 0, 0)"},{offset:1,transform:"translate3d(0, 0, 0)"}],Du=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:.1,transform:"translate3d(0, -10px, 0)"},{offset:.2,transform:"translate3d(0, 10px, 0)"},{offset:.3,transform:"translate3d(0, -10px, 0)"},{offset:.4,transform:"translate3d(0, 10px, 0)"},{offset:.5,transform:"translate3d(0, -10px, 0)"},{offset:.6,transform:"translate3d(0, 10px, 0)"},{offset:.7,transform:"translate3d(0, -10px, 0)"},{offset:.8,transform:"translate3d(0, 10px, 0)"},{offset:.9,transform:"translate3d(0, -10px, 0)"},{offset:1,transform:"translate3d(0, 0, 0)"}],Iu=[{offset:.2,transform:"rotate3d(0, 0, 1, 15deg)"},{offset:.4,transform:"rotate3d(0, 0, 1, -10deg)"},{offset:.6,transform:"rotate3d(0, 0, 1, 5deg)"},{offset:.8,transform:"rotate3d(0, 0, 1, -5deg)"},{offset:1,transform:"rotate3d(0, 0, 1, 0deg)"}],Ru=[{offset:0,transform:"scale3d(1, 1, 1)"},{offset:.1,transform:"scale3d(0.9, 0.9, 0.9) rotate3d(0, 0, 1, -3deg)"},{offset:.2,transform:"scale3d(0.9, 0.9, 0.9) rotate3d(0, 0, 1, -3deg)"},{offset:.3,transform:"scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, 3deg)"},{offset:.4,transform:"scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, -3deg)"},{offset:.5,transform:"scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, 3deg)"},{offset:.6,transform:"scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, -3deg)"},{offset:.7,transform:"scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, 3deg)"},{offset:.8,transform:"scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, -3deg)"},{offset:.9,transform:"scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, 3deg)"},{offset:1,transform:"scale3d(1, 1, 1)"}],Lu=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:.15,transform:"translate3d(-25%, 0, 0) rotate3d(0, 0, 1, -5deg)"},{offset:.3,transform:"translate3d(20%, 0, 0) rotate3d(0, 0, 1, 3deg)"},{offset:.45,transform:"translate3d(-15%, 0, 0) rotate3d(0, 0, 1, -3deg)"},{offset:.6,transform:"translate3d(10%, 0, 0) rotate3d(0, 0, 1, 2deg)"},{offset:.75,transform:"translate3d(-5%, 0, 0) rotate3d(0, 0, 1, -1deg)"},{offset:1,transform:"translate3d(0, 0, 0)"}],Mu=[{offset:0,transform:"translateY(-1200px) scale(0.7)",opacity:"0.7"},{offset:.8,transform:"translateY(0px) scale(0.7)",opacity:"0.7"},{offset:1,transform:"scale(1)",opacity:"1"}],Nu=[{offset:0,transform:"translateX(-2000px) scale(0.7)",opacity:"0.7"},{offset:.8,transform:"translateX(0px) scale(0.7)",opacity:"0.7"},{offset:1,transform:"scale(1)",opacity:"1"}],Fu=[{offset:0,transform:"translateX(2000px) scale(0.7)",opacity:"0.7"},{offset:.8,transform:"translateX(0px) scale(0.7)",opacity:"0.7"},{offset:1,transform:"scale(1)",opacity:"1"}],Bu=[{offset:0,transform:"translateY(1200px) scale(0.7)",opacity:"0.7"},{offset:.8,transform:"translateY(0px) scale(0.7)",opacity:"0.7"},{offset:1,transform:"scale(1)",opacity:"1"}],Uu=[{offset:0,transform:"scale(1)",opacity:"1"},{offset:.2,transform:"translateY(0px) scale(0.7)",opacity:"0.7"},{offset:1,transform:"translateY(700px) scale(0.7)",opacity:"0.7"}],Hu=[{offset:0,transform:"scale(1)",opacity:"1"},{offset:.2,transform:"translateX(0px) scale(0.7)",opacity:"0.7"},{offset:1,transform:"translateX(-2000px) scale(0.7)",opacity:"0.7"}],Vu=[{offset:0,transform:"scale(1)",opacity:"1"},{offset:.2,transform:"translateX(0px) scale(0.7)",opacity:"0.7"},{offset:1,transform:"translateX(2000px) scale(0.7)",opacity:"0.7"}],ju=[{offset:0,transform:"scale(1)",opacity:"1"},{offset:.2,transform:"translateY(0px) scale(0.7)",opacity:"0.7"},{offset:1,transform:"translateY(-700px) scale(0.7)",opacity:"0.7"}],Wu=[{offset:0,opacity:"0",transform:"scale3d(0.3, 0.3, 0.3)"},{offset:0,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.2,transform:"scale3d(1.1, 1.1, 1.1)"},{offset:.2,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.4,transform:"scale3d(0.9, 0.9, 0.9)"},{offset:.4,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.6,opacity:"1",transform:"scale3d(1.03, 1.03, 1.03)"},{offset:.6,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.8,transform:"scale3d(0.97, 0.97, 0.97)"},{offset:.8,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:1,opacity:"1",transform:"scale3d(1, 1, 1)"},{offset:1,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"}],qu=[{offset:0,opacity:"0",transform:"translate3d(0, -3000px, 0) scaleY(3)"},{offset:0,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.6,opacity:"1",transform:"translate3d(0, 25px, 0) scaleY(0.9)"},{offset:.6,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.75,transform:"translate3d(0, -10px, 0) scaleY(0.95)"},{offset:.75,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.9,transform:"translate3d(0, 5px, 0) scaleY(0.985)"},{offset:.9,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:1,transform:"translate3d(0, 0, 0)"},{offset:1,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"}],Ku=[{offset:0,opacity:"0",transform:"translate3d(-3000px, 0, 0) scaleX(3)"},{offset:0,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.6,opacity:"1",transform:"translate3d(25px, 0, 0) scaleX(1)"},{offset:.6,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.75,transform:"translate3d(-10px, 0, 0) scaleX(0.98)"},{offset:.75,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.9,transform:"translate3d(5px, 0, 0) scaleX(0.995)"},{offset:.9,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:1,transform:"translate3d(0, 0, 0)"},{offset:1,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"}],Yu=[{offset:0,opacity:"0",transform:"translate3d(3000px, 0, 0) scaleX(3)"},{offset:0,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.6,opacity:"1",transform:"translate3d(-25px, 0, 0) scaleX(1)"},{offset:.6,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.75,transform:"translate3d(10px, 0, 0) scaleX(0.98)"},{offset:.75,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.9,transform:"translate3d(-5px, 0, 0) scaleX(0.995)"},{offset:.9,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:1,transform:"translate3d(0, 0, 0)"},{offset:1,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"}],Gu=[{offset:0,opacity:"0",transform:"translate3d(0, 3000px, 0) scaleY(5)"},{offset:0,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.6,opacity:"1",transform:"translate3d(0, -20px, 0) scaleY(0.9)"},{offset:.6,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.75,transform:"translate3d(0, 10px, 0) scaleY(0.95)"},{offset:.75,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:.9,transform:"translate3d(0, -5px, 0) scaleY(0.985)"},{offset:.9,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"},{offset:1,transform:"translate3d(0, 0, 0)"},{offset:1,easing:"cubic-bezier(0.215, 0.61, 0.355, 1)"}],Xu=[{offset:.2,transform:"scale3d(0.9, 0.9, 0.9)"},{offset:.5,opacity:"1",transform:"scale3d(1.1, 1.1, 1.1)"},{offset:.55,opacity:"1",transform:"scale3d(1.1, 1.1, 1.1)"},{offset:1,opacity:"0",transform:"scale3d(0.3, 0.3, 0.3)"}],Zu=[{offset:.2,transform:"translate3d(0, 10px, 0) scaleY(0.985)"},{offset:.4,opacity:"1",transform:"translate3d(0, -20px, 0) scaleY(0.9)"},{offset:.45,opacity:"1",transform:"translate3d(0, -20px, 0) scaleY(0.9)"},{offset:1,opacity:"0",transform:"translate3d(0, 2000px, 0) scaleY(3)"}],Qu=[{offset:.2,opacity:"1",transform:"translate3d(20px, 0, 0) scaleX(0.9)"},{offset:1,opacity:"0",transform:"translate3d(-2000px, 0, 0) scaleX(2)"}],Ju=[{offset:.2,opacity:"1",transform:"translate3d(-20px, 0, 0) scaleX(0.9)"},{offset:1,opacity:"0",transform:"translate3d(2000px, 0, 0) scaleX(2)"}],tp=[{offset:.2,transform:"translate3d(0, -10px, 0) scaleY(0.985)"},{offset:.4,opacity:"1",transform:"translate3d(0, 20px, 0) scaleY(0.9)"},{offset:.45,opacity:"1",transform:"translate3d(0, 20px, 0) scaleY(0.9)"},{offset:1,opacity:"0",transform:"translate3d(0, -2000px, 0) scaleY(3)"}],ep=[{offset:0,opacity:"0"},{offset:1,opacity:"1"}],sp=[{offset:0,opacity:"0",transform:"translate3d(-100%, 100%, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],ip=[{offset:0,opacity:"0",transform:"translate3d(100%, 100%, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],op=[{offset:0,opacity:"0",transform:"translate3d(0, -100%, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],rp=[{offset:0,opacity:"0",transform:"translate3d(0, -2000px, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],ap=[{offset:0,opacity:"0",transform:"translate3d(-100%, 0, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],np=[{offset:0,opacity:"0",transform:"translate3d(-2000px, 0, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],lp=[{offset:0,opacity:"0",transform:"translate3d(100%, 0, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],cp=[{offset:0,opacity:"0",transform:"translate3d(2000px, 0, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],dp=[{offset:0,opacity:"0",transform:"translate3d(-100%, -100%, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],hp=[{offset:0,opacity:"0",transform:"translate3d(100%, -100%, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],up=[{offset:0,opacity:"0",transform:"translate3d(0, 100%, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],pp=[{offset:0,opacity:"0",transform:"translate3d(0, 2000px, 0)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],fp=[{offset:0,opacity:"1"},{offset:1,opacity:"0"}],mp=[{offset:0,opacity:"1",transform:"translate3d(0, 0, 0)"},{offset:1,opacity:"0",transform:"translate3d(-100%, 100%, 0)"}],gp=[{offset:0,opacity:"1",transform:"translate3d(0, 0, 0)"},{offset:1,opacity:"0",transform:"translate3d(100%, 100%, 0)"}],bp=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(0, 100%, 0)"}],vp=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(0, 2000px, 0)"}],yp=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(-100%, 0, 0)"}],wp=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(-2000px, 0, 0)"}],xp=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(100%, 0, 0)"}],_p=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(2000px, 0, 0)"}],kp=[{offset:0,opacity:"1",transform:"translate3d(0, 0, 0)"},{offset:1,opacity:"0",transform:"translate3d(-100%, -100%, 0)"}],$p=[{offset:0,opacity:"1",transform:"translate3d(0, 0, 0)"},{offset:1,opacity:"0",transform:"translate3d(100%, -100%, 0)"}],Cp=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(0, -100%, 0)"}],Sp=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(0, -2000px, 0)"}],Ap=[{offset:0,transform:"perspective(400px) scale3d(1, 1, 1) translate3d(0, 0, 0) rotate3d(0, 1, 0, -360deg)",easing:"ease-out"},{offset:.4,transform:`perspective(400px) scale3d(1, 1, 1) translate3d(0, 0, 150px)
      rotate3d(0, 1, 0, -190deg)`,easing:"ease-out"},{offset:.5,transform:`perspective(400px) scale3d(1, 1, 1) translate3d(0, 0, 150px)
      rotate3d(0, 1, 0, -170deg)`,easing:"ease-in"},{offset:.8,transform:`perspective(400px) scale3d(0.95, 0.95, 0.95) translate3d(0, 0, 0)
      rotate3d(0, 1, 0, 0deg)`,easing:"ease-in"},{offset:1,transform:"perspective(400px) scale3d(1, 1, 1) translate3d(0, 0, 0) rotate3d(0, 1, 0, 0deg)",easing:"ease-in"}],Tp=[{offset:0,transform:"perspective(400px) rotate3d(1, 0, 0, 90deg)",easing:"ease-in",opacity:"0"},{offset:.4,transform:"perspective(400px) rotate3d(1, 0, 0, -20deg)",easing:"ease-in"},{offset:.6,transform:"perspective(400px) rotate3d(1, 0, 0, 10deg)",opacity:"1"},{offset:.8,transform:"perspective(400px) rotate3d(1, 0, 0, -5deg)"},{offset:1,transform:"perspective(400px)"}],zp=[{offset:0,transform:"perspective(400px) rotate3d(0, 1, 0, 90deg)",easing:"ease-in",opacity:"0"},{offset:.4,transform:"perspective(400px) rotate3d(0, 1, 0, -20deg)",easing:"ease-in"},{offset:.6,transform:"perspective(400px) rotate3d(0, 1, 0, 10deg)",opacity:"1"},{offset:.8,transform:"perspective(400px) rotate3d(0, 1, 0, -5deg)"},{offset:1,transform:"perspective(400px)"}],Ep=[{offset:0,transform:"perspective(400px)"},{offset:.3,transform:"perspective(400px) rotate3d(1, 0, 0, -20deg)",opacity:"1"},{offset:1,transform:"perspective(400px) rotate3d(1, 0, 0, 90deg)",opacity:"0"}],Op=[{offset:0,transform:"perspective(400px)"},{offset:.3,transform:"perspective(400px) rotate3d(0, 1, 0, -15deg)",opacity:"1"},{offset:1,transform:"perspective(400px) rotate3d(0, 1, 0, 90deg)",opacity:"0"}],Pp=[{offset:0,transform:"translate3d(-100%, 0, 0) skewX(30deg)",opacity:"0"},{offset:.6,transform:"skewX(-20deg)",opacity:"1"},{offset:.8,transform:"skewX(5deg)"},{offset:1,transform:"translate3d(0, 0, 0)"}],Dp=[{offset:0,transform:"translate3d(100%, 0, 0) skewX(-30deg)",opacity:"0"},{offset:.6,transform:"skewX(20deg)",opacity:"1"},{offset:.8,transform:"skewX(-5deg)"},{offset:1,transform:"translate3d(0, 0, 0)"}],Ip=[{offset:0,opacity:"1"},{offset:1,transform:"translate3d(-100%, 0, 0) skewX(-30deg)",opacity:"0"}],Rp=[{offset:0,opacity:"1"},{offset:1,transform:"translate3d(100%, 0, 0) skewX(30deg)",opacity:"0"}],Lp=[{offset:0,transform:"rotate3d(0, 0, 1, -200deg)",opacity:"0"},{offset:1,transform:"translate3d(0, 0, 0)",opacity:"1"}],Mp=[{offset:0,transform:"rotate3d(0, 0, 1, -45deg)",opacity:"0"},{offset:1,transform:"translate3d(0, 0, 0)",opacity:"1"}],Np=[{offset:0,transform:"rotate3d(0, 0, 1, 45deg)",opacity:"0"},{offset:1,transform:"translate3d(0, 0, 0)",opacity:"1"}],Fp=[{offset:0,transform:"rotate3d(0, 0, 1, 45deg)",opacity:"0"},{offset:1,transform:"translate3d(0, 0, 0)",opacity:"1"}],Bp=[{offset:0,transform:"rotate3d(0, 0, 1, -90deg)",opacity:"0"},{offset:1,transform:"translate3d(0, 0, 0)",opacity:"1"}],Up=[{offset:0,opacity:"1"},{offset:1,transform:"rotate3d(0, 0, 1, 200deg)",opacity:"0"}],Hp=[{offset:0,opacity:"1"},{offset:1,transform:"rotate3d(0, 0, 1, 45deg)",opacity:"0"}],Vp=[{offset:0,opacity:"1"},{offset:1,transform:"rotate3d(0, 0, 1, -45deg)",opacity:"0"}],jp=[{offset:0,opacity:"1"},{offset:1,transform:"rotate3d(0, 0, 1, -45deg)",opacity:"0"}],Wp=[{offset:0,opacity:"1"},{offset:1,transform:"rotate3d(0, 0, 1, 90deg)",opacity:"0"}],qp=[{offset:0,transform:"translate3d(0, -100%, 0)",visibility:"visible"},{offset:1,transform:"translate3d(0, 0, 0)"}],Kp=[{offset:0,transform:"translate3d(-100%, 0, 0)",visibility:"visible"},{offset:1,transform:"translate3d(0, 0, 0)"}],Yp=[{offset:0,transform:"translate3d(100%, 0, 0)",visibility:"visible"},{offset:1,transform:"translate3d(0, 0, 0)"}],Gp=[{offset:0,transform:"translate3d(0, 100%, 0)",visibility:"visible"},{offset:1,transform:"translate3d(0, 0, 0)"}],Xp=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:1,visibility:"hidden",transform:"translate3d(0, 100%, 0)"}],Zp=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:1,visibility:"hidden",transform:"translate3d(-100%, 0, 0)"}],Qp=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:1,visibility:"hidden",transform:"translate3d(100%, 0, 0)"}],Jp=[{offset:0,transform:"translate3d(0, 0, 0)"},{offset:1,visibility:"hidden",transform:"translate3d(0, -100%, 0)"}],tf=[{offset:0,easing:"ease-in-out"},{offset:.2,transform:"rotate3d(0, 0, 1, 80deg)",easing:"ease-in-out"},{offset:.4,transform:"rotate3d(0, 0, 1, 60deg)",easing:"ease-in-out",opacity:"1"},{offset:.6,transform:"rotate3d(0, 0, 1, 80deg)",easing:"ease-in-out"},{offset:.8,transform:"rotate3d(0, 0, 1, 60deg)",easing:"ease-in-out",opacity:"1"},{offset:1,transform:"translate3d(0, 700px, 0)",opacity:"0"}],ef=[{offset:0,opacity:"0",transform:"scale(0.1) rotate(30deg)","transform-origin":"center bottom"},{offset:.5,transform:"rotate(-10deg)"},{offset:.7,transform:"rotate(3deg)"},{offset:1,opacity:"1",transform:"scale(1)"}],sf=[{offset:0,opacity:"0",transform:"translate3d(-100%, 0, 0) rotate3d(0, 0, 1, -120deg)"},{offset:1,opacity:"1",transform:"translate3d(0, 0, 0)"}],of=[{offset:0,opacity:"1"},{offset:1,opacity:"0",transform:"translate3d(100%, 0, 0) rotate3d(0, 0, 1, 120deg)"}],rf=[{offset:0,opacity:"0",transform:"scale3d(0.3, 0.3, 0.3)"},{offset:.5,opacity:"1"}],af=[{offset:0,opacity:"0",transform:"scale3d(0.1, 0.1, 0.1) translate3d(0, -1000px, 0)",easing:"cubic-bezier(0.55, 0.055, 0.675, 0.19)"},{offset:.6,opacity:"1",transform:"scale3d(0.475, 0.475, 0.475) translate3d(0, 60px, 0)",easing:"cubic-bezier(0.175, 0.885, 0.32, 1)"}],nf=[{offset:0,opacity:"0",transform:"scale3d(0.1, 0.1, 0.1) translate3d(-1000px, 0, 0)",easing:"cubic-bezier(0.55, 0.055, 0.675, 0.19)"},{offset:.6,opacity:"1",transform:"scale3d(0.475, 0.475, 0.475) translate3d(10px, 0, 0)",easing:"cubic-bezier(0.175, 0.885, 0.32, 1)"}],lf=[{offset:0,opacity:"0",transform:"scale3d(0.1, 0.1, 0.1) translate3d(1000px, 0, 0)",easing:"cubic-bezier(0.55, 0.055, 0.675, 0.19)"},{offset:.6,opacity:"1",transform:"scale3d(0.475, 0.475, 0.475) translate3d(-10px, 0, 0)",easing:"cubic-bezier(0.175, 0.885, 0.32, 1)"}],cf=[{offset:0,opacity:"0",transform:"scale3d(0.1, 0.1, 0.1) translate3d(0, 1000px, 0)",easing:"cubic-bezier(0.55, 0.055, 0.675, 0.19)"},{offset:.6,opacity:"1",transform:"scale3d(0.475, 0.475, 0.475) translate3d(0, -60px, 0)",easing:"cubic-bezier(0.175, 0.885, 0.32, 1)"}],df=[{offset:0,opacity:"1"},{offset:.5,opacity:"0",transform:"scale3d(0.3, 0.3, 0.3)"},{offset:1,opacity:"0"}],hf=[{offset:.4,opacity:"1",transform:"scale3d(0.475, 0.475, 0.475) translate3d(0, -60px, 0)",easing:"cubic-bezier(0.55, 0.055, 0.675, 0.19)"},{offset:1,opacity:"0",transform:"scale3d(0.1, 0.1, 0.1) translate3d(0, 2000px, 0)",easing:"cubic-bezier(0.175, 0.885, 0.32, 1)"}],uf=[{offset:.4,opacity:"1",transform:"scale3d(0.475, 0.475, 0.475) translate3d(42px, 0, 0)"},{offset:1,opacity:"0",transform:"scale(0.1) translate3d(-2000px, 0, 0)"}],pf=[{offset:.4,opacity:"1",transform:"scale3d(0.475, 0.475, 0.475) translate3d(-42px, 0, 0)"},{offset:1,opacity:"0",transform:"scale(0.1) translate3d(2000px, 0, 0)"}],ff=[{offset:.4,opacity:"1",transform:"scale3d(0.475, 0.475, 0.475) translate3d(0, 60px, 0)",easing:"cubic-bezier(0.55, 0.055, 0.675, 0.19)"},{offset:1,opacity:"0",transform:"scale3d(0.1, 0.1, 0.1) translate3d(0, -2000px, 0)",easing:"cubic-bezier(0.175, 0.885, 0.32, 1)"}],ml={linear:"linear",ease:"ease",easeIn:"ease-in",easeOut:"ease-out",easeInOut:"ease-in-out",easeInSine:"cubic-bezier(0.47, 0, 0.745, 0.715)",easeOutSine:"cubic-bezier(0.39, 0.575, 0.565, 1)",easeInOutSine:"cubic-bezier(0.445, 0.05, 0.55, 0.95)",easeInQuad:"cubic-bezier(0.55, 0.085, 0.68, 0.53)",easeOutQuad:"cubic-bezier(0.25, 0.46, 0.45, 0.94)",easeInOutQuad:"cubic-bezier(0.455, 0.03, 0.515, 0.955)",easeInCubic:"cubic-bezier(0.55, 0.055, 0.675, 0.19)",easeOutCubic:"cubic-bezier(0.215, 0.61, 0.355, 1)",easeInOutCubic:"cubic-bezier(0.645, 0.045, 0.355, 1)",easeInQuart:"cubic-bezier(0.895, 0.03, 0.685, 0.22)",easeOutQuart:"cubic-bezier(0.165, 0.84, 0.44, 1)",easeInOutQuart:"cubic-bezier(0.77, 0, 0.175, 1)",easeInQuint:"cubic-bezier(0.755, 0.05, 0.855, 0.06)",easeOutQuint:"cubic-bezier(0.23, 1, 0.32, 1)",easeInOutQuint:"cubic-bezier(0.86, 0, 0.07, 1)",easeInExpo:"cubic-bezier(0.95, 0.05, 0.795, 0.035)",easeOutExpo:"cubic-bezier(0.19, 1, 0.22, 1)",easeInOutExpo:"cubic-bezier(1, 0, 0, 1)",easeInCirc:"cubic-bezier(0.6, 0.04, 0.98, 0.335)",easeOutCirc:"cubic-bezier(0.075, 0.82, 0.165, 1)",easeInOutCirc:"cubic-bezier(0.785, 0.135, 0.15, 0.86)",easeInBack:"cubic-bezier(0.6, -0.28, 0.735, 0.045)",easeOutBack:"cubic-bezier(0.175, 0.885, 0.32, 1.275)",easeInOutBack:"cubic-bezier(0.68, -0.55, 0.265, 1.55)"},mf=Object.freeze(Object.defineProperty({__proto__:null,backInDown:Mu,backInLeft:Nu,backInRight:Fu,backInUp:Bu,backOutDown:Uu,backOutLeft:Hu,backOutRight:Vu,backOutUp:ju,bounce:$u,bounceIn:Wu,bounceInDown:qu,bounceInLeft:Ku,bounceInRight:Yu,bounceInUp:Gu,bounceOut:Xu,bounceOutDown:Zu,bounceOutLeft:Qu,bounceOutRight:Ju,bounceOutUp:tp,easings:ml,fadeIn:ep,fadeInBottomLeft:sp,fadeInBottomRight:ip,fadeInDown:op,fadeInDownBig:rp,fadeInLeft:ap,fadeInLeftBig:np,fadeInRight:lp,fadeInRightBig:cp,fadeInTopLeft:dp,fadeInTopRight:hp,fadeInUp:up,fadeInUpBig:pp,fadeOut:fp,fadeOutBottomLeft:mp,fadeOutBottomRight:gp,fadeOutDown:bp,fadeOutDownBig:vp,fadeOutLeft:yp,fadeOutLeftBig:wp,fadeOutRight:xp,fadeOutRightBig:_p,fadeOutTopLeft:kp,fadeOutTopRight:$p,fadeOutUp:Cp,fadeOutUpBig:Sp,flash:Cu,flip:Ap,flipInX:Tp,flipInY:zp,flipOutX:Ep,flipOutY:Op,headShake:Su,heartBeat:Au,hinge:tf,jackInTheBox:ef,jello:Tu,lightSpeedInLeft:Pp,lightSpeedInRight:Dp,lightSpeedOutLeft:Ip,lightSpeedOutRight:Rp,pulse:zu,rollIn:sf,rollOut:of,rotateIn:Lp,rotateInDownLeft:Mp,rotateInDownRight:Np,rotateInUpLeft:Fp,rotateInUpRight:Bp,rotateOut:Up,rotateOutDownLeft:Hp,rotateOutDownRight:Vp,rotateOutUpLeft:jp,rotateOutUpRight:Wp,rubberBand:Eu,shake:Ou,shakeX:Pu,shakeY:Du,slideInDown:qp,slideInLeft:Kp,slideInRight:Yp,slideInUp:Gp,slideOutDown:Xp,slideOutLeft:Zp,slideOutRight:Qp,slideOutUp:Jp,swing:Iu,tada:Ru,wobble:Lu,zoomIn:rf,zoomInDown:af,zoomInLeft:nf,zoomInRight:lf,zoomInUp:cf,zoomOut:df,zoomOutDown:hf,zoomOutLeft:uf,zoomOutRight:pf,zoomOutUp:ff},Symbol.toStringTag,{value:"Module"}));var gf=A`
  :host {
    display: contents;
  }
`,Ct=class extends P{constructor(){super(...arguments),this.hasStarted=!1,this.name="none",this.play=!1,this.delay=0,this.direction="normal",this.duration=1e3,this.easing="linear",this.endDelay=0,this.fill="auto",this.iterations=1/0,this.iterationStart=0,this.playbackRate=1,this.handleAnimationFinish=()=>{this.play=!1,this.hasStarted=!1,this.emit("sl-finish")},this.handleAnimationCancel=()=>{this.play=!1,this.hasStarted=!1,this.emit("sl-cancel")}}get currentTime(){var e,t;return(t=(e=this.animation)==null?void 0:e.currentTime)!=null?t:0}set currentTime(e){this.animation&&(this.animation.currentTime=e)}connectedCallback(){super.connectedCallback(),this.createAnimation()}disconnectedCallback(){super.disconnectedCallback(),this.destroyAnimation()}handleSlotChange(){this.destroyAnimation(),this.createAnimation()}async createAnimation(){var e,t;const s=(e=ml[this.easing])!=null?e:this.easing,i=(t=this.keyframes)!=null?t:mf[this.name],r=(await this.defaultSlot).assignedElements()[0];return!r||!i?!1:(this.destroyAnimation(),this.animation=r.animate(i,{delay:this.delay,direction:this.direction,duration:this.duration,easing:s,endDelay:this.endDelay,fill:this.fill,iterationStart:this.iterationStart,iterations:this.iterations}),this.animation.playbackRate=this.playbackRate,this.animation.addEventListener("cancel",this.handleAnimationCancel),this.animation.addEventListener("finish",this.handleAnimationFinish),this.play?(this.hasStarted=!0,this.emit("sl-start")):this.animation.pause(),!0)}destroyAnimation(){this.animation&&(this.animation.cancel(),this.animation.removeEventListener("cancel",this.handleAnimationCancel),this.animation.removeEventListener("finish",this.handleAnimationFinish),this.hasStarted=!1)}handleAnimationChange(){this.hasUpdated&&this.createAnimation()}handlePlayChange(){return this.animation?(this.play&&!this.hasStarted&&(this.hasStarted=!0,this.emit("sl-start")),this.play?this.animation.play():this.animation.pause(),!0):!1}handlePlaybackRateChange(){this.animation&&(this.animation.playbackRate=this.playbackRate)}cancel(){var e;(e=this.animation)==null||e.cancel()}finish(){var e;(e=this.animation)==null||e.finish()}render(){return v` <slot @slotchange=${this.handleSlotChange}></slot> `}};Ct.styles=[M,gf];n([wc("slot")],Ct.prototype,"defaultSlot",2);n([d()],Ct.prototype,"name",2);n([d({type:Boolean,reflect:!0})],Ct.prototype,"play",2);n([d({type:Number})],Ct.prototype,"delay",2);n([d()],Ct.prototype,"direction",2);n([d({type:Number})],Ct.prototype,"duration",2);n([d()],Ct.prototype,"easing",2);n([d({attribute:"end-delay",type:Number})],Ct.prototype,"endDelay",2);n([d()],Ct.prototype,"fill",2);n([d({type:Number})],Ct.prototype,"iterations",2);n([d({attribute:"iteration-start",type:Number})],Ct.prototype,"iterationStart",2);n([d({attribute:!1})],Ct.prototype,"keyframes",2);n([d({attribute:"playback-rate",type:Number})],Ct.prototype,"playbackRate",2);n([C(["name","delay","direction","duration","easing","endDelay","fill","iterations","iterationsStart","keyframes"])],Ct.prototype,"handleAnimationChange",1);n([C("play")],Ct.prototype,"handlePlayChange",1);n([C("playbackRate")],Ct.prototype,"handlePlaybackRateChange",1);Ct.define("sl-animation");class bf{constructor(t){this.client=t}async find(t=0,s=20){return await(await this.client.get("/assistants",{params:{skip:t,limit:s}})).json()}async get(t){return await(await this.client.get(`/assistants/${t}`)).json()}async upsert(t){return await(await this.client.post("/assistants",t)).json()}async delete(t){await this.client.delete(`/assistants/${t}`)}async message(t,s,i){return await this.client.post(`/assistants/${t}/chat`,s,{onReceive:i})}}class vf{constructor(t){this.controller=t}async findCollections(t,s=10,i=0,o="desc"){return await(await this.controller.get("/memory",{params:{limit:s,skip:i,order:o,name:t??""}})).json()}async getCollection(t){return await(await this.controller.get(`/memory/${t}`)).json()}async upsertCollection(t){return await(await this.controller.post("/memory",t)).json()}async deleteCollection(t){await this.controller.delete(`/memory/${t}`)}async searchCollection(t,s){return await this.controller.post(`/memory/${t}/search`,{query:s})}async findDocuments(t,s,i=10,o=0,r="desc"){return await(await this.controller.get(`/memory/${t}/documents`,{params:{limit:i,skip:o,order:r,name:s??""}})).json()}async uploadDocument(t,s,i=[],o){const r=new FormData;return s.forEach(a=>{r.append("files",a)}),i.forEach(a=>{r.append("tags",a)}),await this.controller.post(`/memory/${t}/documents`,r,{onProgress:o})}async deleteDocument(t,s){await this.controller.delete(`/memory/${t}/documents/${s}`)}}class yf{constructor(t){this.client=t}async getModels(t){const s=t==="chat"?"/models/chat":"/models/embedding";return await(await this.client.get(s)).json()}}class wf{constructor(t){this.xhr=t}get ok(){return this.status>=200&&this.status<300}get status(){return this.xhr.status}get headers(){const t=new Headers;return this.xhr.getAllResponseHeaders().trim().split(/[\r\n]+/).forEach(i=>{const o=i.split(": "),r=o.shift(),a=o.join(": ");r&&a&&t.append(r,a)}),t}get url(){return this.xhr.responseURL}get redirected(){return!1}text(){return Promise.resolve(this.xhr.responseText)}bytes(){return Promise.resolve(new Uint8Array(this.xhr.response))}blob(){return Promise.resolve(this.xhr.response)}arrayBuffer(){return Promise.resolve(this.xhr.response)}formData(){return Promise.resolve(this.xhr.response)}json(){return Promise.resolve(JSON.parse(this.xhr.responseText))}cancel(){this.xhr.abort()}}class xf{constructor(t,s){this.response=t,this.controller=s}get ok(){return this.response.ok}get status(){return this.response.status}get headers(){return this.response.headers}get url(){return this.response.url}get redirected(){return this.response.redirected}text(){return this.response.text()}bytes(){return this.response.bytes()}blob(){return this.response.blob()}arrayBuffer(){return this.response.arrayBuffer()}formData(){return this.response.formData()}json(){return this.response.json()}cancel(){this.controller.abort()}}class _f{constructor(t){this.baseUrl=t.baseUrl,this.headers=t.headers,this.timeout=t.timeout,this.credentials=t.credentials,this.mode=t.mode,this.cache=t.cache,this.keepalive=t.keepalive}get(t,s){const i=this.buildRequest("GET",t,void 0,s);return this.send(i)}post(t,s,i){const o=this.buildRequest("POST",t,s,i);return this.send(o)}put(t,s,i){const o=this.buildRequest("PUT",t,s,i);return this.send(o)}patch(t,s,i){const o=this.buildRequest("PATCH",t,s,i);return this.send(o)}delete(t,s){const i=this.buildRequest("DELETE",t,void 0,s);return this.send(i)}async send(t){return t.body instanceof FormData?this.sendXhr(t):this.sendFetch(t)}async sendFetch(t){const s=this.buildUrl(t.path,t.params),i=new Headers(t.headers);let o=t.body;o&&!(o instanceof FormData)&&typeof o=="object"&&!(o instanceof Blob)&&(o instanceof URLSearchParams?(i.set("Content-Type","application/x-www-form-urlencoded"),o=o.toString()):(i.set("Content-Type","application/json"),o=JSON.stringify(o)));const r=new AbortController,a=r.signal,l=t.timeout?setTimeout(()=>r.abort(),t.timeout):null;try{const c=await fetch(s.toString(),{method:t.method,headers:i,body:o,cache:t.cache,credentials:t.credentials,mode:t.mode,keepalive:t.keepalive,signal:a});if(t.onReceive&&c.body){const h=c.body.getReader(),u=new TextDecoder("utf-8");let p="";(async()=>{for(;;){const{done:m,value:b}=await h.read();if(m)break;p+=u.decode(b,{stream:!0});let y=p.split(`
`);p=y.pop()||"";for(const k of y)if(k.trim())try{const z=JSON.parse(k);t.onReceive(z)}catch(z){console.error("Failed to parse JSON:",z)}}if(p.trim())try{const m=JSON.parse(p);t.onReceive(m)}catch(m){console.error("Failed to parse JSON:",m)}})()}return new xf(c,r)}catch(c){throw c}finally{l&&clearTimeout(l)}}sendXhr(t){return new Promise((s,i)=>{const o=this.buildUrl(t.path,t.params),r=new XMLHttpRequest;if(r.open(t.method,o.toString(),!0),t.body&&(t.body instanceof FormData||typeof t.body=="object"&&(r.setRequestHeader("Content-Type","application/json"),t.body=JSON.stringify(t.body))),t.headers)for(const[a,l]of Object.entries(t.headers))r.setRequestHeader(a,l);if(t.credentials&&(r.withCredentials=t.credentials!=="omit"),t.timeout&&(r.timeout=t.timeout),t.onProgress&&r.upload&&(r.upload.onprogress=a=>{if(a.lengthComputable){const l=a.loaded/a.total;t.onProgress(l)}else t.onProgress(-1)}),t.onReceive&&"responseType"in r){r.responseType="text";let a=0;r.onprogress=()=>{const l=r.responseText.slice(a);a=r.responseText.length,l.split(`
`).forEach(h=>{if(h.trim())try{const u=JSON.parse(h);t.onReceive(u)}catch(u){console.error("Failed to parse JSON:",u)}})}}r.onload=()=>{s(new wf(r))},r.onerror=()=>{i(new Error("Network error"))},r.ontimeout=()=>{i(new Error("Request timed out"))};try{r.send(t.body)}catch(a){i(a)}})}buildRequest(t,s,i,o){return{method:t,path:s,body:i,headers:(o==null?void 0:o.headers)||this.headers,params:o==null?void 0:o.params,credentials:(o==null?void 0:o.credentials)||this.credentials,cache:(o==null?void 0:o.cache)||this.cache,mode:(o==null?void 0:o.mode)||this.mode,keepalive:(o==null?void 0:o.keepalive)||this.keepalive,timeout:(o==null?void 0:o.timeout)||this.timeout,onReceive:o==null?void 0:o.onReceive,onProgress:o==null?void 0:o.onProgress}}buildUrl(t,s){let i=this.baseUrl.endsWith("/")&&t.startsWith("/")?this.baseUrl+t.slice(1):this.baseUrl+t;const o=new URL(i);return s&&Object.keys(s).forEach(r=>{const a=s[r];Array.isArray(a)?a.forEach(l=>o.searchParams.append(r,String(l))):a!=null&&o.searchParams.append(r,String(a))}),o}}const $e=class $e{static getBaseUrl(){let t,s;return t=window.location.origin,s="v1",console.log("API Host:",t),console.log("API Version:",s),console.log("API Origin:",window.location.origin),new URL(`${s}`,t).toString()}};$e.controller=new _f({baseUrl:$e.getBaseUrl()}),$e.System=new yf($e.controller),$e.Assistant=new bf($e.controller),$e.Memory=new vf($e.controller);let Dt=$e;class Wt{static setTheme(t){localStorage.setItem("app-theme",t),t==="light"&&document.body.classList.remove("sl-theme-dark"),t==="dark"&&!document.body.classList.contains("sl-theme-dark")&&document.body.classList.add("sl-theme-dark")}static getTheme(){const t=localStorage.getItem("app-theme");return t||(this.setTheme("light"),"light")}static alert(t,s="danger"){const i=Object.assign(document.createElement("sl-alert"),{variant:s,closable:!0,duration:3e3,innerHTML:`<span>${t}</span>`});return document.body.append(i),i.toast()}}const fn=e=>{if(!e)return"N/A";const t=new Date(e),s=new Date,i=e.endsWith("Z")?0:s.getTimezoneOffset()*60*1e3,o=Math.floor((s.getTime()-(t.getTime()-i))/1e3);if(o<0)return"N/A";if(o<60)return`${o} seconds ago`;const r=Math.floor(o/60);if(r<60)return`${r} minutes ago`;const a=Math.floor(r/60);if(a<24)return`${a} hours ago`;const l=Math.floor(a/24);if(l<30)return`${l} days ago`;const c=Math.floor(l/30);return c<12?`${c} months ago`:`${Math.floor(c/12)} years ago`},gl=e=>e?e.lastUpdatedAt?fn(e.lastUpdatedAt):e.createdAt?fn(e.createdAt):"N/A":"N/A",kf=e=>{if(!e)return"N/A";let t=new Date(e);const s=e.endsWith("Z")?0:t.getTimezoneOffset()*60*1e3;return t=new Date(t.getTime()-s),new Intl.DateTimeFormat("ko-KR",{month:"2-digit",day:"2-digit",hour:"2-digit",minute:"2-digit"}).format(t)},xi=e=>{window.history.pushState(null,"",e),window.dispatchEvent(new PopStateEvent("popstate"))},$f=e=>JSON.parse(JSON.stringify(e));class Cf{constructor(){this.startTime=0,this.elapsedTime=0,this.timerInterval=null,this.laps=[]}timeToString(t){const s=t/36e5,i=Math.floor(s),o=(s-i)*60,r=Math.floor(o),a=(o-r)*60,l=Math.floor(a),c=(a-l)*100,h=Math.floor(c),u=r.toString().padStart(2,"0"),p=l.toString().padStart(2,"0"),f=h.toString().padStart(2,"0");return`${u}:${p}:${f}`}get elapsed(){return this.timeToString(this.elapsedTime)}start(){this.timerInterval||(this.startTime=Date.now()-this.elapsedTime,this.timerInterval=setInterval(()=>{this.elapsedTime=Date.now()-this.startTime},10))}stop(){this.timerInterval&&(clearInterval(this.timerInterval),this.timerInterval=null)}reset(){this.timerInterval&&(clearInterval(this.timerInterval),this.timerInterval=null),this.startTime=0,this.elapsedTime=0,this.laps=[],console.log("00:00:00")}lap(){if(this.timerInterval){const t=this.timeToString(this.elapsedTime);return this.laps.push(t),console.log(`Lap ${this.laps.length}: ${t}`),t}else return console.log("스탑워치가 작동 중이지 않습니다."),null}getLaps(){return[...this.laps]}}var Sf=Object.defineProperty,Af=Object.getOwnPropertyDescriptor,bl=(e,t,s,i)=>{for(var o=i>1?void 0:i?Af(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&Sf(t,s,o),o};let uo=class extends tt{constructor(){super(...arguments),this.onSelect=async()=>{var e;this.dispatchEvent(new CustomEvent("select",{detail:(e=this.collection)==null?void 0:e.id}))},this.onDelete=async e=>{var t;e.stopPropagation(),this.dispatchEvent(new CustomEvent("delete",{detail:(t=this.collection)==null?void 0:t.id}))}}render(){return this.collection?v`
      <div class="container"
        @click=${this.onSelect}>
        <div class="header">
          ${this.collection.name}
        </div>
        <div class="control">
          <sl-icon-button
            name="trash"
            @click=${this.onDelete}
          ></sl-icon-button>
        </div>
        <div class="description">
          ${this.collection.description}
        </div>
        <div class="meta">
          <strong>${this.collection.embedService}</strong>
          ${this.collection.embedModel}
        </div>
        <div class="date">
          ${gl(this.collection)}
        </div>
      </div>
    `:G}};uo.styles=A`
    :host {
      display: contents;
    }

    .container {
      display: grid;
      grid-template-areas:
        "header control"
        "description description"
        "meta date";
      grid-template-columns: 70% 30%;
      grid-template-rows: 20% 60% 20%;
      padding: 16px;
      width: 400px;
      height: 200px;
      border: 1px solid var(--sl-color-gray-200);
      background-color: var(--sl-panel-background-color);
      border-radius: 8px;
      box-sizing: border-box;
      font-size: 18px;
      font-family: Arial, sans-serif;
      overflow: hidden;
      cursor: pointer;
    }
    .container:hover {
      scale: 1.05;
      background-color: var(--sl-color-gray-50);
    }
    .container:active {
      scale: 1.0;
    }

    .header {
      grid-area: header;
      align-content: center;
      font-size: 1em;
      font-weight: 600;
    }

    .control {
      grid-area: control;
      text-align: right;
    }

    .description {
      grid-area: description;
      font-size: 0.8em;
    }

    .meta {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      justify-content: flex-end;
      grid-area: meta;
      font-size: 0.8em;
    }

    .date {
      display: flex;
      align-items: flex-end;
      justify-content: flex-end;
      grid-area: date;
      font-size: 0.8em;
      text-align: right;
    }
  `;bl([d({type:Object})],uo.prototype,"collection",2);uo=bl([lt("collection-card")],uo);var Tf=Object.defineProperty,zf=Object.getOwnPropertyDescriptor,vl=(e,t,s,i)=>{for(var o=i>1?void 0:i?zf(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&Tf(t,s,o),o};let po=class extends tt{constructor(){super(...arguments),this.onDelete=async e=>{var t;e.stopPropagation(),(t=this.assistant)!=null&&t.id&&this.dispatchEvent(new CustomEvent("delete",{detail:this.assistant.id}))},this.onSelect=async e=>{var t;e.stopPropagation(),(t=this.assistant)!=null&&t.id&&this.dispatchEvent(new CustomEvent("select",{detail:this.assistant.id}))}}render(){return this.assistant?v`
      <div class="container"
        @click=${this.onSelect}>
        <div class="header">
          ${this.assistant.name}
        </div>
        <div class="control">
          <sl-icon-button
            name="trash"
            @click=${this.onDelete}
          ></sl-icon-button>
        </div>
        <div class="description">
          ${this.assistant.description}
        </div>
        <div class="meta">
          <strong>${this.assistant.service}</strong>
          ${this.assistant.model}
        </div>
        <div class="date">
          ${gl(this.assistant)}
        </div>
      </div>
    `:G}};po.styles=A`
    :host {
      display: contents;
    }

    .container {
      display: grid;
      grid-template-areas:
        "header control"
        "description description"
        "meta date";
      grid-template-columns: 70% 30%;
      grid-template-rows: 20% 60% 20%;
      padding: 16px;
      width: 350px;
      height: 200px;
      border: 1px solid var(--sl-color-gray-200);
      background-color: var(--sl-panel-background-color);
      border-radius: 8px;
      box-sizing: border-box;
      font-size: 18px;
      font-family: Arial, sans-serif;
      overflow: hidden;
      cursor: pointer;
    }
    .container:hover {
      scale: 1.05;
      background-color: var(--sl-color-gray-50);
    }
    .container:active {
      scale: 1.0;
    }

    .header {
      grid-area: header;
      align-content: center;
      font-size: 1em;
      font-weight: 600;
    }

    .control {
      grid-area: control;
      text-align: right;
    }

    .description {
      grid-area: description;
      font-size: 0.8em;
    }

    .meta {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      justify-content: flex-end;
      grid-area: meta;
      font-size: 0.8em;
    }

    .date {
      display: flex;
      align-items: flex-end;
      justify-content: flex-end;
      grid-area: date;
      font-size: 0.8em;
      text-align: right;
    }
  `;vl([d({type:Object})],po.prototype,"assistant",2);po=vl([lt("assistant-card")],po);var Ef=Object.defineProperty,Of=Object.getOwnPropertyDescriptor,Bs=(e,t,s,i)=>{for(var o=i>1?void 0:i?Of(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&Ef(t,s,o),o};let Ke=class extends tt{constructor(){super(...arguments),this.name="",this.label="",this.helpText="",this.disabled=!1,this.checked=!1,this.onChange=async e=>{this.checked=e.target.checked,this.dispatchEvent(new CustomEvent("change",{detail:this.checked}))}}render(){return v`
      <label>
        <input type="checkbox"
          name=${this.name}
          ?checked=${this.checked}
          ?disabled=${this.disabled}
          @change=${this.onChange}/>
        ${this.label}
      </label>
      <div class="help-text">${this.helpText}</div>
      <div class="body">
        <slot></slot>
      </div>
    `}};Ke.styles=A`
    :host {
      display: flex;
      flex-direction: column;
      gap: 4px;
      padding: 16px;
      border: var(--sl-input-border-width) solid var(--sl-input-border-color);
      border-radius: var(--sl-input-border-radius-small);
      background-color: var(--sl-input-background-color);
      box-sizing: border-box;
    }
    :host([checked]) .body {
      max-height: 500px;
      overflow: auto;
    }

    label {
      display: flex;
      flex-direction: row;
      align-items: center;
      font-size: 14px;
      line-height: 1.5;
      gap: 8px;
      cursor: pointer;

      input {
        width: 1.2em;
        height: 1.2em;
        margin: 0;
      }
    }

    .help-text {
      font-size: var(--sl-input-help-text-font-size-small);
      color: var(--sl-input-help-text-color);
    }

    .body {
      max-height: 0;
      overflow: hidden;
      transition: max-height 0.3s ease;
    }
  `;Bs([d({type:String})],Ke.prototype,"name",2);Bs([d({type:String})],Ke.prototype,"label",2);Bs([d({type:String,attribute:"help-text"})],Ke.prototype,"helpText",2);Bs([d({type:Boolean,reflect:!0})],Ke.prototype,"disabled",2);Bs([d({type:Boolean,reflect:!0})],Ke.prototype,"checked",2);Ke=Bs([lt("checkbox-option")],Ke);/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */class Pf{constructor(t){this.Y=t}disconnect(){this.Y=void 0}reconnect(t){this.Y=t}deref(){return this.Y}}class Df{constructor(){this.Z=void 0,this.q=void 0}get(){return this.Z}pause(){this.Z??(this.Z=new Promise(t=>this.q=t))}resume(){var t;(t=this.q)==null||t.call(this),this.Z=this.q=void 0}}/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const mn=e=>!Ud(e)&&typeof e.then=="function",gn=1073741823;class If extends cl{constructor(){super(...arguments),this._$Cwt=gn,this._$Cbt=[],this._$CK=new Pf(this),this._$CX=new Df}render(...t){return t.find(s=>!mn(s))??Nt}update(t,s){const i=this._$Cbt;let o=i.length;this._$Cbt=s;const r=this._$CK,a=this._$CX;this.isConnected||this.disconnected();for(let l=0;l<s.length&&!(l>this._$Cwt);l++){const c=s[l];if(!mn(c))return this._$Cwt=l,c;l<o&&c===i[l]||(this._$Cwt=gn,o=0,Promise.resolve(c).then(async h=>{for(;a.get();)await a.get();const u=r.deref();if(u!==void 0){const p=u._$Cbt.indexOf(c);p>-1&&p<u._$Cwt&&(u._$Cwt=p,u.setValue(h))}}))}return Nt}disconnected(){this._$CK.disconnect(),this._$CX.pause()}reconnected(){this._$CK.reconnect(this),this._$CX.resume()}}const Kr=Ls(If);var Rf=Object.defineProperty,Lf=Object.getOwnPropertyDescriptor,Be=(e,t,s,i)=>{for(var o=i>1?void 0:i?Lf(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&Rf(t,s,o),o};let vt=class extends tt{constructor(){super(...arguments),this.type="embed",this.label="",this.name="",this.size="small",this.placeholder="",this.required=!1,this.disabled=!1,this.value="",this.getModels=async()=>this.type==="chat"?(vt.cmodels??(vt.cmodels=await Dt.System.getModels("chat")),vt.cmodels):(vt.emodels??(vt.emodels=await Dt.System.getModels("embed")),vt.emodels),this.onChange=async e=>{e.preventDefault(),e.stopPropagation(),this.value=e.target.value;const[t,s]=this.value.split("/"),i={provider:t,model:s};this.dispatchEvent(new CustomEvent("change",{detail:i}))}}render(){return Kr(this.getModels().then(e=>e?v`
        <sl-select
          label=${this.label}
          name=${this.name}
          size=${this.size}
          placeholder=${this.placeholder}
          ?required=${this.required}
          ?disabled=${this.disabled}
          value=${this.value}
          .hoist=${!0}
          @sl-change=${this.onChange}
        >
          ${Object.entries(e).map(([t,s])=>v`
            <small>${t}</small>
            ${s.map(i=>v`
              <sl-option value=${`${t}/${i}`}>${i}</sl-option>
            `)}
          `)}
        </sl-select>
      `:G),v`
      <sl-skeleton 
        effect="pulse"
        style="width: 100%; height: 30px;"
      ></sl-skeleton>
    `)}};vt.styles=A`
    :host {
      width: 100%;
    }
  `;Be([d({type:String})],vt.prototype,"type",2);Be([d({type:String})],vt.prototype,"label",2);Be([d({type:String})],vt.prototype,"name",2);Be([d({type:String})],vt.prototype,"size",2);Be([d({type:String})],vt.prototype,"placeholder",2);Be([d({type:Boolean})],vt.prototype,"required",2);Be([d({type:Boolean})],vt.prototype,"disabled",2);Be([d({type:String})],vt.prototype,"value",2);vt=Be([lt("model-select")],vt);var Mf=Object.defineProperty,Nf=Object.getOwnPropertyDescriptor,Po=(e,t,s,i)=>{for(var o=i>1?void 0:i?Nf(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&Mf(t,s,o),o};let Os=class extends tt{constructor(){super(...arguments),this.open=!1,this.loading=!1,this.files=[],this.onDragOver=e=>{e.preventDefault()},this.onDragLeave=e=>{e.preventDefault()},this.onDrop=e=>{var s;e.preventDefault();const t=Array.from(((s=e.dataTransfer)==null?void 0:s.files)||[]);this.files=[...this.files,...t]},this.onDelete=async e=>{this.files=this.files.filter(t=>t!==e)},this.onClick=async()=>{const e=document.createElement("input");e.type="file",e.multiple=!0,e.accept="*/*",e.addEventListener("change",this.onSelect.bind(this)),e.click()},this.onHide=async()=>{this.dispatchEvent(new CustomEvent("close"))},this.onUpload=async()=>{this.dispatchEvent(new CustomEvent("upload",{detail:this.files}))}}render(){return v`
      <sl-dialog label="Upload Files"
        ?open=${this.open} 
        @sl-hide=${this.onHide}>
        <div class="drop-zone"
          @click=${this.onClick}
          @drop=${this.onDrop} 
          @dragover=${this.onDragOver}
          @dragleave=${this.onDragLeave}>
          Drop files here OR click to upload
        </div>
        <div class="files">
          ${this.files.map(e=>v`
              <div class="item">
                <div class="name">
                  ${e.name}
                </div>
                <sl-icon-button 
                  name="trash"
                  @click=${()=>this.onDelete(e)}
                ></sl-icon-button>
              </div>
            `)}
        </div>
        <div class="control" slot="footer">
          <sl-button size="small" 
            @click=${this.onHide}>
            Cancel
          </sl-button>
          <sl-button size="small" 
            variant="primary"
            @click=${this.onUpload}>
            Upload
          </sl-button>
        </div>
      </sl-dialog>
    `}onSelect(e){const t=e.target,s=Array.from(t.files||[]);this.files=[...this.files,...s]}};Os.styles=A`
    sl-dialog::part(panel) {
      width: 500px;
      height: 500px;
    }
    sl-dialog::part(body) {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .drop-zone {
      width: 100%;
      height: 100px;
      border: 2px dashed var(--sl-color-gray-300);
      display: flex;
      align-items: center;
      justify-content: center;
      box-sizing: border-box;
      cursor: pointer;
    }
    
    .files {
      height: calc(100% - 100px);
      display: flex;
      flex-direction: column;
      border: 1px solid var(--sl-color-gray-300);
      font-size: 14px;
      overflow: auto;

      .item {
        display: flex;
        flex-direction: row;
        align-items: center;
        justify-content: space-between;
        gap: 4px;
        padding: 0px 8px;
        box-sizing: border-box;
      }
    }

    .control {
      display: flex;
      flex-direction: row;
      gap: 8px;
      justify-content: flex-end;
    }
  `;Po([d({type:Boolean,reflect:!0})],Os.prototype,"open",2);Po([d({type:Boolean,reflect:!0})],Os.prototype,"loading",2);Po([E()],Os.prototype,"files",2);Os=Po([lt("file-uploader")],Os);var Ff=Object.defineProperty,Bf=Object.getOwnPropertyDescriptor,yl=(e,t,s,i)=>{for(var o=i>1?void 0:i?Bf(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&Ff(t,s,o),o};let fo=class extends tt{constructor(){super(...arguments),this.appendContent=e=>{var s;(s=this.message).content??(s.content=[]);const t=this.message.content.find(i=>i.index===e.index);if(t)if(t.type==="text"&&e.type==="text")t.text??(t.text=""),t.text+=e.text;else if(t.type==="image"&&e.type==="image")t.data=e.data;else if(t.type==="tool"&&e.type==="tool")t.id=e.id,t.name=e.name,t.arguments=e.arguments,t.result=e.result;else throw new Error("TODO: Implement appendContent for other types");else{const i=$f(e);this.message.content.push(i)}this.requestUpdate()}}render(){var e,t,s;return this.message?v`
      <div class="container">
        <img class="avatar"
          src="/assets/images/bot-avatar.png" />
        <div class="name">
          ${((e=this.message)==null?void 0:e.name)||"Assistant"}
        </div>
        <div class="content">
          ${(s=(t=this.message)==null?void 0:t.content)==null?void 0:s.map(i=>i.type==="text"?v`
                <markdown-block
                  .content=${i.text||""}
                ></markdown-block>
              `:i.type==="image"?v`
                <image-block
                  data=${i.data||""}
                ></image-block>
              `:i.type==="tool"?v`
                <tool-block
                  .name=${i.name||""}
                  .result=${i.result||""}
                ></tool-block>
              `:G)}
        </div>
      </div>
    `:G}};fo.styles=A`
    :host {
      width: 100%;
      display: block;
    }

    .container {
      display: grid;
      grid-template-areas: 
        "avatar name"
        "avatar content";
      grid-template-columns: auto 1fr;
      grid-template-rows: auto 1fr;
      gap: 12px;
    }

    .avatar {
      grid-area: avatar;
      width: 40px;
      height: 40px;
    }

    .name {
      width: 100%;
      grid-area: name;
      font-size: 12px;
      font-weight: 600;
      color: var(--sl-color-gray-600);
    }

    .content {
      width: 100%;
      grid-area: content;
      display: flex;
      flex-direction: column;
      gap: 4px;
      border-radius: 4px;
      padding: 8px;
      background-color: var(--sl-color-gray-100);
      border: 1px solid var(--sl-color-gray-200);
      box-sizing: border-box;
    }
  `;yl([d({type:Object})],fo.prototype,"message",2);fo=yl([lt("assistant-message")],fo);var Uf=Object.defineProperty,Hf=Object.getOwnPropertyDescriptor,wl=(e,t,s,i)=>{for(var o=i>1?void 0:i?Hf(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&Uf(t,s,o),o};let mo=class extends tt{render(){var e,t,s;return v`
      <div class="container">
        <img class="avatar"
          src="/assets/images/user-avatar.png" />
        <div class="name">
          ${((e=this.message)==null?void 0:e.name)||"User"}
        </div>
        <div class="content">
          ${(s=(t=this.message)==null?void 0:t.content)==null?void 0:s.map(i=>i.type==="text"?v`
                <text-block
                  .content=${i.text||""}
                ></text-block>
              `:G)}
        </div>
      </div>
    `}};mo.styles=A`
    :host {
      width: 100%;
      display: block;
    }

    .container {
      display: grid;
      grid-template-areas: 
        "avatar name"
        "avatar content";
      grid-template-columns: auto 1fr;
      grid-template-rows: auto 1fr;
      gap: 12px;
    }

    .avatar {
      grid-area: avatar;
      width: 40px;
      height: 40px;
      border-radius: 50%;
    }

    .name {
      width: 100%;
      grid-area: name;
      font-size: 12px;
      font-weight: 600;
      color: var(--sl-color-gray-600);
    }

    .content {
      width: 100%;
      grid-area: content;
      display: flex;
      flex-direction: column;
      gap: 4px;
      border-radius: 4px;
      padding: 8px;
      background-color: var(--sl-color-gray-100);
      border: 1px solid var(--sl-color-gray-200);
      box-sizing: border-box;
    }
  `;wl([d({type:Object})],mo.prototype,"message",2);mo=wl([lt("user-message")],mo);/*! @license DOMPurify 3.2.2 | (c) Cure53 and other contributors | Released under the Apache license 2.0 and Mozilla Public License 2.0 | github.com/cure53/DOMPurify/blob/3.2.2/LICENSE */const{entries:xl,setPrototypeOf:bn,isFrozen:Vf,getPrototypeOf:jf,getOwnPropertyDescriptor:Wf}=Object;let{freeze:Ut,seal:le,create:_l}=Object,{apply:$r,construct:Cr}=typeof Reflect<"u"&&Reflect;Ut||(Ut=function(t){return t});le||(le=function(t){return t});$r||($r=function(t,s,i){return t.apply(s,i)});Cr||(Cr=function(t,s){return new t(...s)});const Zi=Zt(Array.prototype.forEach),vn=Zt(Array.prototype.pop),Js=Zt(Array.prototype.push),io=Zt(String.prototype.toLowerCase),rr=Zt(String.prototype.toString),yn=Zt(String.prototype.match),ti=Zt(String.prototype.replace),qf=Zt(String.prototype.indexOf),Kf=Zt(String.prototype.trim),he=Zt(Object.prototype.hasOwnProperty),Mt=Zt(RegExp.prototype.test),ei=Yf(TypeError);function Zt(e){return function(t){for(var s=arguments.length,i=new Array(s>1?s-1:0),o=1;o<s;o++)i[o-1]=arguments[o];return $r(e,t,i)}}function Yf(e){return function(){for(var t=arguments.length,s=new Array(t),i=0;i<t;i++)s[i]=arguments[i];return Cr(e,s)}}function V(e,t){let s=arguments.length>2&&arguments[2]!==void 0?arguments[2]:io;bn&&bn(e,null);let i=t.length;for(;i--;){let o=t[i];if(typeof o=="string"){const r=s(o);r!==o&&(Vf(t)||(t[i]=r),o=r)}e[o]=!0}return e}function Gf(e){for(let t=0;t<e.length;t++)he(e,t)||(e[t]=null);return e}function Je(e){const t=_l(null);for(const[s,i]of xl(e))he(e,s)&&(Array.isArray(i)?t[s]=Gf(i):i&&typeof i=="object"&&i.constructor===Object?t[s]=Je(i):t[s]=i);return t}function si(e,t){for(;e!==null;){const i=Wf(e,t);if(i){if(i.get)return Zt(i.get);if(typeof i.value=="function")return Zt(i.value)}e=jf(e)}function s(){return null}return s}const wn=Ut(["a","abbr","acronym","address","area","article","aside","audio","b","bdi","bdo","big","blink","blockquote","body","br","button","canvas","caption","center","cite","code","col","colgroup","content","data","datalist","dd","decorator","del","details","dfn","dialog","dir","div","dl","dt","element","em","fieldset","figcaption","figure","font","footer","form","h1","h2","h3","h4","h5","h6","head","header","hgroup","hr","html","i","img","input","ins","kbd","label","legend","li","main","map","mark","marquee","menu","menuitem","meter","nav","nobr","ol","optgroup","option","output","p","picture","pre","progress","q","rp","rt","ruby","s","samp","section","select","shadow","small","source","spacer","span","strike","strong","style","sub","summary","sup","table","tbody","td","template","textarea","tfoot","th","thead","time","tr","track","tt","u","ul","var","video","wbr"]),ar=Ut(["svg","a","altglyph","altglyphdef","altglyphitem","animatecolor","animatemotion","animatetransform","circle","clippath","defs","desc","ellipse","filter","font","g","glyph","glyphref","hkern","image","line","lineargradient","marker","mask","metadata","mpath","path","pattern","polygon","polyline","radialgradient","rect","stop","style","switch","symbol","text","textpath","title","tref","tspan","view","vkern"]),nr=Ut(["feBlend","feColorMatrix","feComponentTransfer","feComposite","feConvolveMatrix","feDiffuseLighting","feDisplacementMap","feDistantLight","feDropShadow","feFlood","feFuncA","feFuncB","feFuncG","feFuncR","feGaussianBlur","feImage","feMerge","feMergeNode","feMorphology","feOffset","fePointLight","feSpecularLighting","feSpotLight","feTile","feTurbulence"]),Xf=Ut(["animate","color-profile","cursor","discard","font-face","font-face-format","font-face-name","font-face-src","font-face-uri","foreignobject","hatch","hatchpath","mesh","meshgradient","meshpatch","meshrow","missing-glyph","script","set","solidcolor","unknown","use"]),lr=Ut(["math","menclose","merror","mfenced","mfrac","mglyph","mi","mlabeledtr","mmultiscripts","mn","mo","mover","mpadded","mphantom","mroot","mrow","ms","mspace","msqrt","mstyle","msub","msup","msubsup","mtable","mtd","mtext","mtr","munder","munderover","mprescripts"]),Zf=Ut(["maction","maligngroup","malignmark","mlongdiv","mscarries","mscarry","msgroup","mstack","msline","msrow","semantics","annotation","annotation-xml","mprescripts","none"]),xn=Ut(["#text"]),_n=Ut(["accept","action","align","alt","autocapitalize","autocomplete","autopictureinpicture","autoplay","background","bgcolor","border","capture","cellpadding","cellspacing","checked","cite","class","clear","color","cols","colspan","controls","controlslist","coords","crossorigin","datetime","decoding","default","dir","disabled","disablepictureinpicture","disableremoteplayback","download","draggable","enctype","enterkeyhint","face","for","headers","height","hidden","high","href","hreflang","id","inputmode","integrity","ismap","kind","label","lang","list","loading","loop","low","max","maxlength","media","method","min","minlength","multiple","muted","name","nonce","noshade","novalidate","nowrap","open","optimum","pattern","placeholder","playsinline","popover","popovertarget","popovertargetaction","poster","preload","pubdate","radiogroup","readonly","rel","required","rev","reversed","role","rows","rowspan","spellcheck","scope","selected","shape","size","sizes","span","srclang","start","src","srcset","step","style","summary","tabindex","title","translate","type","usemap","valign","value","width","wrap","xmlns","slot"]),cr=Ut(["accent-height","accumulate","additive","alignment-baseline","amplitude","ascent","attributename","attributetype","azimuth","basefrequency","baseline-shift","begin","bias","by","class","clip","clippathunits","clip-path","clip-rule","color","color-interpolation","color-interpolation-filters","color-profile","color-rendering","cx","cy","d","dx","dy","diffuseconstant","direction","display","divisor","dur","edgemode","elevation","end","exponent","fill","fill-opacity","fill-rule","filter","filterunits","flood-color","flood-opacity","font-family","font-size","font-size-adjust","font-stretch","font-style","font-variant","font-weight","fx","fy","g1","g2","glyph-name","glyphref","gradientunits","gradienttransform","height","href","id","image-rendering","in","in2","intercept","k","k1","k2","k3","k4","kerning","keypoints","keysplines","keytimes","lang","lengthadjust","letter-spacing","kernelmatrix","kernelunitlength","lighting-color","local","marker-end","marker-mid","marker-start","markerheight","markerunits","markerwidth","maskcontentunits","maskunits","max","mask","media","method","mode","min","name","numoctaves","offset","operator","opacity","order","orient","orientation","origin","overflow","paint-order","path","pathlength","patterncontentunits","patterntransform","patternunits","points","preservealpha","preserveaspectratio","primitiveunits","r","rx","ry","radius","refx","refy","repeatcount","repeatdur","restart","result","rotate","scale","seed","shape-rendering","slope","specularconstant","specularexponent","spreadmethod","startoffset","stddeviation","stitchtiles","stop-color","stop-opacity","stroke-dasharray","stroke-dashoffset","stroke-linecap","stroke-linejoin","stroke-miterlimit","stroke-opacity","stroke","stroke-width","style","surfacescale","systemlanguage","tabindex","tablevalues","targetx","targety","transform","transform-origin","text-anchor","text-decoration","text-rendering","textlength","type","u1","u2","unicode","values","viewbox","visibility","version","vert-adv-y","vert-origin-x","vert-origin-y","width","word-spacing","wrap","writing-mode","xchannelselector","ychannelselector","x","x1","x2","xmlns","y","y1","y2","z","zoomandpan"]),kn=Ut(["accent","accentunder","align","bevelled","close","columnsalign","columnlines","columnspan","denomalign","depth","dir","display","displaystyle","encoding","fence","frame","height","href","id","largeop","length","linethickness","lspace","lquote","mathbackground","mathcolor","mathsize","mathvariant","maxsize","minsize","movablelimits","notation","numalign","open","rowalign","rowlines","rowspacing","rowspan","rspace","rquote","scriptlevel","scriptminsize","scriptsizemultiplier","selection","separator","separators","stretchy","subscriptshift","supscriptshift","symmetric","voffset","width","xmlns"]),Qi=Ut(["xlink:href","xml:id","xlink:title","xml:space","xmlns:xlink"]),Qf=le(/\{\{[\w\W]*|[\w\W]*\}\}/gm),Jf=le(/<%[\w\W]*|[\w\W]*%>/gm),tm=le(/\${[\w\W]*}/gm),em=le(/^data-[\-\w.\u00B7-\uFFFF]/),sm=le(/^aria-[\-\w]+$/),kl=le(/^(?:(?:(?:f|ht)tps?|mailto|tel|callto|sms|cid|xmpp):|[^a-z]|[a-z+.\-]+(?:[^a-z+.\-:]|$))/i),im=le(/^(?:\w+script|data):/i),om=le(/[\u0000-\u0020\u00A0\u1680\u180E\u2000-\u2029\u205F\u3000]/g),$l=le(/^html$/i),rm=le(/^[a-z][.\w]*(-[.\w]+)+$/i);var $n=Object.freeze({__proto__:null,ARIA_ATTR:sm,ATTR_WHITESPACE:om,CUSTOM_ELEMENT:rm,DATA_ATTR:em,DOCTYPE_NAME:$l,ERB_EXPR:Jf,IS_ALLOWED_URI:kl,IS_SCRIPT_OR_DATA:im,MUSTACHE_EXPR:Qf,TMPLIT_EXPR:tm});const ii={element:1,attribute:2,text:3,cdataSection:4,entityReference:5,entityNode:6,progressingInstruction:7,comment:8,document:9,documentType:10,documentFragment:11,notation:12},am=function(){return typeof window>"u"?null:window},nm=function(t,s){if(typeof t!="object"||typeof t.createPolicy!="function")return null;let i=null;const o="data-tt-policy-suffix";s&&s.hasAttribute(o)&&(i=s.getAttribute(o));const r="dompurify"+(i?"#"+i:"");try{return t.createPolicy(r,{createHTML(a){return a},createScriptURL(a){return a}})}catch{return console.warn("TrustedTypes policy "+r+" could not be created."),null}},Cn=function(){return{afterSanitizeAttributes:[],afterSanitizeElements:[],afterSanitizeShadowDOM:[],beforeSanitizeAttributes:[],beforeSanitizeElements:[],beforeSanitizeShadowDOM:[],uponSanitizeAttribute:[],uponSanitizeElement:[],uponSanitizeShadowNode:[]}};function Cl(){let e=arguments.length>0&&arguments[0]!==void 0?arguments[0]:am();const t=N=>Cl(N);if(t.version="3.2.2",t.removed=[],!e||!e.document||e.document.nodeType!==ii.document)return t.isSupported=!1,t;let{document:s}=e;const i=s,o=i.currentScript,{DocumentFragment:r,HTMLTemplateElement:a,Node:l,Element:c,NodeFilter:h,NamedNodeMap:u=e.NamedNodeMap||e.MozNamedAttrMap,HTMLFormElement:p,DOMParser:f,trustedTypes:m}=e,b=c.prototype,y=si(b,"cloneNode"),k=si(b,"remove"),z=si(b,"nextSibling"),_=si(b,"childNodes"),S=si(b,"parentNode");if(typeof a=="function"){const N=s.createElement("template");N.content&&N.content.ownerDocument&&(s=N.content.ownerDocument)}let w,x="";const{implementation:D,createNodeIterator:B,createDocumentFragment:U,getElementsByTagName:F}=s,{importNode:R}=i;let q=Cn();t.isSupported=typeof xl=="function"&&typeof S=="function"&&D&&D.createHTMLDocument!==void 0;const{MUSTACHE_EXPR:ot,ERB_EXPR:mt,TMPLIT_EXPR:rt,DATA_ATTR:qt,ARIA_ATTR:ie,IS_SCRIPT_OR_DATA:oe,ATTR_WHITESPACE:de,CUSTOM_ELEMENT:we}=$n;let{IS_ALLOWED_URI:Li}=$n,xt=null;const aa=V({},[...wn,...ar,...nr,...lr,...xn]);let St=null;const na=V({},[..._n,...cr,...kn,...Qi]);let ft=Object.seal(_l(null,{tagNameCheck:{writable:!0,configurable:!1,enumerable:!0,value:null},attributeNameCheck:{writable:!0,configurable:!1,enumerable:!0,value:null},allowCustomizedBuiltInElements:{writable:!0,configurable:!1,enumerable:!0,value:!1}})),Hs=null,Ro=null,la=!0,Lo=!0,ca=!1,da=!0,vs=!1,Mo=!0,Ze=!1,No=!1,Fo=!1,ys=!1,Mi=!1,Ni=!1,ha=!0,ua=!1;const Hl="user-content-";let Bo=!0,Vs=!1,ws={},xs=null;const pa=V({},["annotation-xml","audio","colgroup","desc","foreignobject","head","iframe","math","mi","mn","mo","ms","mtext","noembed","noframes","noscript","plaintext","script","style","svg","template","thead","title","video","xmp"]);let fa=null;const ma=V({},["audio","video","img","source","image","track"]);let Uo=null;const ga=V({},["alt","class","for","id","label","name","pattern","placeholder","role","summary","title","value","style","xmlns"]),Fi="http://www.w3.org/1998/Math/MathML",Bi="http://www.w3.org/2000/svg",Ie="http://www.w3.org/1999/xhtml";let _s=Ie,Ho=!1,Vo=null;const Vl=V({},[Fi,Bi,Ie],rr);let Ui=V({},["mi","mo","mn","ms","mtext"]),Hi=V({},["annotation-xml"]);const jl=V({},["title","style","font","a","script"]);let js=null;const Wl=["application/xhtml+xml","text/html"],ql="text/html";let _t=null,ks=null;const Kl=s.createElement("form"),ba=function(g){return g instanceof RegExp||g instanceof Function},jo=function(){let g=arguments.length>0&&arguments[0]!==void 0?arguments[0]:{};if(!(ks&&ks===g)){if((!g||typeof g!="object")&&(g={}),g=Je(g),js=Wl.indexOf(g.PARSER_MEDIA_TYPE)===-1?ql:g.PARSER_MEDIA_TYPE,_t=js==="application/xhtml+xml"?rr:io,xt=he(g,"ALLOWED_TAGS")?V({},g.ALLOWED_TAGS,_t):aa,St=he(g,"ALLOWED_ATTR")?V({},g.ALLOWED_ATTR,_t):na,Vo=he(g,"ALLOWED_NAMESPACES")?V({},g.ALLOWED_NAMESPACES,rr):Vl,Uo=he(g,"ADD_URI_SAFE_ATTR")?V(Je(ga),g.ADD_URI_SAFE_ATTR,_t):ga,fa=he(g,"ADD_DATA_URI_TAGS")?V(Je(ma),g.ADD_DATA_URI_TAGS,_t):ma,xs=he(g,"FORBID_CONTENTS")?V({},g.FORBID_CONTENTS,_t):pa,Hs=he(g,"FORBID_TAGS")?V({},g.FORBID_TAGS,_t):{},Ro=he(g,"FORBID_ATTR")?V({},g.FORBID_ATTR,_t):{},ws=he(g,"USE_PROFILES")?g.USE_PROFILES:!1,la=g.ALLOW_ARIA_ATTR!==!1,Lo=g.ALLOW_DATA_ATTR!==!1,ca=g.ALLOW_UNKNOWN_PROTOCOLS||!1,da=g.ALLOW_SELF_CLOSE_IN_ATTR!==!1,vs=g.SAFE_FOR_TEMPLATES||!1,Mo=g.SAFE_FOR_XML!==!1,Ze=g.WHOLE_DOCUMENT||!1,ys=g.RETURN_DOM||!1,Mi=g.RETURN_DOM_FRAGMENT||!1,Ni=g.RETURN_TRUSTED_TYPE||!1,Fo=g.FORCE_BODY||!1,ha=g.SANITIZE_DOM!==!1,ua=g.SANITIZE_NAMED_PROPS||!1,Bo=g.KEEP_CONTENT!==!1,Vs=g.IN_PLACE||!1,Li=g.ALLOWED_URI_REGEXP||kl,_s=g.NAMESPACE||Ie,Ui=g.MATHML_TEXT_INTEGRATION_POINTS||Ui,Hi=g.HTML_INTEGRATION_POINTS||Hi,ft=g.CUSTOM_ELEMENT_HANDLING||{},g.CUSTOM_ELEMENT_HANDLING&&ba(g.CUSTOM_ELEMENT_HANDLING.tagNameCheck)&&(ft.tagNameCheck=g.CUSTOM_ELEMENT_HANDLING.tagNameCheck),g.CUSTOM_ELEMENT_HANDLING&&ba(g.CUSTOM_ELEMENT_HANDLING.attributeNameCheck)&&(ft.attributeNameCheck=g.CUSTOM_ELEMENT_HANDLING.attributeNameCheck),g.CUSTOM_ELEMENT_HANDLING&&typeof g.CUSTOM_ELEMENT_HANDLING.allowCustomizedBuiltInElements=="boolean"&&(ft.allowCustomizedBuiltInElements=g.CUSTOM_ELEMENT_HANDLING.allowCustomizedBuiltInElements),vs&&(Lo=!1),Mi&&(ys=!0),ws&&(xt=V({},xn),St=[],ws.html===!0&&(V(xt,wn),V(St,_n)),ws.svg===!0&&(V(xt,ar),V(St,cr),V(St,Qi)),ws.svgFilters===!0&&(V(xt,nr),V(St,cr),V(St,Qi)),ws.mathMl===!0&&(V(xt,lr),V(St,kn),V(St,Qi))),g.ADD_TAGS&&(xt===aa&&(xt=Je(xt)),V(xt,g.ADD_TAGS,_t)),g.ADD_ATTR&&(St===na&&(St=Je(St)),V(St,g.ADD_ATTR,_t)),g.ADD_URI_SAFE_ATTR&&V(Uo,g.ADD_URI_SAFE_ATTR,_t),g.FORBID_CONTENTS&&(xs===pa&&(xs=Je(xs)),V(xs,g.FORBID_CONTENTS,_t)),Bo&&(xt["#text"]=!0),Ze&&V(xt,["html","head","body"]),xt.table&&(V(xt,["tbody"]),delete Hs.tbody),g.TRUSTED_TYPES_POLICY){if(typeof g.TRUSTED_TYPES_POLICY.createHTML!="function")throw ei('TRUSTED_TYPES_POLICY configuration option must provide a "createHTML" hook.');if(typeof g.TRUSTED_TYPES_POLICY.createScriptURL!="function")throw ei('TRUSTED_TYPES_POLICY configuration option must provide a "createScriptURL" hook.');w=g.TRUSTED_TYPES_POLICY,x=w.createHTML("")}else w===void 0&&(w=nm(m,o)),w!==null&&typeof x=="string"&&(x=w.createHTML(""));Ut&&Ut(g),ks=g}},va=V({},[...ar,...nr,...Xf]),ya=V({},[...lr,...Zf]),Yl=function(g){let $=S(g);(!$||!$.tagName)&&($={namespaceURI:_s,tagName:"template"});const I=io(g.tagName),at=io($.tagName);return Vo[g.namespaceURI]?g.namespaceURI===Bi?$.namespaceURI===Ie?I==="svg":$.namespaceURI===Fi?I==="svg"&&(at==="annotation-xml"||Ui[at]):!!va[I]:g.namespaceURI===Fi?$.namespaceURI===Ie?I==="math":$.namespaceURI===Bi?I==="math"&&Hi[at]:!!ya[I]:g.namespaceURI===Ie?$.namespaceURI===Bi&&!Hi[at]||$.namespaceURI===Fi&&!Ui[at]?!1:!ya[I]&&(jl[I]||!va[I]):!!(js==="application/xhtml+xml"&&Vo[g.namespaceURI]):!1},xe=function(g){Js(t.removed,{element:g});try{S(g).removeChild(g)}catch{k(g)}},Vi=function(g,$){try{Js(t.removed,{attribute:$.getAttributeNode(g),from:$})}catch{Js(t.removed,{attribute:null,from:$})}if($.removeAttribute(g),g==="is")if(ys||Mi)try{xe($)}catch{}else try{$.setAttribute(g,"")}catch{}},wa=function(g){let $=null,I=null;if(Fo)g="<remove></remove>"+g;else{const At=yn(g,/^[\r\n\t ]+/);I=At&&At[0]}js==="application/xhtml+xml"&&_s===Ie&&(g='<html xmlns="http://www.w3.org/1999/xhtml"><head></head><body>'+g+"</body></html>");const at=w?w.createHTML(g):g;if(_s===Ie)try{$=new f().parseFromString(at,js)}catch{}if(!$||!$.documentElement){$=D.createDocument(_s,"template",null);try{$.documentElement.innerHTML=Ho?x:at}catch{}}const Pt=$.body||$.documentElement;return g&&I&&Pt.insertBefore(s.createTextNode(I),Pt.childNodes[0]||null),_s===Ie?F.call($,Ze?"html":"body")[0]:Ze?$.documentElement:Pt},xa=function(g){return B.call(g.ownerDocument||g,g,h.SHOW_ELEMENT|h.SHOW_COMMENT|h.SHOW_TEXT|h.SHOW_PROCESSING_INSTRUCTION|h.SHOW_CDATA_SECTION,null)},_a=function(g){return g instanceof p&&(typeof g.nodeName!="string"||typeof g.textContent!="string"||typeof g.removeChild!="function"||!(g.attributes instanceof u)||typeof g.removeAttribute!="function"||typeof g.setAttribute!="function"||typeof g.namespaceURI!="string"||typeof g.insertBefore!="function"||typeof g.hasChildNodes!="function")},ka=function(g){return typeof l=="function"&&g instanceof l};function Re(N,g,$){Zi(N,I=>{I.call(t,g,$,ks)})}const $a=function(g){let $=null;if(Re(q.beforeSanitizeElements,g,null),_a(g))return xe(g),!0;const I=_t(g.nodeName);if(Re(q.uponSanitizeElement,g,{tagName:I,allowedTags:xt}),g.hasChildNodes()&&!ka(g.firstElementChild)&&Mt(/<[/\w]/g,g.innerHTML)&&Mt(/<[/\w]/g,g.textContent)||g.nodeType===ii.progressingInstruction||Mo&&g.nodeType===ii.comment&&Mt(/<[/\w]/g,g.data))return xe(g),!0;if(!xt[I]||Hs[I]){if(!Hs[I]&&Sa(I)&&(ft.tagNameCheck instanceof RegExp&&Mt(ft.tagNameCheck,I)||ft.tagNameCheck instanceof Function&&ft.tagNameCheck(I)))return!1;if(Bo&&!xs[I]){const at=S(g)||g.parentNode,Pt=_(g)||g.childNodes;if(Pt&&at){const At=Pt.length;for(let Vt=At-1;Vt>=0;--Vt){const _e=y(Pt[Vt],!0);_e.__removalCount=(g.__removalCount||0)+1,at.insertBefore(_e,z(g))}}}return xe(g),!0}return g instanceof c&&!Yl(g)||(I==="noscript"||I==="noembed"||I==="noframes")&&Mt(/<\/no(script|embed|frames)/i,g.innerHTML)?(xe(g),!0):(vs&&g.nodeType===ii.text&&($=g.textContent,Zi([ot,mt,rt],at=>{$=ti($,at," ")}),g.textContent!==$&&(Js(t.removed,{element:g.cloneNode()}),g.textContent=$)),Re(q.afterSanitizeElements,g,null),!1)},Ca=function(g,$,I){if(ha&&($==="id"||$==="name")&&(I in s||I in Kl))return!1;if(!(Lo&&!Ro[$]&&Mt(qt,$))){if(!(la&&Mt(ie,$))){if(!St[$]||Ro[$]){if(!(Sa(g)&&(ft.tagNameCheck instanceof RegExp&&Mt(ft.tagNameCheck,g)||ft.tagNameCheck instanceof Function&&ft.tagNameCheck(g))&&(ft.attributeNameCheck instanceof RegExp&&Mt(ft.attributeNameCheck,$)||ft.attributeNameCheck instanceof Function&&ft.attributeNameCheck($))||$==="is"&&ft.allowCustomizedBuiltInElements&&(ft.tagNameCheck instanceof RegExp&&Mt(ft.tagNameCheck,I)||ft.tagNameCheck instanceof Function&&ft.tagNameCheck(I))))return!1}else if(!Uo[$]){if(!Mt(Li,ti(I,de,""))){if(!(($==="src"||$==="xlink:href"||$==="href")&&g!=="script"&&qf(I,"data:")===0&&fa[g])){if(!(ca&&!Mt(oe,ti(I,de,"")))){if(I)return!1}}}}}}return!0},Sa=function(g){return g!=="annotation-xml"&&yn(g,we)},Aa=function(g){Re(q.beforeSanitizeAttributes,g,null);const{attributes:$}=g;if(!$)return;const I={attrName:"",attrValue:"",keepAttr:!0,allowedAttributes:St,forceKeepAttr:void 0};let at=$.length;for(;at--;){const Pt=$[at],{name:At,namespaceURI:Vt,value:_e}=Pt,Ws=_t(At);let Lt=At==="value"?_e:Kf(_e);if(I.attrName=Ws,I.attrValue=Lt,I.keepAttr=!0,I.forceKeepAttr=void 0,Re(q.uponSanitizeAttribute,g,I),Lt=I.attrValue,ua&&(Ws==="id"||Ws==="name")&&(Vi(At,g),Lt=Hl+Lt),Mo&&Mt(/((--!?|])>)|<\/(style|title)/i,Lt)){Vi(At,g);continue}if(I.forceKeepAttr||(Vi(At,g),!I.keepAttr))continue;if(!da&&Mt(/\/>/i,Lt)){Vi(At,g);continue}vs&&Zi([ot,mt,rt],za=>{Lt=ti(Lt,za," ")});const Ta=_t(g.nodeName);if(Ca(Ta,Ws,Lt)){if(w&&typeof m=="object"&&typeof m.getAttributeType=="function"&&!Vt)switch(m.getAttributeType(Ta,Ws)){case"TrustedHTML":{Lt=w.createHTML(Lt);break}case"TrustedScriptURL":{Lt=w.createScriptURL(Lt);break}}try{Vt?g.setAttributeNS(Vt,At,Lt):g.setAttribute(At,Lt),_a(g)?xe(g):vn(t.removed)}catch{}}}Re(q.afterSanitizeAttributes,g,null)},Gl=function N(g){let $=null;const I=xa(g);for(Re(q.beforeSanitizeShadowDOM,g,null);$=I.nextNode();)Re(q.uponSanitizeShadowNode,$,null),!$a($)&&($.content instanceof r&&N($.content),Aa($));Re(q.afterSanitizeShadowDOM,g,null)};return t.sanitize=function(N){let g=arguments.length>1&&arguments[1]!==void 0?arguments[1]:{},$=null,I=null,at=null,Pt=null;if(Ho=!N,Ho&&(N="<!-->"),typeof N!="string"&&!ka(N))if(typeof N.toString=="function"){if(N=N.toString(),typeof N!="string")throw ei("dirty is not a string, aborting")}else throw ei("toString is not a function");if(!t.isSupported)return N;if(No||jo(g),t.removed=[],typeof N=="string"&&(Vs=!1),Vs){if(N.nodeName){const _e=_t(N.nodeName);if(!xt[_e]||Hs[_e])throw ei("root node is forbidden and cannot be sanitized in-place")}}else if(N instanceof l)$=wa("<!---->"),I=$.ownerDocument.importNode(N,!0),I.nodeType===ii.element&&I.nodeName==="BODY"||I.nodeName==="HTML"?$=I:$.appendChild(I);else{if(!ys&&!vs&&!Ze&&N.indexOf("<")===-1)return w&&Ni?w.createHTML(N):N;if($=wa(N),!$)return ys?null:Ni?x:""}$&&Fo&&xe($.firstChild);const At=xa(Vs?N:$);for(;at=At.nextNode();)$a(at)||(at.content instanceof r&&Gl(at.content),Aa(at));if(Vs)return N;if(ys){if(Mi)for(Pt=U.call($.ownerDocument);$.firstChild;)Pt.appendChild($.firstChild);else Pt=$;return(St.shadowroot||St.shadowrootmode)&&(Pt=R.call(i,Pt,!0)),Pt}let Vt=Ze?$.outerHTML:$.innerHTML;return Ze&&xt["!doctype"]&&$.ownerDocument&&$.ownerDocument.doctype&&$.ownerDocument.doctype.name&&Mt($l,$.ownerDocument.doctype.name)&&(Vt="<!DOCTYPE "+$.ownerDocument.doctype.name+`>
`+Vt),vs&&Zi([ot,mt,rt],_e=>{Vt=ti(Vt,_e," ")}),w&&Ni?w.createHTML(Vt):Vt},t.setConfig=function(){let N=arguments.length>0&&arguments[0]!==void 0?arguments[0]:{};jo(N),No=!0},t.clearConfig=function(){ks=null,No=!1},t.isValidAttribute=function(N,g,$){ks||jo({});const I=_t(N),at=_t(g);return Ca(I,at,$)},t.addHook=function(N,g){typeof g=="function"&&Js(q[N],g)},t.removeHook=function(N){return vn(q[N])},t.removeHooks=function(N){q[N]=[]},t.removeAllHooks=function(){q=Cn()},t}var Sl=Cl();function Yr(){return{async:!1,breaks:!1,extensions:null,gfm:!0,hooks:null,pedantic:!1,renderer:null,silent:!1,tokenizer:null,walkTokens:null}}let ms=Yr();function Al(e){ms=e}const mi={exec:()=>null};function et(e,t=""){let s=typeof e=="string"?e:e.source;const i={replace:(o,r)=>{let a=typeof r=="string"?r:r.source;return a=a.replace(Ft.caret,"$1"),s=s.replace(o,a),i},getRegex:()=>new RegExp(s,t)};return i}const Ft={codeRemoveIndent:/^(?: {1,4}| {0,3}\t)/gm,outputLinkReplace:/\\([\[\]])/g,indentCodeCompensation:/^(\s+)(?:```)/,beginningSpace:/^\s+/,endingHash:/#$/,startingSpaceChar:/^ /,endingSpaceChar:/ $/,nonSpaceChar:/[^ ]/,newLineCharGlobal:/\n/g,tabCharGlobal:/\t/g,multipleSpaceGlobal:/\s+/g,blankLine:/^[ \t]*$/,doubleBlankLine:/\n[ \t]*\n[ \t]*$/,blockquoteStart:/^ {0,3}>/,blockquoteSetextReplace:/\n {0,3}((?:=+|-+) *)(?=\n|$)/g,blockquoteSetextReplace2:/^ {0,3}>[ \t]?/gm,listReplaceTabs:/^\t+/,listReplaceNesting:/^ {1,4}(?=( {4})*[^ ])/g,listIsTask:/^\[[ xX]\] /,listReplaceTask:/^\[[ xX]\] +/,anyLine:/\n.*\n/,hrefBrackets:/^<(.*)>$/,tableDelimiter:/[:|]/,tableAlignChars:/^\||\| *$/g,tableRowBlankLine:/\n[ \t]*$/,tableAlignRight:/^ *-+: *$/,tableAlignCenter:/^ *:-+: *$/,tableAlignLeft:/^ *:-+ *$/,startATag:/^<a /i,endATag:/^<\/a>/i,startPreScriptTag:/^<(pre|code|kbd|script)(\s|>)/i,endPreScriptTag:/^<\/(pre|code|kbd|script)(\s|>)/i,startAngleBracket:/^</,endAngleBracket:/>$/,pedanticHrefTitle:/^([^'"]*[^\s])\s+(['"])(.*)\2/,unicodeAlphaNumeric:/[\p{L}\p{N}]/u,escapeTest:/[&<>"']/,escapeReplace:/[&<>"']/g,escapeTestNoEncode:/[<>"']|&(?!(#\d{1,7}|#[Xx][a-fA-F0-9]{1,6}|\w+);)/,escapeReplaceNoEncode:/[<>"']|&(?!(#\d{1,7}|#[Xx][a-fA-F0-9]{1,6}|\w+);)/g,unescapeTest:/&(#(?:\d+)|(?:#x[0-9A-Fa-f]+)|(?:\w+));?/ig,caret:/(^|[^\[])\^/g,percentDecode:/%25/g,findPipe:/\|/g,splitPipe:/ \|/,slashPipe:/\\\|/g,carriageReturn:/\r\n|\r/g,spaceLine:/^ +$/gm,notSpaceStart:/^\S*/,endingNewline:/\n$/,listItemRegex:e=>new RegExp(`^( {0,3}${e})((?:[	 ][^\\n]*)?(?:\\n|$))`),nextBulletRegex:e=>new RegExp(`^ {0,${Math.min(3,e-1)}}(?:[*+-]|\\d{1,9}[.)])((?:[ 	][^\\n]*)?(?:\\n|$))`),hrRegex:e=>new RegExp(`^ {0,${Math.min(3,e-1)}}((?:- *){3,}|(?:_ *){3,}|(?:\\* *){3,})(?:\\n+|$)`),fencesBeginRegex:e=>new RegExp(`^ {0,${Math.min(3,e-1)}}(?:\`\`\`|~~~)`),headingBeginRegex:e=>new RegExp(`^ {0,${Math.min(3,e-1)}}#`),htmlBeginRegex:e=>new RegExp(`^ {0,${Math.min(3,e-1)}}<(?:[a-z].*>|!--)`,"i")},lm=/^(?:[ \t]*(?:\n|$))+/,cm=/^((?: {4}| {0,3}\t)[^\n]+(?:\n(?:[ \t]*(?:\n|$))*)?)+/,dm=/^ {0,3}(`{3,}(?=[^`\n]*(?:\n|$))|~{3,})([^\n]*)(?:\n|$)(?:|([\s\S]*?)(?:\n|$))(?: {0,3}\1[~`]* *(?=\n|$)|$)/,Ii=/^ {0,3}((?:-[\t ]*){3,}|(?:_[ \t]*){3,}|(?:\*[ \t]*){3,})(?:\n+|$)/,hm=/^ {0,3}(#{1,6})(?=\s|$)(.*)(?:\n+|$)/,Tl=/(?:[*+-]|\d{1,9}[.)])/,zl=et(/^(?!bull |blockCode|fences|blockquote|heading|html)((?:.|\n(?!\s*?\n|bull |blockCode|fences|blockquote|heading|html))+?)\n {0,3}(=+|-+) *(?:\n+|$)/).replace(/bull/g,Tl).replace(/blockCode/g,/(?: {4}| {0,3}\t)/).replace(/fences/g,/ {0,3}(?:`{3,}|~{3,})/).replace(/blockquote/g,/ {0,3}>/).replace(/heading/g,/ {0,3}#{1,6}/).replace(/html/g,/ {0,3}<[^\n>]+>\n/).getRegex(),Gr=/^([^\n]+(?:\n(?!hr|heading|lheading|blockquote|fences|list|html|table| +\n)[^\n]+)*)/,um=/^[^\n]+/,Xr=/(?!\s*\])(?:\\.|[^\[\]\\])+/,pm=et(/^ {0,3}\[(label)\]: *(?:\n[ \t]*)?([^<\s][^\s]*|<.*?>)(?:(?: +(?:\n[ \t]*)?| *\n[ \t]*)(title))? *(?:\n+|$)/).replace("label",Xr).replace("title",/(?:"(?:\\"?|[^"\\])*"|'[^'\n]*(?:\n[^'\n]+)*\n?'|\([^()]*\))/).getRegex(),fm=et(/^( {0,3}bull)([ \t][^\n]+?)?(?:\n|$)/).replace(/bull/g,Tl).getRegex(),Do="address|article|aside|base|basefont|blockquote|body|caption|center|col|colgroup|dd|details|dialog|dir|div|dl|dt|fieldset|figcaption|figure|footer|form|frame|frameset|h[1-6]|head|header|hr|html|iframe|legend|li|link|main|menu|menuitem|meta|nav|noframes|ol|optgroup|option|p|param|search|section|summary|table|tbody|td|tfoot|th|thead|title|tr|track|ul",Zr=/<!--(?:-?>|[\s\S]*?(?:-->|$))/,mm=et("^ {0,3}(?:<(script|pre|style|textarea)[\\s>][\\s\\S]*?(?:</\\1>[^\\n]*\\n+|$)|comment[^\\n]*(\\n+|$)|<\\?[\\s\\S]*?(?:\\?>\\n*|$)|<![A-Z][\\s\\S]*?(?:>\\n*|$)|<!\\[CDATA\\[[\\s\\S]*?(?:\\]\\]>\\n*|$)|</?(tag)(?: +|\\n|/?>)[\\s\\S]*?(?:(?:\\n[ 	]*)+\\n|$)|<(?!script|pre|style|textarea)([a-z][\\w-]*)(?:attribute)*? */?>(?=[ \\t]*(?:\\n|$))[\\s\\S]*?(?:(?:\\n[ 	]*)+\\n|$)|</(?!script|pre|style|textarea)[a-z][\\w-]*\\s*>(?=[ \\t]*(?:\\n|$))[\\s\\S]*?(?:(?:\\n[ 	]*)+\\n|$))","i").replace("comment",Zr).replace("tag",Do).replace("attribute",/ +[a-zA-Z:_][\w.:-]*(?: *= *"[^"\n]*"| *= *'[^'\n]*'| *= *[^\s"'=<>`]+)?/).getRegex(),El=et(Gr).replace("hr",Ii).replace("heading"," {0,3}#{1,6}(?:\\s|$)").replace("|lheading","").replace("|table","").replace("blockquote"," {0,3}>").replace("fences"," {0,3}(?:`{3,}(?=[^`\\n]*\\n)|~{3,})[^\\n]*\\n").replace("list"," {0,3}(?:[*+-]|1[.)]) ").replace("html","</?(?:tag)(?: +|\\n|/?>)|<(?:script|pre|style|textarea|!--)").replace("tag",Do).getRegex(),gm=et(/^( {0,3}> ?(paragraph|[^\n]*)(?:\n|$))+/).replace("paragraph",El).getRegex(),Qr={blockquote:gm,code:cm,def:pm,fences:dm,heading:hm,hr:Ii,html:mm,lheading:zl,list:fm,newline:lm,paragraph:El,table:mi,text:um},Sn=et("^ *([^\\n ].*)\\n {0,3}((?:\\| *)?:?-+:? *(?:\\| *:?-+:? *)*(?:\\| *)?)(?:\\n((?:(?! *\\n|hr|heading|blockquote|code|fences|list|html).*(?:\\n|$))*)\\n*|$)").replace("hr",Ii).replace("heading"," {0,3}#{1,6}(?:\\s|$)").replace("blockquote"," {0,3}>").replace("code","(?: {4}| {0,3}	)[^\\n]").replace("fences"," {0,3}(?:`{3,}(?=[^`\\n]*\\n)|~{3,})[^\\n]*\\n").replace("list"," {0,3}(?:[*+-]|1[.)]) ").replace("html","</?(?:tag)(?: +|\\n|/?>)|<(?:script|pre|style|textarea|!--)").replace("tag",Do).getRegex(),bm={...Qr,table:Sn,paragraph:et(Gr).replace("hr",Ii).replace("heading"," {0,3}#{1,6}(?:\\s|$)").replace("|lheading","").replace("table",Sn).replace("blockquote"," {0,3}>").replace("fences"," {0,3}(?:`{3,}(?=[^`\\n]*\\n)|~{3,})[^\\n]*\\n").replace("list"," {0,3}(?:[*+-]|1[.)]) ").replace("html","</?(?:tag)(?: +|\\n|/?>)|<(?:script|pre|style|textarea|!--)").replace("tag",Do).getRegex()},vm={...Qr,html:et(`^ *(?:comment *(?:\\n|\\s*$)|<(tag)[\\s\\S]+?</\\1> *(?:\\n{2,}|\\s*$)|<tag(?:"[^"]*"|'[^']*'|\\s[^'"/>\\s]*)*?/?> *(?:\\n{2,}|\\s*$))`).replace("comment",Zr).replace(/tag/g,"(?!(?:a|em|strong|small|s|cite|q|dfn|abbr|data|time|code|var|samp|kbd|sub|sup|i|b|u|mark|ruby|rt|rp|bdi|bdo|span|br|wbr|ins|del|img)\\b)\\w+(?!:|[^\\w\\s@]*@)\\b").getRegex(),def:/^ *\[([^\]]+)\]: *<?([^\s>]+)>?(?: +(["(][^\n]+[")]))? *(?:\n+|$)/,heading:/^(#{1,6})(.*)(?:\n+|$)/,fences:mi,lheading:/^(.+?)\n {0,3}(=+|-+) *(?:\n+|$)/,paragraph:et(Gr).replace("hr",Ii).replace("heading",` *#{1,6} *[^
]`).replace("lheading",zl).replace("|table","").replace("blockquote"," {0,3}>").replace("|fences","").replace("|list","").replace("|html","").replace("|tag","").getRegex()},Ol=/^\\([!"#$%&'()*+,\-./:;<=>?@\[\]\\^_`{|}~])/,ym=/^(`+)([^`]|[^`][\s\S]*?[^`])\1(?!`)/,Pl=/^( {2,}|\\)\n(?!\s*$)/,wm=/^(`+|[^`])(?:(?= {2,}\n)|[\s\S]*?(?:(?=[\\<!\[`*_]|\b_|$)|[^ ](?= {2,}\n)))/,Io=/[\p{P}\p{S}]/u,Jr=/[\s\p{P}\p{S}]/u,Dl=/[^\s\p{P}\p{S}]/u,xm=et(/^((?![*_])punctSpace)/,"u").replace(/punctSpace/g,Jr).getRegex(),_m=/\[[^[\]]*?\]\((?:\\.|[^\\\(\)]|\((?:\\.|[^\\\(\)])*\))*\)|`[^`]*?`|<[^<>]*?>/g,km=et(/^(?:\*+(?:((?!\*)punct)|[^\s*]))|^_+(?:((?!_)punct)|([^\s_]))/,"u").replace(/punct/g,Io).getRegex(),$m=et("^[^_*]*?__[^_*]*?\\*[^_*]*?(?=__)|[^*]+(?=[^*])|(?!\\*)punct(\\*+)(?=[\\s]|$)|notPunctSpace(\\*+)(?!\\*)(?=punctSpace|$)|(?!\\*)punctSpace(\\*+)(?=notPunctSpace)|[\\s](\\*+)(?!\\*)(?=punct)|(?!\\*)punct(\\*+)(?!\\*)(?=punct)|notPunctSpace(\\*+)(?=notPunctSpace)","gu").replace(/notPunctSpace/g,Dl).replace(/punctSpace/g,Jr).replace(/punct/g,Io).getRegex(),Cm=et("^[^_*]*?\\*\\*[^_*]*?_[^_*]*?(?=\\*\\*)|[^_]+(?=[^_])|(?!_)punct(_+)(?=[\\s]|$)|notPunctSpace(_+)(?!_)(?=punctSpace|$)|(?!_)punctSpace(_+)(?=notPunctSpace)|[\\s](_+)(?!_)(?=punct)|(?!_)punct(_+)(?!_)(?=punct)","gu").replace(/notPunctSpace/g,Dl).replace(/punctSpace/g,Jr).replace(/punct/g,Io).getRegex(),Sm=et(/\\(punct)/,"gu").replace(/punct/g,Io).getRegex(),Am=et(/^<(scheme:[^\s\x00-\x1f<>]*|email)>/).replace("scheme",/[a-zA-Z][a-zA-Z0-9+.-]{1,31}/).replace("email",/[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+(@)[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)+(?![-_])/).getRegex(),Tm=et(Zr).replace("(?:-->|$)","-->").getRegex(),zm=et("^comment|^</[a-zA-Z][\\w:-]*\\s*>|^<[a-zA-Z][\\w-]*(?:attribute)*?\\s*/?>|^<\\?[\\s\\S]*?\\?>|^<![a-zA-Z]+\\s[\\s\\S]*?>|^<!\\[CDATA\\[[\\s\\S]*?\\]\\]>").replace("comment",Tm).replace("attribute",/\s+[a-zA-Z:_][\w.:-]*(?:\s*=\s*"[^"]*"|\s*=\s*'[^']*'|\s*=\s*[^\s"'=<>`]+)?/).getRegex(),go=/(?:\[(?:\\.|[^\[\]\\])*\]|\\.|`[^`]*`|[^\[\]\\`])*?/,Em=et(/^!?\[(label)\]\(\s*(href)(?:\s+(title))?\s*\)/).replace("label",go).replace("href",/<(?:\\.|[^\n<>\\])+>|[^\s\x00-\x1f]*/).replace("title",/"(?:\\"?|[^"\\])*"|'(?:\\'?|[^'\\])*'|\((?:\\\)?|[^)\\])*\)/).getRegex(),Il=et(/^!?\[(label)\]\[(ref)\]/).replace("label",go).replace("ref",Xr).getRegex(),Rl=et(/^!?\[(ref)\](?:\[\])?/).replace("ref",Xr).getRegex(),Om=et("reflink|nolink(?!\\()","g").replace("reflink",Il).replace("nolink",Rl).getRegex(),ta={_backpedal:mi,anyPunctuation:Sm,autolink:Am,blockSkip:_m,br:Pl,code:ym,del:mi,emStrongLDelim:km,emStrongRDelimAst:$m,emStrongRDelimUnd:Cm,escape:Ol,link:Em,nolink:Rl,punctuation:xm,reflink:Il,reflinkSearch:Om,tag:zm,text:wm,url:mi},Pm={...ta,link:et(/^!?\[(label)\]\((.*?)\)/).replace("label",go).getRegex(),reflink:et(/^!?\[(label)\]\s*\[([^\]]*)\]/).replace("label",go).getRegex()},Sr={...ta,escape:et(Ol).replace("])","~|])").getRegex(),url:et(/^((?:ftp|https?):\/\/|www\.)(?:[a-zA-Z0-9\-]+\.?)+[^\s<]*|^email/,"i").replace("email",/[A-Za-z0-9._+-]+(@)[a-zA-Z0-9-_]+(?:\.[a-zA-Z0-9-_]*[a-zA-Z0-9])+(?![-_])/).getRegex(),_backpedal:/(?:[^?!.,:;*_'"~()&]+|\([^)]*\)|&(?![a-zA-Z0-9]+;$)|[?!.,:;*_'"~)]+(?!$))+/,del:/^(~~?)(?=[^\s~])((?:\\.|[^\\])*?(?:\\.|[^\s~\\]))\1(?=[^~]|$)/,text:/^([`~]+|[^`~])(?:(?= {2,}\n)|(?=[a-zA-Z0-9.!#$%&'*+\/=?_`{\|}~-]+@)|[\s\S]*?(?:(?=[\\<!\[`*~_]|\b_|https?:\/\/|ftp:\/\/|www\.|$)|[^ ](?= {2,}\n)|[^a-zA-Z0-9.!#$%&'*+\/=?_`{\|}~-](?=[a-zA-Z0-9.!#$%&'*+\/=?_`{\|}~-]+@)))/},Dm={...Sr,br:et(Pl).replace("{2,}","*").getRegex(),text:et(Sr.text).replace("\\b_","\\b_| {2,}\\n").replace(/\{2,\}/g,"*").getRegex()},Ji={normal:Qr,gfm:bm,pedantic:vm},oi={normal:ta,gfm:Sr,breaks:Dm,pedantic:Pm},Im={"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#39;"},An=e=>Im[e];function ke(e,t){if(t){if(Ft.escapeTest.test(e))return e.replace(Ft.escapeReplace,An)}else if(Ft.escapeTestNoEncode.test(e))return e.replace(Ft.escapeReplaceNoEncode,An);return e}function Tn(e){try{e=encodeURI(e).replace(Ft.percentDecode,"%")}catch{return null}return e}function zn(e,t){var r;const s=e.replace(Ft.findPipe,(a,l,c)=>{let h=!1,u=l;for(;--u>=0&&c[u]==="\\";)h=!h;return h?"|":" |"}),i=s.split(Ft.splitPipe);let o=0;if(i[0].trim()||i.shift(),i.length>0&&!((r=i.at(-1))!=null&&r.trim())&&i.pop(),t)if(i.length>t)i.splice(t);else for(;i.length<t;)i.push("");for(;o<i.length;o++)i[o]=i[o].trim().replace(Ft.slashPipe,"|");return i}function ri(e,t,s){const i=e.length;if(i===0)return"";let o=0;for(;o<i;){const r=e.charAt(i-o-1);if(r===t&&!s)o++;else if(r!==t&&s)o++;else break}return e.slice(0,i-o)}function Rm(e,t){if(e.indexOf(t[1])===-1)return-1;let s=0;for(let i=0;i<e.length;i++)if(e[i]==="\\")i++;else if(e[i]===t[0])s++;else if(e[i]===t[1]&&(s--,s<0))return i;return-1}function En(e,t,s,i,o){const r=t.href,a=t.title||null,l=e[1].replace(o.other.outputLinkReplace,"$1");if(e[0].charAt(0)!=="!"){i.state.inLink=!0;const c={type:"link",raw:s,href:r,title:a,text:l,tokens:i.inlineTokens(l)};return i.state.inLink=!1,c}return{type:"image",raw:s,href:r,title:a,text:l}}function Lm(e,t,s){const i=e.match(s.other.indentCodeCompensation);if(i===null)return t;const o=i[1];return t.split(`
`).map(r=>{const a=r.match(s.other.beginningSpace);if(a===null)return r;const[l]=a;return l.length>=o.length?r.slice(o.length):r}).join(`
`)}class bo{constructor(t){st(this,"options");st(this,"rules");st(this,"lexer");this.options=t||ms}space(t){const s=this.rules.block.newline.exec(t);if(s&&s[0].length>0)return{type:"space",raw:s[0]}}code(t){const s=this.rules.block.code.exec(t);if(s){const i=s[0].replace(this.rules.other.codeRemoveIndent,"");return{type:"code",raw:s[0],codeBlockStyle:"indented",text:this.options.pedantic?i:ri(i,`
`)}}}fences(t){const s=this.rules.block.fences.exec(t);if(s){const i=s[0],o=Lm(i,s[3]||"",this.rules);return{type:"code",raw:i,lang:s[2]?s[2].trim().replace(this.rules.inline.anyPunctuation,"$1"):s[2],text:o}}}heading(t){const s=this.rules.block.heading.exec(t);if(s){let i=s[2].trim();if(this.rules.other.endingHash.test(i)){const o=ri(i,"#");(this.options.pedantic||!o||this.rules.other.endingSpaceChar.test(o))&&(i=o.trim())}return{type:"heading",raw:s[0],depth:s[1].length,text:i,tokens:this.lexer.inline(i)}}}hr(t){const s=this.rules.block.hr.exec(t);if(s)return{type:"hr",raw:ri(s[0],`
`)}}blockquote(t){const s=this.rules.block.blockquote.exec(t);if(s){let i=ri(s[0],`
`).split(`
`),o="",r="";const a=[];for(;i.length>0;){let l=!1;const c=[];let h;for(h=0;h<i.length;h++)if(this.rules.other.blockquoteStart.test(i[h]))c.push(i[h]),l=!0;else if(!l)c.push(i[h]);else break;i=i.slice(h);const u=c.join(`
`),p=u.replace(this.rules.other.blockquoteSetextReplace,`
    $1`).replace(this.rules.other.blockquoteSetextReplace2,"");o=o?`${o}
${u}`:u,r=r?`${r}
${p}`:p;const f=this.lexer.state.top;if(this.lexer.state.top=!0,this.lexer.blockTokens(p,a,!0),this.lexer.state.top=f,i.length===0)break;const m=a.at(-1);if((m==null?void 0:m.type)==="code")break;if((m==null?void 0:m.type)==="blockquote"){const b=m,y=b.raw+`
`+i.join(`
`),k=this.blockquote(y);a[a.length-1]=k,o=o.substring(0,o.length-b.raw.length)+k.raw,r=r.substring(0,r.length-b.text.length)+k.text;break}else if((m==null?void 0:m.type)==="list"){const b=m,y=b.raw+`
`+i.join(`
`),k=this.list(y);a[a.length-1]=k,o=o.substring(0,o.length-m.raw.length)+k.raw,r=r.substring(0,r.length-b.raw.length)+k.raw,i=y.substring(a.at(-1).raw.length).split(`
`);continue}}return{type:"blockquote",raw:o,tokens:a,text:r}}}list(t){let s=this.rules.block.list.exec(t);if(s){let i=s[1].trim();const o=i.length>1,r={type:"list",raw:"",ordered:o,start:o?+i.slice(0,-1):"",loose:!1,items:[]};i=o?`\\d{1,9}\\${i.slice(-1)}`:`\\${i}`,this.options.pedantic&&(i=o?i:"[*+-]");const a=this.rules.other.listItemRegex(i);let l=!1;for(;t;){let h=!1,u="",p="";if(!(s=a.exec(t))||this.rules.block.hr.test(t))break;u=s[0],t=t.substring(u.length);let f=s[2].split(`
`,1)[0].replace(this.rules.other.listReplaceTabs,_=>" ".repeat(3*_.length)),m=t.split(`
`,1)[0],b=!f.trim(),y=0;if(this.options.pedantic?(y=2,p=f.trimStart()):b?y=s[1].length+1:(y=s[2].search(this.rules.other.nonSpaceChar),y=y>4?1:y,p=f.slice(y),y+=s[1].length),b&&this.rules.other.blankLine.test(m)&&(u+=m+`
`,t=t.substring(m.length+1),h=!0),!h){const _=this.rules.other.nextBulletRegex(y),S=this.rules.other.hrRegex(y),w=this.rules.other.fencesBeginRegex(y),x=this.rules.other.headingBeginRegex(y),D=this.rules.other.htmlBeginRegex(y);for(;t;){const B=t.split(`
`,1)[0];let U;if(m=B,this.options.pedantic?(m=m.replace(this.rules.other.listReplaceNesting,"  "),U=m):U=m.replace(this.rules.other.tabCharGlobal,"    "),w.test(m)||x.test(m)||D.test(m)||_.test(m)||S.test(m))break;if(U.search(this.rules.other.nonSpaceChar)>=y||!m.trim())p+=`
`+U.slice(y);else{if(b||f.replace(this.rules.other.tabCharGlobal,"    ").search(this.rules.other.nonSpaceChar)>=4||w.test(f)||x.test(f)||S.test(f))break;p+=`
`+m}!b&&!m.trim()&&(b=!0),u+=B+`
`,t=t.substring(B.length+1),f=U.slice(y)}}r.loose||(l?r.loose=!0:this.rules.other.doubleBlankLine.test(u)&&(l=!0));let k=null,z;this.options.gfm&&(k=this.rules.other.listIsTask.exec(p),k&&(z=k[0]!=="[ ] ",p=p.replace(this.rules.other.listReplaceTask,""))),r.items.push({type:"list_item",raw:u,task:!!k,checked:z,loose:!1,text:p,tokens:[]}),r.raw+=u}const c=r.items.at(-1);c&&(c.raw=c.raw.trimEnd(),c.text=c.text.trimEnd()),r.raw=r.raw.trimEnd();for(let h=0;h<r.items.length;h++)if(this.lexer.state.top=!1,r.items[h].tokens=this.lexer.blockTokens(r.items[h].text,[]),!r.loose){const u=r.items[h].tokens.filter(f=>f.type==="space"),p=u.length>0&&u.some(f=>this.rules.other.anyLine.test(f.raw));r.loose=p}if(r.loose)for(let h=0;h<r.items.length;h++)r.items[h].loose=!0;return r}}html(t){const s=this.rules.block.html.exec(t);if(s)return{type:"html",block:!0,raw:s[0],pre:s[1]==="pre"||s[1]==="script"||s[1]==="style",text:s[0]}}def(t){const s=this.rules.block.def.exec(t);if(s){const i=s[1].toLowerCase().replace(this.rules.other.multipleSpaceGlobal," "),o=s[2]?s[2].replace(this.rules.other.hrefBrackets,"$1").replace(this.rules.inline.anyPunctuation,"$1"):"",r=s[3]?s[3].substring(1,s[3].length-1).replace(this.rules.inline.anyPunctuation,"$1"):s[3];return{type:"def",tag:i,raw:s[0],href:o,title:r}}}table(t){var l;const s=this.rules.block.table.exec(t);if(!s||!this.rules.other.tableDelimiter.test(s[2]))return;const i=zn(s[1]),o=s[2].replace(this.rules.other.tableAlignChars,"").split("|"),r=(l=s[3])!=null&&l.trim()?s[3].replace(this.rules.other.tableRowBlankLine,"").split(`
`):[],a={type:"table",raw:s[0],header:[],align:[],rows:[]};if(i.length===o.length){for(const c of o)this.rules.other.tableAlignRight.test(c)?a.align.push("right"):this.rules.other.tableAlignCenter.test(c)?a.align.push("center"):this.rules.other.tableAlignLeft.test(c)?a.align.push("left"):a.align.push(null);for(let c=0;c<i.length;c++)a.header.push({text:i[c],tokens:this.lexer.inline(i[c]),header:!0,align:a.align[c]});for(const c of r)a.rows.push(zn(c,a.header.length).map((h,u)=>({text:h,tokens:this.lexer.inline(h),header:!1,align:a.align[u]})));return a}}lheading(t){const s=this.rules.block.lheading.exec(t);if(s)return{type:"heading",raw:s[0],depth:s[2].charAt(0)==="="?1:2,text:s[1],tokens:this.lexer.inline(s[1])}}paragraph(t){const s=this.rules.block.paragraph.exec(t);if(s){const i=s[1].charAt(s[1].length-1)===`
`?s[1].slice(0,-1):s[1];return{type:"paragraph",raw:s[0],text:i,tokens:this.lexer.inline(i)}}}text(t){const s=this.rules.block.text.exec(t);if(s)return{type:"text",raw:s[0],text:s[0],tokens:this.lexer.inline(s[0])}}escape(t){const s=this.rules.inline.escape.exec(t);if(s)return{type:"escape",raw:s[0],text:s[1]}}tag(t){const s=this.rules.inline.tag.exec(t);if(s)return!this.lexer.state.inLink&&this.rules.other.startATag.test(s[0])?this.lexer.state.inLink=!0:this.lexer.state.inLink&&this.rules.other.endATag.test(s[0])&&(this.lexer.state.inLink=!1),!this.lexer.state.inRawBlock&&this.rules.other.startPreScriptTag.test(s[0])?this.lexer.state.inRawBlock=!0:this.lexer.state.inRawBlock&&this.rules.other.endPreScriptTag.test(s[0])&&(this.lexer.state.inRawBlock=!1),{type:"html",raw:s[0],inLink:this.lexer.state.inLink,inRawBlock:this.lexer.state.inRawBlock,block:!1,text:s[0]}}link(t){const s=this.rules.inline.link.exec(t);if(s){const i=s[2].trim();if(!this.options.pedantic&&this.rules.other.startAngleBracket.test(i)){if(!this.rules.other.endAngleBracket.test(i))return;const a=ri(i.slice(0,-1),"\\");if((i.length-a.length)%2===0)return}else{const a=Rm(s[2],"()");if(a>-1){const c=(s[0].indexOf("!")===0?5:4)+s[1].length+a;s[2]=s[2].substring(0,a),s[0]=s[0].substring(0,c).trim(),s[3]=""}}let o=s[2],r="";if(this.options.pedantic){const a=this.rules.other.pedanticHrefTitle.exec(o);a&&(o=a[1],r=a[3])}else r=s[3]?s[3].slice(1,-1):"";return o=o.trim(),this.rules.other.startAngleBracket.test(o)&&(this.options.pedantic&&!this.rules.other.endAngleBracket.test(i)?o=o.slice(1):o=o.slice(1,-1)),En(s,{href:o&&o.replace(this.rules.inline.anyPunctuation,"$1"),title:r&&r.replace(this.rules.inline.anyPunctuation,"$1")},s[0],this.lexer,this.rules)}}reflink(t,s){let i;if((i=this.rules.inline.reflink.exec(t))||(i=this.rules.inline.nolink.exec(t))){const o=(i[2]||i[1]).replace(this.rules.other.multipleSpaceGlobal," "),r=s[o.toLowerCase()];if(!r){const a=i[0].charAt(0);return{type:"text",raw:a,text:a}}return En(i,r,i[0],this.lexer,this.rules)}}emStrong(t,s,i=""){let o=this.rules.inline.emStrongLDelim.exec(t);if(!o||o[3]&&i.match(this.rules.other.unicodeAlphaNumeric))return;if(!(o[1]||o[2]||"")||!i||this.rules.inline.punctuation.exec(i)){const a=[...o[0]].length-1;let l,c,h=a,u=0;const p=o[0][0]==="*"?this.rules.inline.emStrongRDelimAst:this.rules.inline.emStrongRDelimUnd;for(p.lastIndex=0,s=s.slice(-1*t.length+a);(o=p.exec(s))!=null;){if(l=o[1]||o[2]||o[3]||o[4]||o[5]||o[6],!l)continue;if(c=[...l].length,o[3]||o[4]){h+=c;continue}else if((o[5]||o[6])&&a%3&&!((a+c)%3)){u+=c;continue}if(h-=c,h>0)continue;c=Math.min(c,c+h+u);const f=[...o[0]][0].length,m=t.slice(0,a+o.index+f+c);if(Math.min(a,c)%2){const y=m.slice(1,-1);return{type:"em",raw:m,text:y,tokens:this.lexer.inlineTokens(y)}}const b=m.slice(2,-2);return{type:"strong",raw:m,text:b,tokens:this.lexer.inlineTokens(b)}}}}codespan(t){const s=this.rules.inline.code.exec(t);if(s){let i=s[2].replace(this.rules.other.newLineCharGlobal," ");const o=this.rules.other.nonSpaceChar.test(i),r=this.rules.other.startingSpaceChar.test(i)&&this.rules.other.endingSpaceChar.test(i);return o&&r&&(i=i.substring(1,i.length-1)),{type:"codespan",raw:s[0],text:i}}}br(t){const s=this.rules.inline.br.exec(t);if(s)return{type:"br",raw:s[0]}}del(t){const s=this.rules.inline.del.exec(t);if(s)return{type:"del",raw:s[0],text:s[2],tokens:this.lexer.inlineTokens(s[2])}}autolink(t){const s=this.rules.inline.autolink.exec(t);if(s){let i,o;return s[2]==="@"?(i=s[1],o="mailto:"+i):(i=s[1],o=i),{type:"link",raw:s[0],text:i,href:o,tokens:[{type:"text",raw:i,text:i}]}}}url(t){var i;let s;if(s=this.rules.inline.url.exec(t)){let o,r;if(s[2]==="@")o=s[0],r="mailto:"+o;else{let a;do a=s[0],s[0]=((i=this.rules.inline._backpedal.exec(s[0]))==null?void 0:i[0])??"";while(a!==s[0]);o=s[0],s[1]==="www."?r="http://"+s[0]:r=s[0]}return{type:"link",raw:s[0],text:o,href:r,tokens:[{type:"text",raw:o,text:o}]}}}inlineText(t){const s=this.rules.inline.text.exec(t);if(s){const i=this.lexer.state.inRawBlock;return{type:"text",raw:s[0],text:s[0],escaped:i}}}}class ae{constructor(t){st(this,"tokens");st(this,"options");st(this,"state");st(this,"tokenizer");st(this,"inlineQueue");this.tokens=[],this.tokens.links=Object.create(null),this.options=t||ms,this.options.tokenizer=this.options.tokenizer||new bo,this.tokenizer=this.options.tokenizer,this.tokenizer.options=this.options,this.tokenizer.lexer=this,this.inlineQueue=[],this.state={inLink:!1,inRawBlock:!1,top:!0};const s={other:Ft,block:Ji.normal,inline:oi.normal};this.options.pedantic?(s.block=Ji.pedantic,s.inline=oi.pedantic):this.options.gfm&&(s.block=Ji.gfm,this.options.breaks?s.inline=oi.breaks:s.inline=oi.gfm),this.tokenizer.rules=s}static get rules(){return{block:Ji,inline:oi}}static lex(t,s){return new ae(s).lex(t)}static lexInline(t,s){return new ae(s).inlineTokens(t)}lex(t){t=t.replace(Ft.carriageReturn,`
`),this.blockTokens(t,this.tokens);for(let s=0;s<this.inlineQueue.length;s++){const i=this.inlineQueue[s];this.inlineTokens(i.src,i.tokens)}return this.inlineQueue=[],this.tokens}blockTokens(t,s=[],i=!1){var o,r,a;for(this.options.pedantic&&(t=t.replace(Ft.tabCharGlobal,"    ").replace(Ft.spaceLine,""));t;){let l;if((r=(o=this.options.extensions)==null?void 0:o.block)!=null&&r.some(h=>(l=h.call({lexer:this},t,s))?(t=t.substring(l.raw.length),s.push(l),!0):!1))continue;if(l=this.tokenizer.space(t)){t=t.substring(l.raw.length);const h=s.at(-1);l.raw.length===1&&h!==void 0?h.raw+=`
`:s.push(l);continue}if(l=this.tokenizer.code(t)){t=t.substring(l.raw.length);const h=s.at(-1);(h==null?void 0:h.type)==="paragraph"||(h==null?void 0:h.type)==="text"?(h.raw+=`
`+l.raw,h.text+=`
`+l.text,this.inlineQueue.at(-1).src=h.text):s.push(l);continue}if(l=this.tokenizer.fences(t)){t=t.substring(l.raw.length),s.push(l);continue}if(l=this.tokenizer.heading(t)){t=t.substring(l.raw.length),s.push(l);continue}if(l=this.tokenizer.hr(t)){t=t.substring(l.raw.length),s.push(l);continue}if(l=this.tokenizer.blockquote(t)){t=t.substring(l.raw.length),s.push(l);continue}if(l=this.tokenizer.list(t)){t=t.substring(l.raw.length),s.push(l);continue}if(l=this.tokenizer.html(t)){t=t.substring(l.raw.length),s.push(l);continue}if(l=this.tokenizer.def(t)){t=t.substring(l.raw.length);const h=s.at(-1);(h==null?void 0:h.type)==="paragraph"||(h==null?void 0:h.type)==="text"?(h.raw+=`
`+l.raw,h.text+=`
`+l.raw,this.inlineQueue.at(-1).src=h.text):this.tokens.links[l.tag]||(this.tokens.links[l.tag]={href:l.href,title:l.title});continue}if(l=this.tokenizer.table(t)){t=t.substring(l.raw.length),s.push(l);continue}if(l=this.tokenizer.lheading(t)){t=t.substring(l.raw.length),s.push(l);continue}let c=t;if((a=this.options.extensions)!=null&&a.startBlock){let h=1/0;const u=t.slice(1);let p;this.options.extensions.startBlock.forEach(f=>{p=f.call({lexer:this},u),typeof p=="number"&&p>=0&&(h=Math.min(h,p))}),h<1/0&&h>=0&&(c=t.substring(0,h+1))}if(this.state.top&&(l=this.tokenizer.paragraph(c))){const h=s.at(-1);i&&(h==null?void 0:h.type)==="paragraph"?(h.raw+=`
`+l.raw,h.text+=`
`+l.text,this.inlineQueue.pop(),this.inlineQueue.at(-1).src=h.text):s.push(l),i=c.length!==t.length,t=t.substring(l.raw.length);continue}if(l=this.tokenizer.text(t)){t=t.substring(l.raw.length);const h=s.at(-1);(h==null?void 0:h.type)==="text"?(h.raw+=`
`+l.raw,h.text+=`
`+l.text,this.inlineQueue.pop(),this.inlineQueue.at(-1).src=h.text):s.push(l);continue}if(t){const h="Infinite loop on byte: "+t.charCodeAt(0);if(this.options.silent){console.error(h);break}else throw new Error(h)}}return this.state.top=!0,s}inline(t,s=[]){return this.inlineQueue.push({src:t,tokens:s}),s}inlineTokens(t,s=[]){var l,c,h;let i=t,o=null;if(this.tokens.links){const u=Object.keys(this.tokens.links);if(u.length>0)for(;(o=this.tokenizer.rules.inline.reflinkSearch.exec(i))!=null;)u.includes(o[0].slice(o[0].lastIndexOf("[")+1,-1))&&(i=i.slice(0,o.index)+"["+"a".repeat(o[0].length-2)+"]"+i.slice(this.tokenizer.rules.inline.reflinkSearch.lastIndex))}for(;(o=this.tokenizer.rules.inline.blockSkip.exec(i))!=null;)i=i.slice(0,o.index)+"["+"a".repeat(o[0].length-2)+"]"+i.slice(this.tokenizer.rules.inline.blockSkip.lastIndex);for(;(o=this.tokenizer.rules.inline.anyPunctuation.exec(i))!=null;)i=i.slice(0,o.index)+"++"+i.slice(this.tokenizer.rules.inline.anyPunctuation.lastIndex);let r=!1,a="";for(;t;){r||(a=""),r=!1;let u;if((c=(l=this.options.extensions)==null?void 0:l.inline)!=null&&c.some(f=>(u=f.call({lexer:this},t,s))?(t=t.substring(u.raw.length),s.push(u),!0):!1))continue;if(u=this.tokenizer.escape(t)){t=t.substring(u.raw.length),s.push(u);continue}if(u=this.tokenizer.tag(t)){t=t.substring(u.raw.length),s.push(u);continue}if(u=this.tokenizer.link(t)){t=t.substring(u.raw.length),s.push(u);continue}if(u=this.tokenizer.reflink(t,this.tokens.links)){t=t.substring(u.raw.length);const f=s.at(-1);u.type==="text"&&(f==null?void 0:f.type)==="text"?(f.raw+=u.raw,f.text+=u.text):s.push(u);continue}if(u=this.tokenizer.emStrong(t,i,a)){t=t.substring(u.raw.length),s.push(u);continue}if(u=this.tokenizer.codespan(t)){t=t.substring(u.raw.length),s.push(u);continue}if(u=this.tokenizer.br(t)){t=t.substring(u.raw.length),s.push(u);continue}if(u=this.tokenizer.del(t)){t=t.substring(u.raw.length),s.push(u);continue}if(u=this.tokenizer.autolink(t)){t=t.substring(u.raw.length),s.push(u);continue}if(!this.state.inLink&&(u=this.tokenizer.url(t))){t=t.substring(u.raw.length),s.push(u);continue}let p=t;if((h=this.options.extensions)!=null&&h.startInline){let f=1/0;const m=t.slice(1);let b;this.options.extensions.startInline.forEach(y=>{b=y.call({lexer:this},m),typeof b=="number"&&b>=0&&(f=Math.min(f,b))}),f<1/0&&f>=0&&(p=t.substring(0,f+1))}if(u=this.tokenizer.inlineText(p)){t=t.substring(u.raw.length),u.raw.slice(-1)!=="_"&&(a=u.raw.slice(-1)),r=!0;const f=s.at(-1);(f==null?void 0:f.type)==="text"?(f.raw+=u.raw,f.text+=u.text):s.push(u);continue}if(t){const f="Infinite loop on byte: "+t.charCodeAt(0);if(this.options.silent){console.error(f);break}else throw new Error(f)}}return s}}class vo{constructor(t){st(this,"options");st(this,"parser");this.options=t||ms}space(t){return""}code({text:t,lang:s,escaped:i}){var a;const o=(a=(s||"").match(Ft.notSpaceStart))==null?void 0:a[0],r=t.replace(Ft.endingNewline,"")+`
`;return o?'<pre><code class="language-'+ke(o)+'">'+(i?r:ke(r,!0))+`</code></pre>
`:"<pre><code>"+(i?r:ke(r,!0))+`</code></pre>
`}blockquote({tokens:t}){return`<blockquote>
${this.parser.parse(t)}</blockquote>
`}html({text:t}){return t}heading({tokens:t,depth:s}){return`<h${s}>${this.parser.parseInline(t)}</h${s}>
`}hr(t){return`<hr>
`}list(t){const s=t.ordered,i=t.start;let o="";for(let l=0;l<t.items.length;l++){const c=t.items[l];o+=this.listitem(c)}const r=s?"ol":"ul",a=s&&i!==1?' start="'+i+'"':"";return"<"+r+a+`>
`+o+"</"+r+`>
`}listitem(t){var i;let s="";if(t.task){const o=this.checkbox({checked:!!t.checked});t.loose?((i=t.tokens[0])==null?void 0:i.type)==="paragraph"?(t.tokens[0].text=o+" "+t.tokens[0].text,t.tokens[0].tokens&&t.tokens[0].tokens.length>0&&t.tokens[0].tokens[0].type==="text"&&(t.tokens[0].tokens[0].text=o+" "+ke(t.tokens[0].tokens[0].text),t.tokens[0].tokens[0].escaped=!0)):t.tokens.unshift({type:"text",raw:o+" ",text:o+" ",escaped:!0}):s+=o+" "}return s+=this.parser.parse(t.tokens,!!t.loose),`<li>${s}</li>
`}checkbox({checked:t}){return"<input "+(t?'checked="" ':"")+'disabled="" type="checkbox">'}paragraph({tokens:t}){return`<p>${this.parser.parseInline(t)}</p>
`}table(t){let s="",i="";for(let r=0;r<t.header.length;r++)i+=this.tablecell(t.header[r]);s+=this.tablerow({text:i});let o="";for(let r=0;r<t.rows.length;r++){const a=t.rows[r];i="";for(let l=0;l<a.length;l++)i+=this.tablecell(a[l]);o+=this.tablerow({text:i})}return o&&(o=`<tbody>${o}</tbody>`),`<table>
<thead>
`+s+`</thead>
`+o+`</table>
`}tablerow({text:t}){return`<tr>
${t}</tr>
`}tablecell(t){const s=this.parser.parseInline(t.tokens),i=t.header?"th":"td";return(t.align?`<${i} align="${t.align}">`:`<${i}>`)+s+`</${i}>
`}strong({tokens:t}){return`<strong>${this.parser.parseInline(t)}</strong>`}em({tokens:t}){return`<em>${this.parser.parseInline(t)}</em>`}codespan({text:t}){return`<code>${ke(t,!0)}</code>`}br(t){return"<br>"}del({tokens:t}){return`<del>${this.parser.parseInline(t)}</del>`}link({href:t,title:s,tokens:i}){const o=this.parser.parseInline(i),r=Tn(t);if(r===null)return o;t=r;let a='<a href="'+t+'"';return s&&(a+=' title="'+ke(s)+'"'),a+=">"+o+"</a>",a}image({href:t,title:s,text:i}){const o=Tn(t);if(o===null)return ke(i);t=o;let r=`<img src="${t}" alt="${i}"`;return s&&(r+=` title="${ke(s)}"`),r+=">",r}text(t){return"tokens"in t&&t.tokens?this.parser.parseInline(t.tokens):"escaped"in t&&t.escaped?t.text:ke(t.text)}}class ea{strong({text:t}){return t}em({text:t}){return t}codespan({text:t}){return t}del({text:t}){return t}html({text:t}){return t}text({text:t}){return t}link({text:t}){return""+t}image({text:t}){return""+t}br(){return""}}class ne{constructor(t){st(this,"options");st(this,"renderer");st(this,"textRenderer");this.options=t||ms,this.options.renderer=this.options.renderer||new vo,this.renderer=this.options.renderer,this.renderer.options=this.options,this.renderer.parser=this,this.textRenderer=new ea}static parse(t,s){return new ne(s).parse(t)}static parseInline(t,s){return new ne(s).parseInline(t)}parse(t,s=!0){var o,r;let i="";for(let a=0;a<t.length;a++){const l=t[a];if((r=(o=this.options.extensions)==null?void 0:o.renderers)!=null&&r[l.type]){const h=l,u=this.options.extensions.renderers[h.type].call({parser:this},h);if(u!==!1||!["space","hr","heading","code","table","blockquote","list","html","paragraph","text"].includes(h.type)){i+=u||"";continue}}const c=l;switch(c.type){case"space":{i+=this.renderer.space(c);continue}case"hr":{i+=this.renderer.hr(c);continue}case"heading":{i+=this.renderer.heading(c);continue}case"code":{i+=this.renderer.code(c);continue}case"table":{i+=this.renderer.table(c);continue}case"blockquote":{i+=this.renderer.blockquote(c);continue}case"list":{i+=this.renderer.list(c);continue}case"html":{i+=this.renderer.html(c);continue}case"paragraph":{i+=this.renderer.paragraph(c);continue}case"text":{let h=c,u=this.renderer.text(h);for(;a+1<t.length&&t[a+1].type==="text";)h=t[++a],u+=`
`+this.renderer.text(h);s?i+=this.renderer.paragraph({type:"paragraph",raw:u,text:u,tokens:[{type:"text",raw:u,text:u,escaped:!0}]}):i+=u;continue}default:{const h='Token with "'+c.type+'" type was not found.';if(this.options.silent)return console.error(h),"";throw new Error(h)}}}return i}parseInline(t,s=this.renderer){var o,r;let i="";for(let a=0;a<t.length;a++){const l=t[a];if((r=(o=this.options.extensions)==null?void 0:o.renderers)!=null&&r[l.type]){const h=this.options.extensions.renderers[l.type].call({parser:this},l);if(h!==!1||!["escape","html","link","image","strong","em","codespan","br","del","text"].includes(l.type)){i+=h||"";continue}}const c=l;switch(c.type){case"escape":{i+=s.text(c);break}case"html":{i+=s.html(c);break}case"link":{i+=s.link(c);break}case"image":{i+=s.image(c);break}case"strong":{i+=s.strong(c);break}case"em":{i+=s.em(c);break}case"codespan":{i+=s.codespan(c);break}case"br":{i+=s.br(c);break}case"del":{i+=s.del(c);break}case"text":{i+=s.text(c);break}default:{const h='Token with "'+c.type+'" type was not found.';if(this.options.silent)return console.error(h),"";throw new Error(h)}}}return i}}class gi{constructor(t){st(this,"options");st(this,"block");this.options=t||ms}preprocess(t){return t}postprocess(t){return t}processAllTokens(t){return t}provideLexer(){return this.block?ae.lex:ae.lexInline}provideParser(){return this.block?ne.parse:ne.parseInline}}st(gi,"passThroughHooks",new Set(["preprocess","postprocess","processAllTokens"]));class Mm{constructor(...t){st(this,"defaults",Yr());st(this,"options",this.setOptions);st(this,"parse",this.parseMarkdown(!0));st(this,"parseInline",this.parseMarkdown(!1));st(this,"Parser",ne);st(this,"Renderer",vo);st(this,"TextRenderer",ea);st(this,"Lexer",ae);st(this,"Tokenizer",bo);st(this,"Hooks",gi);this.use(...t)}walkTokens(t,s){var o,r;let i=[];for(const a of t)switch(i=i.concat(s.call(this,a)),a.type){case"table":{const l=a;for(const c of l.header)i=i.concat(this.walkTokens(c.tokens,s));for(const c of l.rows)for(const h of c)i=i.concat(this.walkTokens(h.tokens,s));break}case"list":{const l=a;i=i.concat(this.walkTokens(l.items,s));break}default:{const l=a;(r=(o=this.defaults.extensions)==null?void 0:o.childTokens)!=null&&r[l.type]?this.defaults.extensions.childTokens[l.type].forEach(c=>{const h=l[c].flat(1/0);i=i.concat(this.walkTokens(h,s))}):l.tokens&&(i=i.concat(this.walkTokens(l.tokens,s)))}}return i}use(...t){const s=this.defaults.extensions||{renderers:{},childTokens:{}};return t.forEach(i=>{const o={...i};if(o.async=this.defaults.async||o.async||!1,i.extensions&&(i.extensions.forEach(r=>{if(!r.name)throw new Error("extension name required");if("renderer"in r){const a=s.renderers[r.name];a?s.renderers[r.name]=function(...l){let c=r.renderer.apply(this,l);return c===!1&&(c=a.apply(this,l)),c}:s.renderers[r.name]=r.renderer}if("tokenizer"in r){if(!r.level||r.level!=="block"&&r.level!=="inline")throw new Error("extension level must be 'block' or 'inline'");const a=s[r.level];a?a.unshift(r.tokenizer):s[r.level]=[r.tokenizer],r.start&&(r.level==="block"?s.startBlock?s.startBlock.push(r.start):s.startBlock=[r.start]:r.level==="inline"&&(s.startInline?s.startInline.push(r.start):s.startInline=[r.start]))}"childTokens"in r&&r.childTokens&&(s.childTokens[r.name]=r.childTokens)}),o.extensions=s),i.renderer){const r=this.defaults.renderer||new vo(this.defaults);for(const a in i.renderer){if(!(a in r))throw new Error(`renderer '${a}' does not exist`);if(["options","parser"].includes(a))continue;const l=a,c=i.renderer[l],h=r[l];r[l]=(...u)=>{let p=c.apply(r,u);return p===!1&&(p=h.apply(r,u)),p||""}}o.renderer=r}if(i.tokenizer){const r=this.defaults.tokenizer||new bo(this.defaults);for(const a in i.tokenizer){if(!(a in r))throw new Error(`tokenizer '${a}' does not exist`);if(["options","rules","lexer"].includes(a))continue;const l=a,c=i.tokenizer[l],h=r[l];r[l]=(...u)=>{let p=c.apply(r,u);return p===!1&&(p=h.apply(r,u)),p}}o.tokenizer=r}if(i.hooks){const r=this.defaults.hooks||new gi;for(const a in i.hooks){if(!(a in r))throw new Error(`hook '${a}' does not exist`);if(["options","block"].includes(a))continue;const l=a,c=i.hooks[l],h=r[l];gi.passThroughHooks.has(a)?r[l]=u=>{if(this.defaults.async)return Promise.resolve(c.call(r,u)).then(f=>h.call(r,f));const p=c.call(r,u);return h.call(r,p)}:r[l]=(...u)=>{let p=c.apply(r,u);return p===!1&&(p=h.apply(r,u)),p}}o.hooks=r}if(i.walkTokens){const r=this.defaults.walkTokens,a=i.walkTokens;o.walkTokens=function(l){let c=[];return c.push(a.call(this,l)),r&&(c=c.concat(r.call(this,l))),c}}this.defaults={...this.defaults,...o}}),this}setOptions(t){return this.defaults={...this.defaults,...t},this}lexer(t,s){return ae.lex(t,s??this.defaults)}parser(t,s){return ne.parse(t,s??this.defaults)}parseMarkdown(t){return(i,o)=>{const r={...o},a={...this.defaults,...r},l=this.onError(!!a.silent,!!a.async);if(this.defaults.async===!0&&r.async===!1)return l(new Error("marked(): The async option was set to true by an extension. Remove async: false from the parse options object to return a Promise."));if(typeof i>"u"||i===null)return l(new Error("marked(): input parameter is undefined or null"));if(typeof i!="string")return l(new Error("marked(): input parameter is of type "+Object.prototype.toString.call(i)+", string expected"));a.hooks&&(a.hooks.options=a,a.hooks.block=t);const c=a.hooks?a.hooks.provideLexer():t?ae.lex:ae.lexInline,h=a.hooks?a.hooks.provideParser():t?ne.parse:ne.parseInline;if(a.async)return Promise.resolve(a.hooks?a.hooks.preprocess(i):i).then(u=>c(u,a)).then(u=>a.hooks?a.hooks.processAllTokens(u):u).then(u=>a.walkTokens?Promise.all(this.walkTokens(u,a.walkTokens)).then(()=>u):u).then(u=>h(u,a)).then(u=>a.hooks?a.hooks.postprocess(u):u).catch(l);try{a.hooks&&(i=a.hooks.preprocess(i));let u=c(i,a);a.hooks&&(u=a.hooks.processAllTokens(u)),a.walkTokens&&this.walkTokens(u,a.walkTokens);let p=h(u,a);return a.hooks&&(p=a.hooks.postprocess(p)),p}catch(u){return l(u)}}}onError(t,s){return i=>{if(i.message+=`
Please report this to https://github.com/markedjs/marked.`,t){const o="<p>An error occurred:</p><pre>"+ke(i.message+"",!0)+"</pre>";return s?Promise.resolve(o):o}if(s)return Promise.reject(i);throw i}}}const ns=new Mm;function J(e,t){return ns.parse(e,t)}J.options=J.setOptions=function(e){return ns.setOptions(e),J.defaults=ns.defaults,Al(J.defaults),J};J.getDefaults=Yr;J.defaults=ms;J.use=function(...e){return ns.use(...e),J.defaults=ns.defaults,Al(J.defaults),J};J.walkTokens=function(e,t){return ns.walkTokens(e,t)};J.parseInline=ns.parseInline;J.Parser=ne;J.parser=ne.parse;J.Renderer=vo;J.TextRenderer=ea;J.Lexer=ae;J.lexer=ae.lex;J.Tokenizer=bo;J.Hooks=gi;J.parse=J;J.options;J.setOptions;J.use;J.walkTokens;J.parseInline;ne.parse;ae.lex;var Nm=Object.defineProperty,Fm=Object.getOwnPropertyDescriptor,Ll=(e,t,s,i)=>{for(var o=i>1?void 0:i?Fm(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&Nm(t,s,o),o};let yo=class extends tt{constructor(){super(...arguments),this.content="",this.parse=async e=>(e=e.replace(/^[\u200B\u200C\u200D\u200E\u200F\uFEFF]/,""),e=await J.parse(e,{async:!0,gfm:!0}),e=Sl.sanitize(e),pi(e))}render(){return Kr(this.parse(this.content),v`<sl-skeleton></sl-skeleton>`)}};yo.styles=A`
    :host {
      display: block;
      width: 100%;
    }
  `;Ll([d({type:String})],yo.prototype,"content",2);yo=Ll([lt("markdown-block")],yo);var Bm=Object.defineProperty,Um=Object.getOwnPropertyDescriptor,Ml=(e,t,s,i)=>{for(var o=i>1?void 0:i?Um(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&Bm(t,s,o),o};let wo=class extends tt{constructor(){super(...arguments),this.content=""}render(){return v`
      <pre class="text">${Sl.sanitize(this.content)}</pre>
    `}};wo.styles=A`
    :host {
      display: block;
      width: 100%;
    }

    .text {
      margin: 0;
      padding: 0;
      font-size: 14px;
      font-style: inherit;
      line-height: 1.5;
    }
  `;Ml([d({type:String})],wo.prototype,"content",2);wo=Ml([lt("text-block")],wo);var Hm=Object.defineProperty,Vm=Object.getOwnPropertyDescriptor,sa=(e,t,s,i)=>{for(var o=i>1?void 0:i?Vm(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&Hm(t,s,o),o};let _i=class extends tt{constructor(){super(...arguments),this.data="",this.alt=""}render(){return v`
      <div class="container">
        <img src="${this.data}" alt="${this.alt}" />
      </div>
    `}};_i.styles=A`
    :host {
      display: block;
      width: 100%;
    }

    .container {
      display: flex;
      justify-content: center;
      align-items: center;
      background-color: #f0f0f0;
      border-radius: 8px;
      padding: 8px;
      box-sizing: border-box;
    }

    img {
      width: 300px;
      height: 300px;
      border-radius: 8px;
    }
  `;sa([d({type:String})],_i.prototype,"data",2);sa([d({type:String})],_i.prototype,"alt",2);_i=sa([lt("image-block")],_i);var jm=Object.defineProperty,Wm=Object.getOwnPropertyDescriptor,ia=(e,t,s,i)=>{for(var o=i>1?void 0:i?Wm(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&jm(t,s,o),o};let ki=class extends tt{constructor(){super(...arguments),this.name=""}render(){return console.log(this.result),v`
      <div class="name">
        Tool: ${this.name}
      </div>
      <div class="result">
      ${this.result?v`  
        ${this.result.result.map(e=>v`
          <div class="question">${e.payload.content.question}</div>
          <div class="answer">${e.payload.content.answer}</div>
        `)}`:v`
          <sl-spinner></sl-spinner>
        `}
      </div>
    `}};ki.styles=A`
    :host {
      display: flex;
      flex-direction: column;
      width: 100%;
      background-color: #f0f0f0;
      border-radius: 8px;
      padding: 8px;
      box-sizing: border-box;
    }

    .name {
      width: 100%;
      font-weight: 600;
      font-size: 18px;
    }

    .result {
      width: 100%;
      margin-top: 8px;
      display: flex;
      flex-direction: column;
      gap: 8px;

      .question {
        font-weight: 600;
        font-size: 16px;
      }

      .answer {
        font-size: 14px;
      }

      .question, .answer {
        padding: 8px;
        border-radius: 8px;
        background-color: #fff;
      }

      .question {
        background-color: #f0f0f0;
      }
    }
  `;ia([d({type:String})],ki.prototype,"name",2);ia([d({type:Object})],ki.prototype,"result",2);ki=ia([lt("tool-block")],ki);var qm=Object.defineProperty,Km=Object.getOwnPropertyDescriptor,Us=(e,t,s,i)=>{for(var o=i>1?void 0:i?Km(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&qm(t,s,o),o};let Ye=class extends tt{constructor(){super(...arguments),this.placeholder="",this.rows=1,this.disabled=!1,this.value="",this.onKeydown=e=>{e.key==="Enter"&&!e.shiftKey&&(e.preventDefault(),this.onSend())},this.onInput=e=>{const t=e.target;t.style.height="auto",t.style.height=`${t.scrollHeight}px`,this.value=t.value,this.dispatchEvent(new CustomEvent("input",{detail:this.value}))},this.onSend=()=>{const e=this.value.trim();e&&(this.dispatchEvent(new CustomEvent("send",{detail:e})),this.value="")}}render(){return v`
      <div class="container">
        <!-- Input -->
        <div class="input">
          <textarea
            spellcheck="false"
            placeholder=${this.placeholder}
            rows=${this.rows}
            maxlength=${O(this.maxlength)}
            .value=${this.value}
            @input=${this.onInput}
            @keydown=${this.onKeydown}
          ></textarea>
        </div>

        <!-- Button Control -->
        <div class="control">
          <sl-icon-button
            name="paperclip"
          ></sl-icon-button>
          <div class="flex"></div>
          <sl-button
            size="small"
            variant="primary"
            ?circle=${!0}
            ?disabled=${this.disabled||!this.value.trim()}
            @click=${this.onSend}>
            <sl-icon name="send"></sl-icon>
          </sl-button>
        </div>

      </div>
    `}};Ye.styles=A`
    :host {
      display: block;
      width: 100%;
    }

    .container {
      display: flex;
      flex-direction: column;
      gap: 4px;
      padding: 8px;
      background-color: var(--sl-panel-background-color);
      border: 1px solid var(--sl-panel-border-color);
      border-radius: 4px;
      box-sizing: border-box;
      overflow: hidden;
    }

    .input {
      display: flex;
      padding: 8px;
      box-sizing: border-box;
      max-height: 200px;
      overflow-y: auto;

      textarea {
        width: 100%;
        height: auto;
        border: none;
        resize: none;
        outline: none;
        background-color: transparent;
        font-size: 14px;
        font-family: inherit;
        overflow: hidden;
      }
    }

    .control {
      grid-area: control;
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      gap: 8px;

      .flex {
        flex: 1;
      }
    }
  `;Us([d({type:String})],Ye.prototype,"placeholder",2);Us([d({type:Number})],Ye.prototype,"rows",2);Us([d({type:Number})],Ye.prototype,"maxlength",2);Us([d({type:Boolean})],Ye.prototype,"disabled",2);Us([d({type:String})],Ye.prototype,"value",2);Ye=Us([lt("message-input")],Ye);var Ym=Object.defineProperty,Gm=Object.getOwnPropertyDescriptor,oa=(e,t,s,i)=>{for(var o=i>1?void 0:i?Gm(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&Ym(t,s,o),o};let Ps=class extends tt{constructor(){super(...arguments),this.value=100,this.label="업로드 중",this.size=200,this.strokeWidth=10,this.radius=(this.size-this.strokeWidth)/2,this.circumference=2*Math.PI*this.radius}render(){const e=Math.min(Math.max(this.value,0),100),t=this.circumference-e/100*this.circumference;return v`
      <div class="progress-container">
        <svg
          class="progress-ring"
          width="${this.size}"
          height="${this.size}"
        >
          <circle
            class="progress-ring__background"
            stroke="var(--sl-color-gray-300)"
            stroke-width="${this.strokeWidth}"
            fill="transparent"
            r="${this.radius}"
            cx="${this.size/2}"
            cy="${this.size/2}"
          ></circle>
          <circle
            class="progress-ring__progress"
            stroke="var(--sl-color-primary-500)"
            stroke-width="${this.strokeWidth}"
            fill="transparent"
            r="${this.radius}"
            cx="${this.size/2}"
            cy="${this.size/2}"
            stroke-dasharray="${this.circumference}"
            stroke-dashoffset="${t}"
            style="transition: stroke-dashoffset 0.35s; transform: rotate(-90deg); transform-origin: center;"
          ></circle>
          <text
            x="50%"
            y="50%"
            dominant-baseline="middle"
            text-anchor="middle"
            font-size="20"
            fill="#fff"
          >
            ${Math.floor(e)}%
          </text>
        </svg>
        <p class="label">
          ${this.label}
        </p>
      </div>
    `}};Ps.styles=A`
    :host {
      position: fixed;
      top: 0;
      left: 0;
      width: 100vw;
      height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background-color: rgba(0, 0, 0, 0.5);
      z-index: 9999;
    }

    .progress-container {
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .progress-ring {
      transform: rotate(0deg);
    }

    .progress-ring__background {
      /* 배경 원의 스타일 */
      background-color: transparent;
    }

    .progress-ring__progress {
      /* 프로그레스 원의 스타일 */
      transition: stroke-dashoffset 0.35s;
      transform: rotate(0deg);
      transform-origin: center;
    }

    .label {
      margin-top: 16px;
      color: #fff;
      font-size: 1.2em;
      text-align: center;
    }
  `;oa([d({type:Number})],Ps.prototype,"value",2);oa([d({type:String})],Ps.prototype,"label",2);Ps=oa([lt("loading-overlay")],Ps);var Xm=Object.defineProperty,Zm=Object.getOwnPropertyDescriptor,Qm=(e,t,s,i)=>{for(var o=i>1?void 0:i?Zm(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&Xm(t,s,o),o};let Ar=class extends tt{render(){return v`
      <div class="intro">
        <p>Welcome</p>
        <p>Raggle is an app that combines the Rag system with a chatbot.</p>
      </div>
      <div class="input">
        <message-input
          
        ></message-input>
      </div>
    `}};Ar.styles=A`
    :host {
      width: 100%;
      height: 100%;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 32px;
      padding: 64px 16px;
      box-sizing: border-box;
    }

    .intro {
      width: 60%;
      display: flex;
      flex-direction: column;
      gap: 16px;
      align-items: center;

      p {
        margin: 0;
        font-size: 18px;
        font-weight: 600;
        color: var(--sl-color-gray-600);
      }
    }

    .input {
      width: 60%;
      display: flex;
      flex-direction: column;
      gap: 16px;
      align-items: center;
    }
  `;Ar=Qm([lt("home-page")],Ar);var Jm=Object.defineProperty,tg=Object.getOwnPropertyDescriptor,eg=(e,t,s,i)=>{for(var o=i>1?void 0:i?tg(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&Jm(t,s,o),o};let Tr=class extends tt{render(){return v`
      <div class="container">
        <h2>User Page</h2>
        <p>Welcome to the user page!</p>
      </div>
    `}};Tr.styles=A`
    :host {
      display: block;
      padding: 16px;
      font-family: Arial, sans-serif;
    }

    .container {
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 16px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    h2 {
      margin-top: 0;
    }

    p {
      margin: 0;
    }
  `;Tr=eg([lt("user-page")],Tr);var sg=Object.defineProperty,ig=Object.getOwnPropertyDescriptor,Nl=(e,t,s,i)=>{for(var o=i>1?void 0:i?ig(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&sg(t,s,o),o};let xo=class extends tt{constructor(){super(...arguments),this.status=404}render(){return v`
      <div class="error-container">
        <h1>Error ${this.status}</h1>
        <p>${this.getErrorMessage(this.status)}</p>
      </div>
    `}getErrorMessage(e){switch(e){case 400:return"Bad Request";case 401:return"Unauthorized";case 403:return"Forbidden";case 404:return"Page Not Found";case 500:return"Internal Server Error";default:return"An unexpected error occurred"}}};xo.styles=A`
    .error-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100%;
      text-align: center;
      color: var(--sl-color-gray-700);
    }

    h1 {
      font-size: 48px;
      margin: 0;
    }

    p {
      font-size: 24px;
      margin: 16px 0 0;
    }
  `;Nl([d({type:Number})],xo.prototype,"status",2);xo=Nl([lt("error-page")],xo);var og=Object.defineProperty,rg=Object.getOwnPropertyDescriptor,gs=(e,t,s,i)=>{for(var o=i>1?void 0:i?rg(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&og(t,s,o),o};let Le=class extends tt{constructor(){super(...arguments),this.key="",this.assistantId="",this.messages=[],this.onSend=async e=>{const t=e.detail;if(t){this.messages=[...this.messages,{role:"user",content:[{type:"text",index:0,text:t}]},{role:"assistant",content:[]}],await this.updateComplete,this.messagesEl.scrollTop=this.messagesEl.scrollHeight;const s=new Cf;s.start(),await Dt.Assistant.message(this.assistantId,this.messages,async i=>{i.endReason&&(s.stop(),console.log(`Elapsed time: ${s.elapsed}`)),i.content&&(this.assistantMsgs[this.assistantMsgs.length-1].appendContent(i.content),this.messagesEl.scrollTop=this.messagesEl.scrollHeight)})}}}render(){return v`
      <div class="messages">
        ${this.messages.map(e=>e.role==="user"?v`
              <user-message
                .message=${e}
              ></user-message>
            `:e.role==="assistant"?v`
              <assistant-message
                .message=${e}
              ></assistant-message>
            `:G)}
      </div>
      <div class="input">
        <message-input
          placeholder="Type a message..."
          rows="1"
          @send=${this.onSend}
        ></message-input>
      </div>
    `}};Le.styles=A`
    :host {
      position: relative;
      display: flex;
      flex-direction: column;
      width: 100%;
      height: 100%;
      padding: 16px;
      box-sizing: border-box;
    }

    .messages {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 16px;
      overflow-y: auto;
      padding: 32px 16px;
      box-sizing: border-box;
    }

    .input {
      width: 100%;
    }
  `;gs([T(".messages")],Le.prototype,"messagesEl",2);gs([Mn("user-message")],Le.prototype,"userMsgs",2);gs([Mn("assistant-message")],Le.prototype,"assistantMsgs",2);gs([d({type:String})],Le.prototype,"key",2);gs([d({type:String})],Le.prototype,"assistantId",2);gs([d({type:Array})],Le.prototype,"messages",2);Le=gs([lt("chat-room")],Le);var ag=Object.defineProperty,ng=Object.getOwnPropertyDescriptor,Fl=(e,t,s,i)=>{for(var o=i>1?void 0:i?ng(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&ag(t,s,o),o};let _o=class extends tt{constructor(){super(...arguments),this.assistants=[]}connectedCallback(){super.connectedCallback(),this.loadAsync().then(e=>this.assistants=e)}render(){var e;return v`
      <div class="container">
        <div class="header">
          <sl-icon name="robot"></sl-icon>
          <span>Assistant</span>
          <div class="flex"></div>
          <sl-icon name="question-circle"></sl-icon>
        </div>

        <sl-divider></sl-divider>

        <div class="control">
          <div class="flex"></div>
          <sl-button size="small" @click=${this.createAsync}>
            Create New
            <sl-icon slot="suffix" name="plus-lg"></sl-icon>
          </sl-button>
        </div>

        <div class="grid">
          ${(e=this.assistants)==null?void 0:e.map(t=>v`
            <assistant-card
              .assistant=${t}
              @delete=${this.deleteAsync}
              @select=${this.selectAsync}
            ></assistant-card>
          `)}
        </div>
      </div>
    `}async loadAsync(){return await Dt.Assistant.find()}async deleteAsync(e){const t=e.detail;await Dt.Assistant.delete(t),this.assistants=this.assistants.filter(s=>s.id!==t)}async selectAsync(e){const t=e.detail;xi(`/assistant/${t}`)}async createAsync(){xi("/assistant")}};_o.styles=A`
    :host {
      width: 100%;
      display: flex;
      padding: 64px 16px;
      justify-content: center;
      align-items: center;
      box-sizing: border-box;
    }

    .container {
      min-width: 700px;
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .header {
      width: 100%;
      display: flex;
      flex-direction: row;
      align-items: flex-end;
      justify-content: space-between;
      font-size: 28px;
      padding: 0px 16px;
      line-height: 32px;
      gap: 14px;
      box-sizing: border-box;

      span {
        font-size: 32px;
        font-weight: 600;
        color: var(--sl-color-gray-800);
      }

      .flex {
        flex: 1;
      }
    }

    .control {
      width: 100%;
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      gap: 16px;

      .flex {
        flex: 1;
      }
    }

    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 16px;
    }
  `;Fl([E()],_o.prototype,"assistants",2);_o=Fl([lt("assistant-explorer")],_o);var lg=Object.defineProperty,cg=Object.getOwnPropertyDescriptor,Ri=(e,t,s,i)=>{for(var o=i>1?void 0:i?cg(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&lg(t,s,o),o};let Te=class extends tt{constructor(){super(...arguments),this.assistant=Te.DefaultAssistant,this.status="draft",this.error="",this.loadAsync=async()=>{try{this.key?(this.assistant=await Dt.Assistant.get(this.key),this.status="updated"):(this.assistant=Te.DefaultAssistant,this.status="draft")}catch(e){this.error=e,this.status="error"}},this.onChange=e=>{const t=e.target,s=t.name.split(".");if(s[0]==="service-model"){const[i,o]=t.value.split("/");this.assistant.service=i,this.assistant.model=o}else if(s[0]==="tools"){const i=t.checked;this.assistant.tools=i?[...this.assistant.tools,s[1]]:this.assistant.tools.filter(o=>o!==s[1])}else s[0]==="toolOptions"?this.assistant[s[0]]={...this.assistant.toolOptions,[s[1]]:t.value}:s[0]==="options"?this.assistant[s[0]]={...this.assistant.options,[s[1]]:Number(t.value)}:this.assistant[s[0]]=t.value;this.debouncer?clearTimeout(this.debouncer):this.debouncer=setTimeout(this.onUpdate,5e3)},this.onUpdate=async()=>{if(this.validate())try{this.status="updating",this.assistant=await Dt.Assistant.upsert(this.assistant),this.key?this.status="updated":xi(`/assistant/${this.assistant.id}`),this.debouncer=void 0}catch(e){Wt.alert(`Update Error: ${e}`)}},this.validate=()=>this.assistant.name?!this.assistant.service||!this.assistant.model?(this.error="Model is required.",this.status="error",!1):!0:(this.error="Name is required.",this.status="error",!1)}async updated(e){super.updated(e),await this.updateComplete,e.has("key")&&await this.loadAsync()}render(){return v`
      <!-- Preview -->
      <div class="preview">
        <chat-room 
          .assistantId=${this.assistant.id||""}
        ></chat-room>
      </div>

      <sl-divider vertical></sl-divider>

      <!-- Form -->
      <div class="form">

        <!-- Form-Header -->
        <div class="header">
          <div class="status">
          ${this.status==="updating"?v`
              <sl-spinner></sl-spinner>
              <span>Updating...</span>`:this.status==="updated"?v`<sl-icon name="check-lg"></sl-icon>
              <span>Updated ${kf(this.assistant.lastUpdatedAt)}</span>`:this.status==="error"?v`<sl-icon name="exclamation-octagon"></sl-icon>
              <span>${this.error}</span>`:v`
              <sl-icon name="plus-circle"></sl-icon>
              <span>New Assistant</span>`}
          </div>
          <div class="flex"></div>
          <div class="control">
            <sl-button
              size="small"
              ?disabled=${this.status==="updating"}
              @click=${this.onUpdate}>
              Update
            </sl-button>
          </div>
        </div>
        
        <!-- Form-Body -->
        <div class="body">
          <model-select
            type="chat"
            label="Model"
            name="service-model"
            required
            .value=${`${this.assistant.service}/${this.assistant.model}`}
            @change=${this.onChange}
          ></model-select>
          <sl-input
            label="Name"
            name="name"
            size="small"
            required
            .value=${this.assistant.name||""}
            @sl-change=${this.onChange}
          ></sl-input>
          <sl-textarea
            label="Description"
            name="description"
            size="small"
            rows="2"
            .value=${this.assistant.description||""}
            @sl-change=${this.onChange}
          ></sl-textarea>
          <sl-textarea
            label="Instruction"
            name="instruction"
            size="small"
            rows="4"
            .value=${this.assistant.instruction||""}
            @sl-change=${this.onChange}
          ></sl-textarea>
          <div class="label">
            Tools
          </div>
          <checkbox-option
            label="Vector Search"
            name="tools.vector_search"
            ?checked=${this.assistant.tools.includes("vector_search")}
            @change=${this.onChange}>
            ${Kr(Dt.Memory.findCollections().then(e=>{var t,s;return v`
              <sl-select
                size="small"
                ?hoist=${!0}
                ?multiple=${!0}
                ?clearable=${!0}
                name="toolOptions.vector_search"
                value=${((s=(t=this.assistant.toolOptions)==null?void 0:t.vector_search)==null?void 0:s.join(" "))||""}
                @sl-change=${this.onChange}>
                ${e.map(i=>v`
                  <sl-option value=${i.id}>
                    ${i.name}
                  </sl-option>
                `)}
              </sl-select>
            `}),v`
              <sl-skeleton 
                effect="pulse"
                style="width: 100%; height: 30px;"
              ></sl-skeleton>
            `)}
          </checkbox-option>
          <div class="label">
            Options
          </div>
          <sl-input
            type="number"
            label="Max Tokens"
            name="options.maxTokens"
            size="small"
            .value=${this.assistant.options.maxTokens.toString()}
            @sl-change=${this.onChange}
          ></sl-input>
          <sl-input
            type="number"  
            label="Temperature"
            name="options.temperature"
            size="small"
            .value=${this.assistant.options.temperature.toString()}
            @sl-change=${this.onChange}
          ></sl-input>
          <sl-input
            type="number"
            label="Top K"
            name="options.topK"
            size="small"
            .value=${this.assistant.options.topK.toString()}
            @sl-change=${this.onChange}
          ></sl-input>
          <sl-input
            type="number"
            label="Top P"
            name="options.topP"
            size="small"
            .value=${this.assistant.options.topP.toString()}
            @sl-change=${this.onChange}
          ></sl-input>
        </div>
      </div>
    `}};Te.DefaultAssistant={name:"New Assistant",description:"This is a new assistant.",instruction:"You are helpful assistant.",tools:[],options:{maxTokens:2048,temperature:.7,topK:50,topP:.9}};Te.styles=A`
    :host {
      width: 100%;
      height: 100%;
      display: flex;
      flex-direction: row;
      overflow: hidden;
    }

    .preview {
      width: 50%;
      height: 100%;
    }

    sl-divider {
      margin: 0;
    }

    .form {
      width: 50%;
      height: 100%;
      
      .header {
        width: 100%;
        height: 40px;
        display: flex;
        flex-direction: row;
        align-items: center;
        justify-content: space-between;
        gap: 16px;
        padding: 4px 48px;
        box-sizing: border-box;
        font-size: 14px;

        .status {
          display: flex;
          gap: 8px;
          align-items: center;
        }

        .flex {
          flex: 1;
        }
      }

      .body {
        width: 100%;
        height: calc(100% - 40px);
        display: flex;
        flex-direction: column;
        align-items: center;
        padding: 16px;
        gap: 8px;
        box-sizing: border-box;
        overflow-y: auto;

        .label {
          text-align: left;
          font-size: 14px;
        }

        & > * {
          width: 90%;
        }
      }
    }
  `;Ri([d({type:String})],Te.prototype,"key",2);Ri([E()],Te.prototype,"assistant",2);Ri([E()],Te.prototype,"status",2);Ri([E()],Te.prototype,"error",2);Te=Ri([lt("assistant-editor")],Te);var dg=Object.defineProperty,hg=Object.getOwnPropertyDescriptor,Bl=(e,t,s,i)=>{for(var o=i>1?void 0:i?hg(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&dg(t,s,o),o};let ko=class extends tt{constructor(){super(...arguments),this.collections=[],this.loadAsync=async()=>await Dt.Memory.findCollections(),this.onDelete=async e=>{var s;const t=e.detail;await Dt.Memory.deleteCollection(t),this.collections=(s=this.collections)==null?void 0:s.filter(i=>i.id!==t)},this.onSelect=async e=>{const t=e.detail;xi(`/storage/${t}`)}}connectedCallback(){super.connectedCallback(),this.loadAsync().then(e=>this.collections=e)}render(){var e;return v`
      <div class="container">
        <div class="header">
          <sl-icon name="archive"></sl-icon>
          <span>Storage</span>
          <div class="flex"></div>
          <sl-icon name="question-circle"></sl-icon>
        </div>

        <sl-divider></sl-divider>

        <div class="control">
          <div class="flex"></div>
          <sl-button size="small" href="/storage">
            Create New
            <sl-icon slot="suffix" name="plus-lg"></sl-icon>
          </sl-button>
        </div>

        <div class="grid">
          ${(e=this.collections)==null?void 0:e.map(t=>v`
            <collection-card
              .collection=${t}
              @select=${this.onSelect}
              @delete=${this.onDelete}
            ></collection-card>
          `)}
        </div>
      </div>
    `}};ko.styles=A`
    :host {
      width: 100%;
      display: flex;
      padding: 64px 16px;
      justify-content: center;
      align-items: center;
      box-sizing: border-box;
    }

    .container {
      min-width: 700px;
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .header {
      width: 100%;
      display: flex;
      flex-direction: row;
      align-items: flex-end;
      justify-content: space-between;
      font-size: 28px;
      padding: 0px 16px;
      line-height: 32px;
      gap: 14px;
      box-sizing: border-box;

      span {
        font-size: 32px;
        font-weight: 600;
        color: var(--sl-color-gray-800);
      }

      .flex {
        flex: 1;
      }
    }

    .control {
      width: 100%;
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      gap: 16px;

      .flex {
        flex: 1;
      }
    }

    .grid {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }
  `;Bl([E()],ko.prototype,"collections",2);ko=Bl([lt("storage-explorer")],ko);var ug=Object.defineProperty,pg=Object.getOwnPropertyDescriptor,ra=(e,t,s,i)=>{for(var o=i>1?void 0:i?pg(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&ug(t,s,o),o};let ls=class extends tt{constructor(){super(...arguments),this.key="",this.collection=ls.DefaultCollection,this.onChange=e=>{const t=e.target,s=t.name.split(".");if(s[0]==="service-model"){const[i,o]=t.value.split("/");this.collection.embedService=i,this.collection.embedModel=o}else if(s[0]==="handlers")t.checked?this.collection.handlerOptions[s[1]]={}:delete this.collection.handlerOptions[s[1]];else if(s[0]==="handlerOptions")if(s[1]==="summary"||s[1]==="dialogue"){const[i,o]=t.value.split("/");this.collection.handlerOptions[s[1]]={serviceKey:i,modelName:o}}else s[1]==="max_tokens"?this.collection.handlerOptions.chunk[s[1]]=parseInt(t.value):this.collection.handlerOptions[s[1]]=t.value;else this.collection[s[0]]=t.value},this.onCancel=async()=>{window.history.back()},this.onSubmit=async e=>{const t=e.target;if(!t.loading)try{if(t.loading=!0,!this.validate())return;const s=await Dt.Memory.upsertCollection(this.collection);xi(`/storage/${s.id}`)}catch(s){Wt.alert(s)}finally{t.loading=!1}}}render(){return v`
      <div class="form">
        <sl-input
          label="Name"
          name="name"
          size="small"
          required
          .value=${this.collection.name||""}
          @sl-change=${this.onChange}
        ></sl-input>
        <sl-textarea
          label="Description"
          name="description"
          size="small"
          required
          rows="2"
          help-text="Provide a clear and concise description of the storage to help AI assistants understand its purpose."
          .value=${this.collection.description||""}
          @sl-change=${this.onChange}
        ></sl-textarea>
        <model-select
          label="Embedding Model"
          name="service-model"
          size="small"
          required
          .value=${this.getModelValue(this.collection)}
          @change=${this.onChange}
        ></model-select>
        <div class="label">
          Options
        </div>
        <checkbox-option
          label="Chunk Data"
          ?checked=${!0}
          ?disabled=${!0}
          help-text="Chunk data into smaller pieces by token count.">
          <sl-input
            type="number"
            name="handlerOptions.max_tokens"
            label="Max Chunk Token Size"
            size="small"
            value=${this.collection.handlerOptions.chunk.maxTokens||0}
            required
            @sl-change=${this.onChange}
          ></sl-input>
        </checkbox-option>
        <checkbox-option
          label="Generate Summary"
          name="handlers.summary"
          ?checked=${this.collection.handlerOptions.summary!==void 0}
          help-text="Generate a summary for each chunk data"
          @change=${this.onChange}>
          <model-select
            type="chat"
            name="handlerOptions.summary"
            label="Text Model"
            size="small"
            required
            .value=${this.getModelValue(this.collection.handlerOptions.summary)}
            @change=${this.onChange}
          ></model-select>
        </checkbox-option>
        <checkbox-option
          label="Generate QnA"
          name="handlers.dialogue"
          ?checked=${this.collection.handlerOptions.dialogue!==void 0}
          help-text="Generate QnA pairs for each chunk data"
          @change=${this.onChange}>
          <model-select
            type="chat"
            name="handlerOptions.dialogue"
            label="Text Model"
            size="small"
            required
            .value=${this.getModelValue(this.collection.handlerOptions.dialogue)}
            @change=${this.onChange}
          ></model-select>
        </checkbox-option>
        <div class="control">
          <sl-button 
            @click=${this.onCancel}>
            Cancel
          </sl-button>
          <sl-button 
            variant="primary" 
            @click=${this.onSubmit}>
            Confirm
          </sl-button>
        </div>
      </div>
    `}validate(){return this.collection.name?this.collection.description?!this.collection.embedService||!this.collection.embedModel?(Wt.alert("Embedding Model is required","warning"),!1):this.collection.handlerOptions.chunk.maxTokens?this.collection.handlerOptions.summary&&(!this.collection.handlerOptions.summary.serviceKey||!this.collection.handlerOptions.summary.modelName)?(Wt.alert("Summary Model is required","warning"),!1):this.collection.handlerOptions.dialogue&&(!this.collection.handlerOptions.dialogue.serviceKey||!this.collection.handlerOptions.dialogue.modelName)?(Wt.alert("Dialogue Model is required","warning"),!1):!0:(Wt.alert("Max Chunk Token Size is required","warning"),!1):(Wt.alert("Description is required","warning"),!1):(Wt.alert("Name is required","warning"),!1)}getModelValue(e){return e?e.embedService&&e.embedModel?`${e.embedService}/${e.embedModel}`:e.serviceKey&&e.modelName?`${e.serviceKey}/${e.modelName}`:"":""}};ls.DefaultCollection={name:"",description:"",handlerOptions:{chunk:{maxTokens:2048}}};ls.styles=A`
    :host {
      width: 100%;
      display: flex;
      padding: 64px 16px;
      justify-content: center;
      align-items: center;
      box-sizing: border-box;
    }

    .form {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .label {
      display: contents;
      font-size: 14px;
    }

    .control {
      width: 100%;
      display: flex;
      flex-direction: row;
      gap: 16px;

      sl-button {
        width: 100%;
      }
    }
  `;ra([d({type:String})],ls.prototype,"key",2);ra([E()],ls.prototype,"collection",2);ls=ra([lt("storage-editor")],ls);var fg=Object.defineProperty,mg=Object.getOwnPropertyDescriptor,bs=(e,t,s,i)=>{for(var o=i>1?void 0:i?mg(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&fg(t,s,o),o};let Me=class extends tt{constructor(){super(...arguments),this.key="",this.documents=[],this.openPanel=!1,this.openUpload=!1,this.loadCollectionAsync=async e=>await Dt.Memory.getCollection(e),this.loadDocumentAsync=async e=>await Dt.Memory.findDocuments(e),this.onUpload=async e=>{const t=e.target,s=e.detail;(!s||s.length===0)&&Wt.alert("File not selected","neutral");const i=new Ps;i.label=`Uploading...
 Do not close this window`,document.body.append(i),await Dt.Memory.uploadDocument(this.key,s,[],o=>{i.value=o*100}),i.remove(),t.files=[],this.openUpload=!1,this.documents=await this.loadDocumentAsync(this.key)}}connectedCallback(){super.connectedCallback(),this.loadCollectionAsync(this.key).then(e=>{this.collection=e}),this.loadDocumentAsync(this.key).then(e=>{console.log(e),this.documents=e})}render(){var e,t;return v`
      <div class="header">
        <div class="name">
          ${(e=this.collection)==null?void 0:e.name}
        </div>
        <div class="description">
          ${(t=this.collection)==null?void 0:t.description}
        </div>
      </div>

      <sl-divider></sl-divider>

      <div class="control">
        <sl-button size="small"
          @click=${()=>this.openUpload=!0}>
          Upload
        </sl-button>
        <div class="flex"></div>
        <sl-input 
          type="search"
          placeholder="Search" 
          size="small"
        ></sl-input>
        <sl-button size="small"> 
          Delete
        </sl-button>
        <sl-button size="small"
          @click=${()=>this.openPanel=!this.openPanel}>
          Panel
        </sl-button>
      </div>

      <div class="body" style="position: relative;">
        <div class="list">
          ${this.documents.map(s=>v`
            <li>
              <span>${s.fileName}</span>
              <span>${s.fileSize} Byte</span>
            </li>
          `)}
        </div>

        <div class="panel">
          Hello
        </div>
      </div>

      <file-uploader
        ?open=${this.openUpload}
        @upload=${this.onUpload}
        @close=${()=>this.openUpload=!1}
      ></file-uploader>
    `}};Me.styles=A`
    :host {
      width: 100%;
      height: 100%;
      display: flex;
      flex-direction: column;
      padding: 32px 32px;
      box-sizing: border-box;
    }
    :host([openPanel]) .panel {
      display: block;
    }

    .header {
      display: flex;
      flex-direction: column;
      gap: 8px;
      height: 60px;
    }

    .control {
      display: flex;
      flex-direction: row;
      gap: 8px;
      height: 40px;

      .flex {
        flex: 1;
      }
    }

    .body {
      width: 100%;
      position: relative;
      display: flex;
      flex-direction: row;
      gap: 8px;
      height: calc(100% - 100px);

      .list {
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: 8px;
        overflow-y: auto;
        height: 100%;
        border: 1px solid var(--sl-panel-border-color);

        li {
          display: flex;
          flex-direction: row;
          justify-content: space-between;
          padding: 8px;
          background-color: var(--sl-panel-background-color);
          border-radius: var(--sl-border-radius-medium);
        }
      }

      .panel {
        width: 50%;
        display: none;
        position: absolute;
        top: 0;
        right: -50%;
        height: 100%;
        background-color: var(--sl-panel-background-color);
        border-left: 1px solid var(--sl-panel-border-color);
        box-shadow: -1px 0 0 0 var(--sl-panel-border-color);
      }
    }
  `;bs([d({type:String})],Me.prototype,"key",2);bs([E()],Me.prototype,"collection",2);bs([E()],Me.prototype,"documents",2);bs([E()],Me.prototype,"document",2);bs([d({type:Boolean,reflect:!0})],Me.prototype,"openPanel",2);bs([d({type:Boolean,reflect:!0})],Me.prototype,"openUpload",2);Me=bs([lt("storage-viewer")],Me);var gg=Object.defineProperty,bg=Object.getOwnPropertyDescriptor,Ul=(e,t,s,i)=>{for(var o=i>1?void 0:i?bg(t,s):t,r=e.length-1,a;r>=0;r--)(a=e[r])&&(o=(i?a(t,s,o):a(o))||o);return i&&o&&gg(t,s,o),o};let $o=class extends tt{constructor(){super(...arguments),this.router=new kc(this,[{path:"/",render:()=>v`<home-page></home-page>`},{path:"/storages",render:()=>v`<storage-explorer></storage-explorer>`},{path:"/storage",render:()=>v`<storage-editor></storage-editor>`},{path:"/storage/:id",render:({id:e})=>v`<storage-viewer .key=${e}></storage-viewer>`},{path:"/assistants",render:()=>v`<assistant-explorer></assistant-explorer>`},{path:"/assistant/:id?",render:({id:e})=>v`<assistant-editor .key=${e}></assistant-editor>`},{path:"/chat/:id",render:({id:e})=>v`<chat-room .key=${e}></chat-room>`},{path:"/user",render:()=>v`<user-page></user-page>`}],{fallback:{render:()=>v`<error-page status="404"></error-page>`}}),this.theme=Wt.getTheme(),this.goback=async()=>{window.history.back()},this.goforward=async()=>{window.history.forward()},this.toggleTheme=async()=>{Wt.getTheme()==="light"?(Wt.setTheme("dark"),this.theme="dark"):(Wt.setTheme("light"),this.theme="light")}}connectedCallback(){super.connectedCallback(),Wt.setTheme(this.theme)}render(){return v`
      <div class="side-bar">
        <a class="home" href="/">
          <sl-icon name="logo-word"></sl-icon>
        </a>
        <a class="menu" href="/storages">
          <sl-icon name="archive"></sl-icon>
          <span>Storage</span>
        </a>
        <a class="menu" href="/assistants">
          <sl-icon name="robot"></sl-icon>
          <span>Assistant</span>
        </a>
        <a class="menu" href="/">
          <sl-icon name="chat-left-dots"></sl-icon>
          <span>Chat Room</span>
        </a>
        <div class="chat-list">
          ${Array.from({length:5}).map((e,t)=>v`
            <a class="sub-menu" href="/chat/${t}">
              <span>${t}th Chat Dummy</span>
            </a>
          `)}
        </div>
        <a class="menu" href="/user">
          <sl-icon name="person-gear"></sl-icon>
          <span>Profile</span>
        </a>
      </div>
      <div class="top-bar">
        <div class="left">
          <sl-icon-button 
            name="chevron-left"
            @click=${this.goback}
          ></sl-icon-button>
          <sl-icon-button
            name="chevron-right"
            @click=${this.goforward}
          ></sl-icon-button>
        </div>
        <div class="right">
          <sl-icon-button
            name=${this.theme==="dark"?"sun":"moon"}
            @click=${this.toggleTheme}
          ></sl-icon-button>
        </div>
      </div>
      <div class="main">
        ${this.router.outlet()}
      </div>
    `}};$o.styles=A`
    :host {
      display: grid;
      grid-template-columns: 260px 1fr;
      grid-template-rows: 60px 1fr;
      width: 100vw;
      height: 100vh;
      overflow: hidden;

      color: var(--sl-color-neutral-1000);
    }

    .side-bar {
      grid-row: 1 / 3;
      grid-column: 1 / 2;
      background-color: var(--sl-color-gray-100);
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 8px;
      gap: 8px;
      box-sizing: border-box;

      a {
        width: 100%;
        display: flex;
        align-items: center;
        text-decoration: none;
        box-sizing: border-box;
      }

      .home {
        text-decoration: none;
        color: var(--sl-color-red-700);
        margin: 12px 0px;
        justify-content: center;

        sl-icon {
          width: 80%;
          height: 80%;
        }

        &:hover {
          color: var(--sl-color-red-600);
        }
      }

      .menu {
        flex-direction: row;
        padding: 12px 16px;
        gap: 12px;
        font-size: 18px;
        line-height: 1;
        border-radius: 4px;
        color: var(--sl-color-gray-600);

        &:hover {
          background-color: var(--sl-color-gray-200);
        }
        &:active {
          transform: scale(0.95);
        }
      }

      .chat-list {
        width: 100%;
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: 4px;
        overflow-x: hidden;
        overflow-y: auto;
        text-overflow: ellipsis;
        white-space: nowrap;
        scrollbar-width: thin;
      }

      .sub-menu {
        padding: 8px 8px 8px 46px;
        font-size: 14px;
        color: var(--sl-color-gray-500);
        border-radius: 4px;

        &:hover {
          background-color: var(--sl-color-gray-200);
        }
        &:active {
          transform: scale(0.95);
        }
      }
    }

    .top-bar {
      grid-row: 1 / 2;
      grid-column: 2 / 3;
      background-color: var(--sl-color-gray-50);
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      padding: 0 18px;
      box-sizing: border-box;

      sl-icon-button {
        font-size: 24px;
      }
    }

    .main {
      grid-row: 2 / 3;
      grid-column: 2 / 3;
      background-color: var(--sl-color-gray-50);
      display: block;
      box-sizing: border-box;
      overflow-y: auto;
    }

  `;Ul([E()],$o.prototype,"theme",2);$o=Ul([lt("main-app")],$o);

import {Directive, ElementRef, OnInit, Renderer2} from "@angular/core";
import { setIframebackground } from '../modules/shared/utilities/utility-functions';

@Directive({
    selector: "[iframeAutoHeight]"
})
export class IframeAutoHeightDirective implements OnInit {
    private el: HTMLIFrameElement;
    private renderer: Renderer2;
    private prevHeight: number;
    private sameCount: number;

    constructor(private _elementRef: ElementRef, _renderer: Renderer2) {
        this.el = _elementRef.nativeElement;
        this.renderer = _renderer;
    }

    ngOnInit() {
        const self = this;
        if (this._elementRef.nativeElement.tagName === "IFRAME") {
            this.renderer.listen(this.el, "load", () => {
                self.prevHeight = 0;
                self.sameCount = 0;
                setTimeout(() => {
                    self.setHeight();
                }, 50);
            });
        }

        
    }

    setHeight() {
        const self = this;
        if (this.el.scrollHeight !== this.prevHeight) {
            //setIframebackground();
            //alert(this.el.contentWindow.parent.document.documentElement.scrollHeight);
            //alert(this.el.innerText);
            let iframeheight = document.getElementsByTagName('iframe')[0]?.style.height;
            this.sameCount = 0;
            this.prevHeight = (this.el.contentWindow?.parent.document.documentElement.scrollHeight) * 1.5;
            this.renderer.setStyle(
                self.el,
                "height",
                (this.el.contentWindow?.parent.document.documentElement.scrollHeight) / 1.5 + "px"
            );
            setTimeout(() => {
                self.setHeight();
            }, 50);

        } else {
            this.sameCount++;
            if (this.sameCount < 2) {
                setTimeout(() => {
                    self.setHeight();
                }, 50);
            }
        }
    }
}
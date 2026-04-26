import { HelpCircle } from 'lucide-react'

import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'

/**
 * Top-right "How to Use" button. Click → centered modal plays the demo video.
 * The video lives in /public/hero-demo.mp4 (already there for the homepage).
 */
export function HowToUseDialog() {
  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button variant="ghost" size="sm">
          <HelpCircle className="size-4 mr-1" />
          How to Use
        </Button>
      </DialogTrigger>
      <DialogContent className="max-w-3xl p-0 overflow-hidden">
        <DialogHeader className="p-6 pb-2">
          <DialogTitle>How Trade Analysis works</DialogTitle>
          <DialogDescription>
            Upload a chart, set your risk profile, and let the AI score the trade.
          </DialogDescription>
        </DialogHeader>
        <video
          src="/hero-demo.mp4"
          autoPlay
          muted
          loop
          controls
          playsInline
          className="w-full h-auto block object-cover"
        />
      </DialogContent>
    </Dialog>
  )
}

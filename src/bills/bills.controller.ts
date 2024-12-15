import { BillsService } from './bills.service';
import { Body, Controller, Get, Param, Post } from '@nestjs/common';
import { CreateBillDto } from './dto/create-bill.dto';

@Controller('bills')
export class BillsController {
  constructor(private billsService: BillsService) {}

  @Post('create-bill')
  createBill(@Body() createBillDto: CreateBillDto) {
    return this.billsService.createBill(createBillDto);
  }

  @Get(':userId')
  getBills(@Param('userId') userId: string) {
    const numericId = parseInt(userId, 10);
    return this.billsService.getBills(numericId);
  }
}
